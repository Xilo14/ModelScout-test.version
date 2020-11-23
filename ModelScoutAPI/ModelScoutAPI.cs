using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelScoutAPI.Models;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace ModelScoutAPI
{
    public class ModelScoutAPI
    {
        private ModelScoutAPIOptions _options;
        public ModelScoutAPIOptions Options
        {
            get { return _options; }
            set
            {
                _options = value;
                _dbOptionsBuilder = new DbContextOptionsBuilder<ModelScoutDbContext>()
                    .UseNpgsql(_options.DbConnectionString);
            }
        }
        private DbContextOptionsBuilder<ModelScoutDbContext> _dbOptionsBuilder;

        public ModelScoutAPI(ModelScoutAPIOptions options)
        {
            this.Options = options;
        }

        public async Task<Models.User> GetUserByTgChatId(int ChatId)
        {
            Models.User user;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                user = await db.Users.FirstOrDefaultAsync(e => e.ChatId == ChatId);
            }
            return user;
        }

        public async Task<List<Models.VkAcc>> GetVkAccs(Models.User User)
        {
            List<Models.VkAcc> vkAccs;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                vkAccs = db.VkAccs.Where(e => e.UserId == User.UserId).ToList();
                foreach (var vkAcc in vkAccs)
                {
                    var profileInfo = VkApisManager.GetProfileInfo(vkAcc);
                    if (profileInfo != null)
                    {
                        vkAcc.FirstName = profileInfo.FirstName;
                        vkAcc.LastName = profileInfo.LastName;
                    }
                    else
                    {
                        //TODO: Check status of acc
                    }
                }
                await db.SaveChangesAsync();
            }
            return vkAccs;

        }

        public async Task<Boolean> CreateVkAcc(Models.User User, string Token)
        {
            var result = VkApisManager.TryAuthorize(Token);
            if (result == true)
            {
                using(var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
                {
                    var user = await db.Users.FirstOrDefaultAsync(e => e.UserId == User.UserId);
                    User.VkAccs.Add(db.VkAccs.)
                }
            }
        }
    }
}