using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModelScoutAPI.Models;
using VkNet;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace ModelScoutAPI {
    public class ModelScoutAPI {
        public ModelScoutAPIOptions Options { get; set; }


        public ModelScoutAPI(ModelScoutAPIOptions options){
            this.Options = options;
        }
        Dictionary<VkAcc, VkApi> cachedApis = new Dictionary<VkAcc, VkApi> ();

        public AccountSaveProfileInfoParams GetProfileInfo (VkAcc vkAcc) {
            var api = _getApi (vkAcc);
            if (_isVkApiActive (api))
                return api.Account.GetProfileInfo ();
            else
                return null;
        }
        public VkNet.Model.User GetVkUser (VkClient vkClient, VkAcc vkAcc = null) {
            if (vkAcc == null)
                vkAcc = vkClient.VkAcc;

            var api = _getApi (vkAcc);
            var user = api.Users.Get (
                    new long[] { vkClient.ProfileVkId },
                    VkNet.Enums.Filters.ProfileFields.BirthDate | VkNet.Enums.Filters.ProfileFields.City)
                .FirstOrDefault ();

            return user;

        }
        public async Task<VkCollection<Photo>> GetProfilePhotosClient (VkClient vkClient, VkAcc vkAcc = null) {
            if (vkAcc == null)
                vkAcc = vkClient.VkAcc;
            var api = _getApi (vkAcc);
            var Photos = api.Photo.Get (new PhotoGetParams () {
                OwnerId = vkClient.ProfileVkId,
                    AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                    Reversed = true,
                    Count = 10
            });
            return Photos;

        }
        VkApi _getApi (VkAcc vkAcc) {
            if (!cachedApis.ContainsKey (vkAcc)) {
                var vkApi = new VkApi (null, new CptchCaptchaSolver ());
                vkApi.Authorize (new ApiAuthParams {
                    AccessToken = vkAcc.AccessToken
                });
                cachedApis.Add (vkAcc, vkApi);
            }
            return cachedApis[vkAcc];
        }
        public static void UpdateAllAccsByUsers (User user) {
            foreach (var acc in user.VkAccs) {
                if (!_isVkApiActive (_getApi (acc))) {
                    user.VkAccs.Remove (acc);
                    cachedApis.Remove (acc);
                }
            }
        }
        public static async Task<VkClient> GetUncheckedClient (VkAcc vkAcc) {
            VkClient vkClient = vkAcc.VkClients.Find (e => e.ClientStatus == VkClient.Status.Unchecked);
            if (vkClient == null) {
                await UpdateClientsFromVkBySearchconfig (vkAcc);
                vkClient = vkAcc.VkClients.Find (e => e.ClientStatus == VkClient.Status.Unchecked);
            }
            return vkClient;

        }
        public static async Task<int> UpdateClientsFromVkBySearchconfig (VkAcc vkAcc, uint WantedCount = 200) {
            uint UpdatedCount = 0;
            uint Offset = 0;

            var vkApi = _getApi (vkAcc);
            if (_isVkApiActive (vkApi)) {

                using (var context = new AutoVkContext ()) {
                    while (UpdatedCount < WantedCount) {
                        var clientsSearch = vkApi.Users.Search (new UserSearchParams () {
                            City = vkAcc.City,
                                Country = vkAcc.Country,
                                AgeFrom = (ushort) vkAcc.AgeFrom,
                                AgeTo = (ushort) vkAcc.AgeTo,
                                Sex = (VkNet.Enums.Sex) vkAcc.Sex,
                                BirthMonth = (ushort) vkAcc.BirthMonth,
                                BirthDay = (ushort) vkAcc.BirthDay,
                                Online = true,
                                HasPhoto = true,
                                Count = WantedCount,
                                Offset = Offset,

                        });

                        foreach (var clientSearch in clientsSearch) {
                            if (!vkAcc.VkClients.Exists (e => e.ProfileVkId == clientSearch.Id) && clientSearch.IsClosed != true) {
                                var vkClient = new VkClient () {
                                ClientStatus = VkClient.Status.Unchecked,
                                VkAccId = vkAcc.VkAccId,
                                ProfileVkId = (int) clientSearch.Id
                                };
                                vkAcc.VkClients.Add (vkClient);
                                context.VkClients.Add (vkClient);
                                UpdatedCount++;
                            }
                        }

                        Offset += WantedCount;

                        if (clientsSearch.TotalCount < Offset) {
                            context.SaveChanges ();
                            return (int) UpdatedCount;
                        }
                    }
                    context.SaveChanges ();
                }
                return (int) UpdatedCount;
            }
            return -1;

        }
        public static VkAcc TryAuthorize (User user, string accessToken) {
            var vkApi = new VkApi ();
            vkApi.Authorize (new ApiAuthParams {
                AccessToken = accessToken
            });
            if (_isVkApiActive (vkApi)) {
                var vkAcc = new VkAcc () {
                    AccessToken = accessToken,
                };
                user.VkAccs.Add (vkAcc);
                cachedApis.Add (vkAcc, vkApi);
                return vkAcc;
            }
            return null;

        }
        public static async Task LikeAndAddToFriends (VkClient vkClient, VkAcc vkAcc = null) {
            using (var context = new AutoVkContext ()) {
                if (vkAcc == null)
                    vkAcc = context.VkAccs.FirstOrDefault (e => e.VkAccId == vkClient.VkAccId);

                var rand = new Random ();
                var api = _getApi (vkAcc);

                vkClient = context.VkClients.FirstOrDefault (e => e.VkClientId == vkClient.VkClientId);

                var Photos = await api.Photo.GetAsync (new PhotoGetParams () {
                    OwnerId = vkClient.ProfileVkId,
                        AlbumId = VkNet.Enums.SafetyEnums.PhotoAlbumType.Profile,
                        Reversed = true,
                        Count = 10
                });

                var countLikes = rand.Next (1, 3);
                if (countLikes > Photos.Count)
                    countLikes = Photos.Count;
                var PhotosNeedLikes = Photos.OrderBy (s => rand.Next ()).Take (countLikes).ToList ();
                foreach (var photo in PhotosNeedLikes) {
                    Thread.Sleep (rand.Next (3000, 8000));
                    api.Likes.Add (new LikesAddParams {
                        Type = VkNet.Enums.SafetyEnums.LikeObjectType.Photo,
                            ItemId = photo.Id.Value,
                            OwnerId = photo.OwnerId,
                    });
                }
                vkClient.ClientStatus = VkClient.Status.Liked;
                context.SaveChanges ();

                Thread.Sleep (rand.Next (3000, 8000));
                api.Friends.Add (
                    userId: (long) vkClient.ProfileVkId,
                    text: "",
                    follow : null
                );
                vkClient.ClientStatus = VkClient.Status.Ready;
                context.SaveChanges ();
            }
        }
        private static bool _isVkApiActive (VkApi vkApi) {
            if (vkApi.IsAuthorized) {
                try {
                    vkApi.Account.GetProfileInfo ();
                    return true;
                } catch (AccessTokenInvalidException) {
                    return false;
                } catch (UserAuthorizationFailException) {
                    return false;
                }

            }
            return false;
        }
    

    }
}