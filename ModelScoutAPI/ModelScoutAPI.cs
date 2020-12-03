using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelScoutAPI.Models;
using Telegram.Bot.Types;
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

        private async Task IncrementAddedFriends(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                vkAcc.CountAddedFriends++;
                db.VkAccs.Update(vkAcc);
                await db.SaveChangesAsync();
            }
        }
        public async Task AddClientToFriends(VkClient client)
        {
            try
            {
                await VkApisManager.AddUserToFriends(client.VkAcc, client.ProfileVkId);
                await this.IncrementAddedFriends(client.VkAcc);
                await this.SetClientStatus(client.VkClientId, VkClient.Status.Ready);
            }
            catch (Exception e)
            {
                await this.ClearFriends(client.VkAcc);
            }
        }

        public async Task LikeClient(VkClient client)
        {
            try
            {
                await VkApisManager.RandomLikeUser(client.VkAcc, client.ProfileVkId);
                await this.SetClientStatus(client.VkClientId, VkClient.Status.Liked);
            }
            catch (Exception e)
            {
                Console.WriteLine("");
            }
        }

        private async Task ClearFriends(VkAcc vkAcc)
        {
            throw new NotImplementedException();
        }

        public async Task<VkClient> GetLikedClient(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                return await db.VkClients
                .Include(e => e.VkAcc)
                .FirstOrDefaultAsync(
                    e => e.ClientStatus == VkClient.Status.Liked
                    && e.VkAcc.VkAccId == vkAcc.VkAccId);
            }
        }
        public async Task<VkClient> GetAcceptedClient(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                return await db.VkClients
                .Include(e => e.VkAcc)
                .FirstOrDefaultAsync(
                    e => e.ClientStatus == VkClient.Status.Accepted
                    && e.VkAcc.VkAccId == vkAcc.VkAccId);
            }
        }

        private DbContextOptionsBuilder<ModelScoutDbContext> _dbOptionsBuilder;

        private Models.User _user;

        public Models.User User
        {
            get
            {
                if (_user != null)
                    _user = _getUserByTgChatId(_user.ChatId);
                return _user;
            }
            set { _user = value; }
        }

        public ModelScoutAPI(ModelScoutAPIOptions options, int TgChatId)
        {
            this.Options = options;
            this.User = _getUserByTgChatId(TgChatId);
        }

        public ModelScoutAPI(ModelScoutAPIOptions options)
        {
            this.Options = options;
        }

        public async Task SetClientStatus(int id, VkClient.Status Status)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                db.VkClients
                .FirstOrDefault(e => e.VkClientId == id)
                .ClientStatus = Status;

                await db.SaveChangesAsync();
            }
        }
        public async Task SetClientAccepted(int id)
        {
            await SetClientStatus(id, VkClient.Status.Accepted);
        }

        public async Task SetClientDeclined(int id)
        {
            await SetClientStatus(id, VkClient.Status.Declined);
        }
        public async Task SetClientStatusError(int id)
        {
            await SetClientStatus(id, VkClient.Status.Declined);
        }

        private Models.User _getUserByTgChatId(long ChatId)
        {
            Models.User user;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                user = db.Users
                .Include(e => e.VkAccs)
                .ThenInclude(e => e.VkClients)
                .FirstOrDefault(e => e.ChatId == ChatId);
                if (user == null)
                {
                    db.Users.Add(user = new Models.User() { ChatId = ChatId });
                    db.SaveChanges();
                }

            }
            return user;
        }

        public async Task ClearCheckedClients()
        {
            foreach (var vkAcc in this.User.VkAccs)
                await this.ClearCheckedClients(vkAcc);
        }
        public async Task ClearUncheckedClients()
        {
            foreach (var vkAcc in this.User.VkAccs)
                await this.ClearUncheckedClients(vkAcc);
        }

        public async Task ClearCheckedClients(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                var clients = db.VkClients.Where(e => e.VkAccId == vkAcc.VkAccId && e.ClientStatus == VkClient.Status.Checked);
                foreach (var client in clients)
                {
                    client.ClientStatus = VkClient.Status.Unchecked;
                }
                await db.SaveChangesAsync();
            }
        }
        public async Task ClearUncheckedClients(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                var clients = db.VkClients.Where(e => e.VkAccId == vkAcc.VkAccId && e.ClientStatus == VkClient.Status.Unchecked);
                db.VkClients.RemoveRange(clients);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<VkClient>> GetUnchekedClientsForActivesVkAccs(int count)
        {
            List<VkClient> clients = new List<VkClient>();
            for (int i = 0; i < count; i++)
            {
                var vkAcc = this.User.VkAccs.Max();
                var client = await this.GetUncheckedClient(vkAcc);
                if (client != null)
                    client.ClientStatus = VkClient.Status.Checked;
                await this.SetClientStatus(client.VkClientId, VkClient.Status.Checked);
                clients.Add(client);
            }

            return clients;
        }



        public async Task<List<InputMediaPhoto>> GetVkProfilePhotosMaxSizesAsync(VkClient vkClient)
        {
            VkNet.Utils.VkCollection<VkNet.Model.Attachments.Photo> Photos;

            Photos = await VkApisManager.GetProfilePhotosClient(vkClient);

            List<InputMediaPhoto> inputPhotos = new List<InputMediaPhoto>();

            foreach (var Photo in Photos)
            {
                var maxHeight = Photo.Sizes.Max(e => e.Height);
                var maxPhoto = Photo.Sizes.FirstOrDefault(e => e.Height == maxHeight);
                inputPhotos.Add(new InputMediaPhoto(new InputMedia(maxPhoto.Url.ToString())));
            }

            return inputPhotos;
        }

        public async Task<VkClient> GetUncheckedClient(VkAcc vkAcc)
        {
            VkClient vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
            if (vkClient == null)
            {
                await this.UpdateClients(vkAcc);
                vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
            }
            return vkClient;
        }

        private async Task UpdateClients(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                int Count = 50;
                var acc = db.VkAccs.FirstOrDefault(e => e.VkAccId == vkAcc.VkAccId);
                vkAcc.VkClients.AddRange(await VkApisManager.GetNewClients(
                    acc,
                     Count, this, db));
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Models.VkAcc>> GetVkAccs(bool NeedUpdate = false)
        {
            List<Models.VkAcc> vkAccs;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                if (User == null)
                    vkAccs = db.VkAccs
                        .Include(e => e.User)
                        .ToList();
                else
                    vkAccs = db.VkAccs
                        .Where(e => e.UserId == this.User.UserId)
                        .Include(e => e.User)
                        .ToList();
                if (NeedUpdate)
                {
                    foreach (var vkAcc in vkAccs)
                        await UpdateVkAccStatus(vkAcc);
                    await db.SaveChangesAsync();
                }
            }
            return vkAccs;

        }

        public async Task<Models.VkAcc> GetVkAcc(long VkAccId)
        {
            Models.VkAcc vkAcc;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                vkAcc = db.VkAccs.FirstOrDefault(e => e.VkAccId == VkAccId);
                await UpdateVkAccStatus(vkAcc);
                await db.SaveChangesAsync();
            }
            return vkAcc;

        }

        public async Task<VkAcc> CreateVkAcc(string Token)
        {
            VkAcc vkAcc;
            var result = await VkApisManager.TryAuthorize(Token);
            if (result == true)
            {
                using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
                {
                    var user = await db.Users.FirstOrDefaultAsync(e => e.UserId == User.UserId);
                    vkAcc = new VkAcc()
                    {
                        AccessToken = Token,
                        FriendsLimit = 40,
                        CountAddedFriends = 0,
                    };
                    await this.UpdateVkAccStatus(vkAcc);
                    user.VkAccs.Add(vkAcc);

                    await db.SaveChangesAsync();
                }
                return vkAcc;
            }
            return null;
        }
        public async Task SaveVkAcc(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                db.VkAccs.Update(vkAcc);
                await db.SaveChangesAsync();
            }
        }
        public async Task RemoveVkAcc(VkAcc vkAcc)
        {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options))
            {
                var user = await db.Users
                    .Include(e => e.VkAccs)
                    .FirstOrDefaultAsync(e => e.UserId == User.UserId);
                user.VkAccs.RemoveAll(e => e.VkAccId == vkAcc.VkAccId);

                await db.SaveChangesAsync();
            }
        }

        private async Task<VkAcc> UpdateVkAccStatus(VkAcc vkAcc)
        {
            //TODO: Check status of acc
            var profileInfo = await VkApisManager.GetProfileInfo(vkAcc);
            if (profileInfo != null)
            {
                vkAcc.FirstName = profileInfo.FirstName;
                vkAcc.LastName = profileInfo.LastName;
                //TODO: Set status of acc
            }
            else
            {
                //TODO: Set status of acc
            }
            return vkAcc;
        }
    }
}