using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModelScoutAPI.Models;
using MoreLinq;
using Telegram.Bot.Types;
using VkNet.Exception;
using VkNet.Model.Attachments;

namespace ModelScoutAPI {
    public class ModelScoutAPI {
        private ModelScoutAPIOptions _options;
        public ModelScoutAPIOptions Options {
            get { return _options; }
            set {
                _options = value;
                _dbOptionsBuilder = new DbContextOptionsBuilder<ModelScoutDbContext>()
                    .UseNpgsql(_options.DbConnectionString);
            }
        }

        public async Task<int> GetCountAcceptedVkClients(int vkAccId) {
            using var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options);
            return await db.VkClients.Where(e => e.ClientStatus == VkClient.Status.Accepted && e.VkAccId == vkAccId).CountAsync();
        }

        public async Task<int> GetCountCheckedVkClients(int vkAccId) {
            using var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options);
            return await db.VkClients.Where(e => e.ClientStatus == VkClient.Status.Checked && e.VkAccId == vkAccId).CountAsync();
        }
        private async Task IncrementAddedFriends(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                vkAcc.CountAddedFriends++;
                db.VkAccs.Update(vkAcc);
                await db.SaveChangesAsync();
            }
        }
        public async Task AddClientToFriends(VkClient client) {
            try {
                await VkApisManager.AddUserToFriends(client.VkAcc, client.ProfileVkId);
                await this.IncrementAddedFriends(client.VkAcc);
                await this.SetClientStatus(client.VkClientId, VkClient.Status.Ready);
            } catch (VkNet.Exception.CannotAddYouBlacklistedException) {
                await SetClientDeclined(client.VkClientId);
            } catch (CannotAddUserBlacklistedException) {
                await SetClientDeclined(client.VkClientId);
            } catch (UserAuthorizationFailException e) {
                Console.WriteLine(client.VkAcc.FirstName + " " + client.VkAcc.LastName);
                throw;
            } catch (Exception e) {
                if (await VkApisManager.CheckCountFriendRequests(client.VkAcc) == 10000)
                    await this.ClearFriends(client.VkAcc);
                else
                    await UpdateVkAccStatus(client.VkAcc);
            }
        }

        public async Task LikeClient(VkClient client) {
            try {
                await VkApisManager.RandomLikeUser(client.VkAcc, client.ProfileVkId);

            } catch (Exception e) {
                await UpdateVkAccStatus(client.VkAcc);
            } finally {
                await this.SetClientStatus(client.VkClientId, VkClient.Status.Liked);
            }
        }

        private async Task ClearFriends(VkAcc vkAcc) {
            if (await VkApisManager.ClearOutRequests(vkAcc) == 0)
                await VkApisManager.ClearFriends(vkAcc);
        }

        public async Task<VkClient> GetLikedClient(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                return await db.VkClients
                .Include(e => e.VkAcc)
                .FirstOrDefaultAsync(
                    e => e.ClientStatus == VkClient.Status.Liked
                    && e.VkAcc.VkAccId == vkAcc.VkAccId);
            }
        }
        public async Task<VkClient> GetAcceptedClient(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                return await db.VkClients
                .Include(e => e.VkAcc)
                .FirstOrDefaultAsync(
                    e => e.ClientStatus == VkClient.Status.Accepted
                    && e.VkAcc.VkAccId == vkAcc.VkAccId);
            }
        }

        private DbContextOptionsBuilder<ModelScoutDbContext> _dbOptionsBuilder;

        private Models.User _user;

        public Models.User User {
            get {
                if (_user != null)
                    _user = GetUserByTgChatId(_user.ChatId);
                return _user;
            }
            set { _user = value; }
        }

        public ModelScoutAPI(ModelScoutAPIOptions options, int TgChatId) {
            this.Options = options;
            this.User = GetUserByTgChatId(TgChatId);
        }

        public ModelScoutAPI(ModelScoutAPIOptions options) {
            this.Options = options;
        }
        public async Task SetVkAccStatus(VkAcc vkAcc, VkAcc.Status status) {
            using var db = new ModelScoutDbContext(_dbOptionsBuilder.Options);
            db.VkAccs
            .FirstOrDefault(e => e.VkAccId == vkAcc.VkAccId)
            .VkAccStatus = status;

            await db.SaveChangesAsync();
        }
        public async Task SetClientStatus(int id, VkClient.Status Status) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                db.VkClients
                .FirstOrDefault(e => e.VkClientId == id)
                .ClientStatus = Status;

                await db.SaveChangesAsync();
            }
        }
        public async Task SetClientAccepted(int id) {
            await SetClientStatus(id, VkClient.Status.Accepted);
        }

        public async Task SetClientDeclined(int id) {
            await SetClientStatus(id, VkClient.Status.Declined);
        }
        public async Task SetClientStatusError(int id) {
            await SetClientStatus(id, VkClient.Status.Declined);
        }

        private Models.User GetUserByTgChatId(long ChatId) {
            Models.User user;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                user = db.Users
                .Include(e => e.VkAccs)
                .ThenInclude(e => e.VkClients)
                .FirstOrDefault(e => e.ChatId == ChatId);
                if (user == null) {
                    db.Users.Add(user = new Models.User() { ChatId = ChatId });
                    db.SaveChanges();
                }

            }
            return user;
        }

        public async Task ClearCheckedClients() {
            foreach (var vkAcc in this.User.VkAccs)
                await this.ClearCheckedClients(vkAcc);
        }
        public async Task ClearUncheckedClients() {
            foreach (var vkAcc in this.User.VkAccs)
                await this.ClearUncheckedClients(vkAcc);
        }
        public async Task ClearAcceptedClients(VkAcc vkAcc) {
            using var db = new ModelScoutDbContext(_dbOptionsBuilder.Options);

            var clients = db.VkClients.Where(
                e => e.VkAccId == vkAcc.VkAccId
                && e.ClientStatus == VkClient.Status.Accepted);

            foreach (var client in clients)
                client.ClientStatus = VkClient.Status.Unchecked;

            await db.SaveChangesAsync();
        }
        public async Task ClearCheckedClients(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                var clients = db.VkClients.Where(e => e.VkAccId == vkAcc.VkAccId && e.ClientStatus == VkClient.Status.Checked);
                foreach (var client in clients) {
                    client.ClientStatus = VkClient.Status.Unchecked;
                }
                await db.SaveChangesAsync();
            }
        }
        public async Task ClearUncheckedClients(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                var clients = db.VkClients.Where(e => e.VkAccId == vkAcc.VkAccId && e.ClientStatus == VkClient.Status.Unchecked);
                db.VkClients.RemoveRange(clients);
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<VkClient>> GetUnchekedClientsForActivesVkAccs(int count) {
            var clients = new List<VkClient>();
            var vkAccs = this.User.VkAccs
                .Where(acc => acc.VkAccStatus == VkAcc.Status.Active)
                .ToList();
            var gettedCount = 0;

            //5vkAccs.RemoveAll(e => e.CountAddedFriends + GetCountAcceptedVkClients(e.VkAccId).Result >= e.FriendsLimit);


            while (gettedCount < count && vkAccs.Count > 0) {
                var maxVkAccs = vkAccs.OrderByDescending((e)
                    => {
                        return e.FriendsLimit
                    - e.CountAddedFriends
                     - GetCountAcceptedVkClients(e.VkAccId).Result
                      - GetCountCheckedVkClients(e.VkAccId).Result;
                    })
                    .ToList();
                var vkAcc = maxVkAccs.FirstOrDefault();
                var client = await this.GetUncheckedClient(vkAcc);
                if (client != null) {
                    client.ClientStatus = VkClient.Status.Checked;
                    await this.SetClientStatus(client.VkClientId, VkClient.Status.Checked);
                    clients.Add(client);
                    gettedCount++;
                } else {
                    vkAccs.Remove(vkAcc);
                }
            }

            return clients;
        }

        public async Task<List<InputMediaPhoto>> GetVkProfilePhotosMaxSizesAsync(VkClient vkClient) {
            VkNet.Utils.VkCollection<VkNet.Model.Attachments.Photo> Photos;

            Photos = await VkApisManager.GetProfilePhotosClient(vkClient);

            List<InputMediaPhoto> inputPhotos = new List<InputMediaPhoto>();

            foreach (var Photo in Photos) {
                var maxHeight = Photo.Sizes.Max(e => e.Height);
                var maxPhoto = Photo.Sizes.FirstOrDefault(e => e.Height == maxHeight);
                inputPhotos.Add(new InputMediaPhoto(new InputMedia(maxPhoto.Url.ToString())));
            }

            return inputPhotos;
        }

        public async Task<VkClient> GetUncheckedClient(VkAcc vkAcc) {
            VkClient vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
            if (vkClient == null) {
                await this.UpdateClients(vkAcc);
                vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
            }
            return vkClient;
        }

        private async Task UpdateClients(VkAcc vkAcc) {
            try {
                using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                    int Count = 200;
                    var acc = db.VkAccs.FirstOrDefault(e => e.VkAccId == vkAcc.VkAccId);
                    vkAcc.VkClients.AddRange(await VkApisManager.GetNewClients(
                        acc,
                         Count, this, db));
                    await db.SaveChangesAsync();
                }
            } catch (Exception) {
                await UpdateVkAccStatus(vkAcc);
            }


        }


        public async Task<List<Models.VkAcc>> GetVkAccs(bool NeedUpdate = false) {
            List<Models.VkAcc> vkAccs;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                if (User == null)
                    vkAccs = db.VkAccs
                        .Include(e => e.User)
                        .ToList();
                else
                    vkAccs = db.VkAccs
                        .Where(e => e.UserId == this.User.UserId)
                        .Include(e => e.User)
                        .ToList();
                if (NeedUpdate) {
                    foreach (var vkAcc in vkAccs)
                        await UpdateVkAccStatus(vkAcc);
                    await db.SaveChangesAsync();
                }
            }
            return vkAccs;

        }

        public async Task<Models.VkAcc> GetVkAcc(long VkAccId) {
            Models.VkAcc vkAcc;
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                vkAcc = db.VkAccs.FirstOrDefault(e => e.VkAccId == VkAccId);
                await UpdateVkAccStatus(vkAcc);
                await db.SaveChangesAsync();
            }
            return vkAcc;

        }

        public async Task<VkAcc> CreateVkAcc(string Token) {
            VkAcc vkAcc;
            var result = await VkApisManager.TryAuthorize(Token);
            if (result == true) {
                using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                    var user = await db.Users.FirstOrDefaultAsync(e => e.UserId == User.UserId);
                    vkAcc = new VkAcc() {
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
        public async Task SaveVkAcc(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                db.VkAccs.Update(vkAcc);
                await db.SaveChangesAsync();
            }
        }
        public async Task RemoveVkAcc(VkAcc vkAcc) {
            using (var db = new ModelScoutDbContext(this._dbOptionsBuilder.Options)) {
                var user = await db.Users
                    .Include(e => e.VkAccs)
                    .FirstOrDefaultAsync(e => e.UserId == User.UserId);
                user.VkAccs.RemoveAll(e => e.VkAccId == vkAcc.VkAccId);

                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> ClearLimitsOnceAtDay() {
            using var db = new ModelScoutDbContext(_dbOptionsBuilder.Options);
            var miscInfo = await db.MiscInfos.FirstOrDefaultAsync();
            if (miscInfo == null) {
                miscInfo = new MiscInfo();
                await db.MiscInfos.AddAsync(miscInfo);
            }
            if (miscInfo.LastDateOfClearLimits.Date + TimeSpan.FromHours(6 + 24) < DateTime.Now) {
                var accs = await db.VkAccs.ToListAsync();

                foreach (var acc in accs)
                    acc.CountAddedFriends = 0;

                miscInfo.LastDateOfClearLimits = DateTime.Now;
                await db.SaveChangesAsync();
                return true;
            } else {
                return false;
            }
        }
        public async Task<VkAcc> UpdateVkAccStatus(VkAcc vkAcc) {
            try {
                var profileInfo = await VkApisManager.GetProfileInfo(vkAcc);
                if (profileInfo != null) {
                    vkAcc.FirstName = profileInfo.FirstName;
                    vkAcc.LastName = profileInfo.LastName;
                    vkAcc.VkAccStatus = VkAcc.Status.Active;
                } else {
                    vkAcc.VkAccStatus = VkAcc.Status.Error;
                }
            } catch (Exception) {
                vkAcc.VkAccStatus = VkAcc.Status.Error;
            }
            return vkAcc;
        }
    }
}
