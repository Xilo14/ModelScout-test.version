using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ModelScoutAPI.Models;
using Quartz;
using Serilog;

namespace ModelScoutBackend {
    public class MainJob : IJob {
        public async Task Execute(IJobExecutionContext context) {
            if (Program.MainOptions == null) {
                Log.Error("Options not initialized");
                return;
            }
            var api = new ModelScoutAPI.ModelScoutAPI(Program.MainOptions);
            await api.ClearLimitsOnceAtDay();
            var tasks = new List<Task>();

            var accs = await api.GetVkAccs();


            Log.Information("Start MainJob. Count accs: {CountOfVkAccs}. General limit - {AddedFriends}/{FriendsLimit}",
                accs.Count,
                accs.Sum(e => e.CountAddedFriends),
                accs.Sum(e => e.FriendsLimit));


            foreach (var acc in accs) {
                string actionText;
                VkClient client = null;
                if (acc.VkAccStatus == VkAcc.Status.Error)
                    actionText = "Ошибка!";
                else if (acc.CountAddedFriends >= acc.FriendsLimit) {
                    actionText = "Достигнут лимит";
                } else {
                    client = await api.GetLikedClient(acc);
                    if (client != null) {
                        tasks.Add(api.AddClientToFriends(client));

                        actionText = "Добавление в друзья";
                    } else {
                        client = await api.GetAcceptedClient(acc);
                        if (client != null) {
                            tasks.Add(api.LikeClient(client));
                            actionText = "Ставим лайки";
                        } else

                            actionText = "Нет клиентов";

                    }
                }
                Log.Information("[{VkAccStatus}]({CountAddedFriends}+{InProccessCount}/{FriendsLimit})[{WorkerAccName}] " +
                    actionText +
                    " {ClientProfileVkId}",
                        acc.VkAccStatus.ToString(),
                        acc.CountAddedFriends,
                        await api.GetCountAcceptedVkClients(acc.VkAccId),
                        acc.FriendsLimit,
                        acc.FirstName + " " + acc.LastName,
                        client != null ? client.ProfileVkId.ToString() : "");
            }
            Log.Debug("Wait for all tasks completed...");
            await Task.WhenAll(tasks);
            Log.Debug("All tasks are completed. Waiting for the pause to finish");
            var delay = 70000;
            if (DateTime.Now.Hour < 13)
                delay *= 2;
            await Task.Delay(delay);
            Log.Information("Finish MainJob");
        }
    }
}
