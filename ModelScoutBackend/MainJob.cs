using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Serilog;

namespace ModelScoutBackend
{
    public class MainJob : IJob {
        public async Task Execute (IJobExecutionContext context) {
            if (Program.MainOptions == null) {
                Log.Error ("Options not initialized");
                return;
            }
            ModelScoutAPI.ModelScoutAPI api = new ModelScoutAPI.ModelScoutAPI (Program.MainOptions);

            List<Task> tasks = new List<Task> ();

            var accs = await api.GetVkAccs ();

            Log.Information ("Start MainJob. Count accs: {CountOfVkAccs}. General limit - {AddedFriends}/{FriendsLimit}",
                accs.Count,
                accs.Sum (e => e.CountAddedFriends),
                accs.Sum (e => e.FriendsLimit));

            foreach (var acc in accs) {
                String ActionText;
                var client = await api.GetLikedClient (acc);
                if (client != null && acc.CountAddedFriends < acc.FriendsLimit) {
                    tasks.Add (api.AddClientToFriends (client));

                    ActionText = "Добавление в друзья";
                } else {
                    client = await api.GetAcceptedClient (acc);
                    if (client != null) {
                        tasks.Add (api.LikeClient (client));
                        ActionText = "Ставим лайки";
                    } else

                        ActionText = "Нет клиентов";

                }
                Log.Information ("[{WorkerAccName}]({CountAddedFriends}/{FriendsLimit}) " +
                    ActionText +
                    " {ClientProfileVkId}",
                    acc.FirstName + " " + acc.LastName,
                    acc.CountAddedFriends,
                    acc.FriendsLimit,
                    client != null ? client.ProfileVkId.ToString() : "");
            }
            Log.Debug ("Wait for all tasks completed...");
            await Task.WhenAll (tasks);
            Log.Debug ("All tasks are completed. Waiting for the pause to finish");
            await Task.Delay (30000);
            Log.Information ("Finish MainJob");

        }
    }
}