using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ModelScoutAPI.Models;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class StartForm : AutoCleanForm {
        public override async Task Action(MessageResult message) {

            var call = message.GetData<CallbackData>();

            await message.ConfirmAction("кусь");

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
                case "WorkForm":
                    var wf = new WorkForm();
                    await this.NavigateTo(wf);
                    break;

                case "AccListForm":
                    var alf = new AccListForm();
                    await this.NavigateTo(alf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render(MessageResult message) {
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            var vkAccs = await api.GetVkAccs();
            var totalLimit = 0;
            var totalAddedFrinds = 0;

            var text = $"У вас {vkAccs.Count} страниц:\n";

            var i = 1;
            foreach (var vkAcc in vkAccs) {
                var status = vkAcc.VkAccStatus switch {
                    VkAcc.Status.Active => "",
                    VkAcc.Status.Error => "(Ошибка)",
                    VkAcc.Status.Paused => "(Пауза)",
                    _ => ""
                };
                text += $"{i++}) {vkAcc.FirstName} {vkAcc.LastName} {vkAcc.CountAddedFriends}/{vkAcc.FriendsLimit} "
                + $"{status}"
                + "В обработке: " + await api.GetCountAcceptedVkClients(vkAcc.VkAccId) + "\n";

                totalLimit += vkAcc.FriendsLimit;
                totalAddedFrinds += vkAcc.CountAddedFriends;
            }

            text += $"\nОбщий лимит {totalAddedFrinds}/{totalLimit}";

            var btn = new ButtonForm();

            btn.AddButtonRow(
                new ButtonBase("Начать работу", new CallbackData("GoTo", "WorkForm").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Список аккаунтов", new CallbackData("GoTo", "AccListForm").Serialize()));

            await Device.Send(text, btn);

        }
    }
}
