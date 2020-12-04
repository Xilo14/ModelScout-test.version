using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class AccListForm : AutoCleanForm {
        public AccListForm () {
            this.DeleteSide = TelegramBotBase.Enums.eDeleteSide.Both;
        }
        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi (message.DeviceId);

            switch (call.Method) {
                case "GoToCfgAccForm":
                    var caf = new CfgAccForm (Convert.ToInt64 (call.Value));
                    await this.NavigateTo (caf);
                    break;

                case "GoToAddAccForm":
                    PromptDialog pd = new PromptDialog (
                        "Введите токен аккаунта\n" +
                        "(Можно получить тут https://vkhost.github.io/)");

                    pd.Closed += async (s, en) => {
                        ModelScoutAPI.Models.VkAcc vkAcc;
                        vkAcc = await api.CreateVkAcc (pd.Value);
                        if (vkAcc == null) {
                            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                            await this.Device.Send ("Аккаунт не был добавлен. Проверьте токен");

                        } else {
                            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                            await this.Device.Send ($"Был добавлен аккаунт {vkAcc.FirstName} {vkAcc.LastName}");

                        }

                    };

                    await this.OpenModal (pd);
                    break;

                case "GoToStartForm":
                    var alf = new StartForm ();
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

            string text =
                $"У вас {vkAccs.Count} страниц:\n" +
                $"Нажмите на аккаунт для настройки\n";

            ButtonForm btn = new ButtonForm ();

            foreach (var vkAcc in vkAccs)
                btn.AddButtonRow (
                    new ButtonBase (
                        $"{vkAcc.FirstName} {vkAcc.LastName} ({vkAcc.CountAddedFriends}/{vkAcc.FriendsLimit})",
                        new CallbackData ("GoToCfgAccForm", vkAcc.VkAccId.ToString ()).Serialize ()));

            btn.AddButtonRow (
                new ButtonBase ("Добавить аккаунт", new CallbackData ("GoToAddAccForm", "").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoToStartForm", "").Serialize ()));

            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnEveryCall;
            await this.Device.Send (text, btn);

        }
    }
}