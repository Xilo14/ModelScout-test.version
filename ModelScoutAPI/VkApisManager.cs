using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModelScoutAPI.Models;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace ModelScoutAPI {
    public static class VkApisManager {
        public static ModelScoutAPIOptions modelScoutAPIOptions = null;
        public static ILogger<VkNet.VkApi> logger = null;
        static Dictionary<String, VkApi> cachedApis = new Dictionary<String, VkApi>();

        public static async Task<AccountSaveProfileInfoParams> GetProfileInfo(VkAcc vkAcc) {
            var api = _getApi(vkAcc);
            if (_isVkApiActive(api))
                return await api.Account.GetProfileInfoAsync();
            else
                return null;
        }
        public static VkNet.Model.User GetVkUser(VkClient vkClient, VkAcc vkAcc = null) {
            if (vkAcc == null)
                vkAcc = vkClient.VkAcc;

            var api = _getApi(vkAcc);
            var user = api.Users.Get(
                    new long[] { vkClient.ProfileVkId },
                    VkNet.Enums.Filters.ProfileFields.BirthDate | VkNet.Enums.Filters.ProfileFields.City)
                .FirstOrDefault();

            return user;

        }

        internal static async Task AddUserToFriends(VkAcc vkAcc, int profileVkId) {
            var api = _getApi(vkAcc);

            await api.Friends.AddAsync(
                userId: (long)profileVkId,
                text: "",
                follow: null
            );
        }

        public static async Task<long> CheckCountFriendRequests(VkAcc vkAcc) {
            var api = _getApi(vkAcc);

            var friendsCount = (await api.Friends.GetAsync(
                new FriendsGetParams() { Count = 0 })).Count;
            var requestCount = (await api.Friends.GetRequestsAsync(
                new FriendsGetRequestsParams() { Count = 0, Out = true })).Count;

            return friendsCount + requestCount;
        }

        public static async Task<int> ClearFriends(VkAcc vkAcc) {
            var api = _getApi(vkAcc);

            var friends = (await api.Friends.GetAsync(
                new FriendsGetParams() { Count = 1000, Order = FriendsOrder.Random }
            )).ToList();
            var recentFriends = await api.Friends.GetRecentAsync(1000);

            friends.RemoveAll(e => recentFriends.Contains(e.Id));

            var deletedCount = 0;
            foreach (var friend in friends) {
                var result = await api.Friends.DeleteAsync((long)friend.Id);
                if (result.Success == true && result.FriendDeleted == true)
                    deletedCount++;
            }

            return deletedCount;
        }
        public static async Task<int> ClearOutRequests(VkAcc vkAcc) {
            var api = _getApi(vkAcc);

            var outRequestCount = (await api.Friends.GetRequestsAsync(
                new FriendsGetRequestsParams() { Count = 0, Out = true })).Count;

            var offset = outRequestCount - 1000;
            if (offset < 0)
                offset = 0;

            var outRequests = await api.Friends.GetRequestsExtendedAsync(
                new FriendsGetRequestsParams() { Count = 1000, Out = true, Offset = offset });

            var deletedCount = 0;
            foreach (var outReq in outRequests)
                if (outReq.UserId != null) {
                    var result = await api.Friends.DeleteAsync((long)outReq.UserId);
                    if (result.Success == true && result.OutRequestDeleted == true)
                        deletedCount++;
                }

            return deletedCount;

        }
        internal static async Task RandomLikeUser(VkAcc vkAcc, int profileVkId) {
            var rand = new Random();
            var api = _getApi(vkAcc);


            var Photos = await api.Photo.GetAsync(new PhotoGetParams() {
                OwnerId = profileVkId,
                AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                Reversed = true,
                Count = 10
            });

            var countLikes = rand.Next(1, 3);
            if (countLikes > Photos.Count)
                countLikes = Photos.Count;
            var PhotosNeedLikes = Photos.OrderBy(s => rand.Next()).Take(countLikes).ToList();
            foreach (var photo in PhotosNeedLikes) {
                await Task.Delay(rand.Next(12000, 24000));
                await api.Likes.AddAsync(new LikesAddParams {
                    Type = VkNet.Enums.SafetyEnums.LikeObjectType.Photo,
                    ItemId = photo.Id.Value,
                    OwnerId = photo.OwnerId,
                });
            }
        }

        public static async Task<VkCollection<Photo>> GetProfilePhotosClient(VkClient vkClient, VkAcc vkAcc = null) {
            if (vkAcc == null)
                vkAcc = vkClient.VkAcc;
            var api = _getApi(vkAcc);
            var Photos = await api.Photo.GetAsync(new PhotoGetParams() {
                OwnerId = vkClient.ProfileVkId,
                AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                Reversed = true,
                Count = 10,
                PhotoSizes = true,
            });
            return Photos;

        }
        static VkApi _getApi(VkAcc vkAcc) {
            if (!cachedApis.ContainsKey(vkAcc.AccessToken)) {
                VkApi vkApi;
                if (VkApisManager.modelScoutAPIOptions != null)
                    vkApi = new VkApi(logger, new CaptchaSolvers.CptchCaptchaSolver(
                        modelScoutAPIOptions.CptchApiKey,
                        modelScoutAPIOptions.CptchSoftId));
                else
                    vkApi = new VkApi(logger);

                // new CaptchaSolvers.CptchCaptchaSolver()
                vkApi.Authorize(new ApiAuthParams {
                    AccessToken = vkAcc.AccessToken
                });
                cachedApis.Add(vkAcc.AccessToken, vkApi);
            }
            return cachedApis[vkAcc.AccessToken];
        }

        internal static async Task<List<VkClient>> GetNewClients(
            VkAcc vkAcc, int Count, ModelScoutAPI api,
             ModelScoutDbContext context) {
            uint updatedCount = 0;
            uint offset = 0;

            List<VkClient> vkClients = new List<VkClient>();

            var vkApi = _getApi(vkAcc);

            if (!_isVkApiActive(vkApi))
                return null;

            while (updatedCount < Count) {
                var clientsSearch = await vkApi.Users.SearchAsync(new UserSearchParams() {
                    City = vkAcc.City,
                    Country = vkAcc.Country,
                    AgeFrom = (ushort)vkAcc.AgeFrom,
                    AgeTo = (ushort)vkAcc.AgeTo,
                    Sex = (VkNet.Enums.Sex)vkAcc.Sex,
                    BirthMonth = (ushort)vkAcc.BirthMonth,
                    BirthDay = (ushort)vkAcc.BirthDay,
                    Online = true,
                    HasPhoto = true,
                    Count = (uint)Count,
                    Offset = offset,
                    Fields = ProfileFields.City,

                });

                foreach (var clientSearch in clientsSearch) {
                    //BAD CODE
                    if (!context.VkClients.Any(e => e.ProfileVkId == clientSearch.Id)
                    && clientSearch.IsClosed != true
                    && (clientSearch.City == null || clientSearch.City?.Id == vkAcc.City)
                    )
                    //BAD CODE
                    {
                        var vkClient = new VkClient() {
                            ClientStatus = VkClient.Status.Unchecked,
                            VkAccId = vkAcc.VkAccId,
                            ProfileVkId = (int)clientSearch.Id
                        };
                        //BAD CODE
                        vkAcc.VkClients.Add(vkClient);
                        //BAD CODE
                        vkClients.Add(vkClient);
                        updatedCount++;
                    }

                }

                offset += (uint)Count;

                if (clientsSearch.TotalCount < offset)
                    break;
            }
            return vkClients;
        }

        // public static async Task<VkClient> GetUncheckedClient(VkAcc vkAcc)
        // {
        //     VkClient vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
        //     if (vkClient == null)
        //     {
        //         await UpdateClientsFromVkBySearchconfig(vkAcc);
        //         vkClient = vkAcc.VkClients.Find(e => e.ClientStatus == VkClient.Status.Unchecked);
        //     }
        //     return vkClient;

        // }
        // public static async Task<int> UpdateClientsFromVkBySearchconfig(VkAcc vkAcc, uint WantedCount = 200)
        // {
        //     uint UpdatedCount = 0;
        //     uint Offset = 0;

        //     var vkApi = _getApi(vkAcc);
        //     if (_isVkApiActive(vkApi))
        //     {

        //         using (var context = new AutoVkContext())
        //         {
        //             while (UpdatedCount < WantedCount)
        //             {
        //                 var clientsSearch = vkApi.Users.Search(new UserSearchParams()
        //                 {
        //                     City = vkAcc.City,
        //                     Country = vkAcc.Country,
        //                     AgeFrom = (ushort)vkAcc.AgeFrom,
        //                     AgeTo = (ushort)vkAcc.AgeTo,
        //                     Sex = (VkNet.Enums.Sex)vkAcc.Sex,
        //                     BirthMonth = (ushort)vkAcc.BirthMonth,
        //                     BirthDay = (ushort)vkAcc.BirthDay,
        //                     Online = true,
        //                     HasPhoto = true,
        //                     Count = WantedCount,
        //                     Offset = Offset,

        //                 });

        //                 foreach (var clientSearch in clientsSearch)
        //                 {
        //                     if (!vkAcc.VkClients.Exists(e => e.ProfileVkId == clientSearch.Id) && clientSearch.IsClosed != true)
        //                     {
        //                         var vkClient = new VkClient()
        //                         {
        //                             ClientStatus = VkClient.Status.Unchecked,
        //                             VkAccId = vkAcc.VkAccId,
        //                             ProfileVkId = (int)clientSearch.Id
        //                         };
        //                         vkAcc.VkClients.Add(vkClient);
        //                         context.VkClients.Add(vkClient);
        //                         UpdatedCount++;
        //                     }
        //                 }

        //                 Offset += WantedCount;

        //                 if (clientsSearch.TotalCount < Offset)
        //                 {
        //                     context.SaveChanges();
        //                     return (int)UpdatedCount;
        //                 }
        //             }
        //             context.SaveChanges();
        //         }
        //         return (int)UpdatedCount;
        //     }
        //     return -1;

        // }
        public static async Task<Boolean> TryAuthorize(string accessToken) {
            var vkApi = new VkApi();
            await vkApi.AuthorizeAsync(new ApiAuthParams {
                AccessToken = accessToken
            });
            if (_isVkApiActive(vkApi)) {
                return true;
            }
            return false;

        }
        // public static async Task LikeAndAddToFriends(VkClient vkClient, VkAcc vkAcc = null)
        // {
        //     using (var context = new AutoVkContext())
        //     {
        //         if (vkAcc == null)
        //             vkAcc = context.VkAccs.FirstOrDefault(e => e.VkAccId == vkClient.VkAccId);

        //         var rand = new Random();
        //         var api = _getApi(vkAcc);

        //         vkClient = context.VkClients.FirstOrDefault(e => e.VkClientId == vkClient.VkClientId);

        //         var Photos = await api.Photo.GetAsync(new PhotoGetParams()
        //         {
        //             OwnerId = vkClient.ProfileVkId,
        //             AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
        //             Reversed = true,
        //             Count = 10
        //         });

        //         var countLikes = rand.Next(1, 3);
        //         if (countLikes > Photos.Count)
        //             countLikes = Photos.Count;
        //         var PhotosNeedLikes = Photos.OrderBy(s => rand.Next()).Take(countLikes).ToList();
        //         foreach (var photo in PhotosNeedLikes)
        //         {
        //             Thread.Sleep(rand.Next(3000, 8000));
        //             api.Likes.Add(new LikesAddParams
        //             {
        //                 Type = VkNet.Enums.SafetyEnums.LikeObjectType.Photo,
        //                 ItemId = photo.Id.Value,
        //                 OwnerId = photo.OwnerId,
        //             });
        //         }
        //         vkClient.ClientStatus = VkClient.Status.Liked;
        //         context.SaveChanges();

        //         Thread.Sleep(rand.Next(3000, 8000));
        //         api.Friends.Add(
        //             userId: (long)vkClient.ProfileVkId,
        //             text: "",
        //             follow: null
        //         );
        //         vkClient.ClientStatus = VkClient.Status.Ready;
        //         context.SaveChanges();
        //     }
        // }
        private static bool _isVkApiActive(VkApi vkApi) {
            if (vkApi.IsAuthorized) {
                try {
                    vkApi.Account.GetProfileInfo();
                    return true;
                } catch (AccessTokenInvalidException ex) {
                    return false;
                } catch (UserAuthorizationFailException ex) {
                    return false;
                }

            }
            return false;
        }
    }
}
