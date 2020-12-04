using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class StartForm : AutoCleanForm {
        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
                case "WorkForm":
                    var wf = new WorkForm ();
                    await this.NavigateTo (wf);
                    break;

                case "AccListForm":
                    var alf = new AccListForm ();
                    await this.NavigateTo (alf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render (MessageResult message) {
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi (message.DeviceId);
            var vkAccs = await api.GetVkAccs ();
            int TotalLimit = 0;
            int TotalAddedFrinds = 0;

            string text = $"У вас {vkAccs.Count} страниц:\n";

            int i = 1;
            foreach (var vkAcc in vkAccs) {
                text += $"{i++}) {vkAcc.FirstName} {vkAcc.LastName} {vkAcc.CountAddedFriends}/{vkAcc.FriendsLimit};\n";

                TotalLimit += vkAcc.FriendsLimit;
                TotalAddedFrinds += vkAcc.CountAddedFriends;
            }

            text += $"\nОбщий лимит {TotalAddedFrinds}/{TotalLimit}";

            ButtonForm btn = new ButtonForm ();

            btn.AddButtonRow (
                new ButtonBase ("Начать работу", new CallbackData ("GoTo", "WorkForm").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Список аккаунтов", new CallbackData ("GoTo", "AccListForm").Serialize ()));

            await this.Device.Send (text, btn);

        }
    }
}