using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class WorkForm : AutoCleanForm {
        public WorkForm () {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
        }
        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi (message.DeviceId);
            int id;
            switch (call.Method) {
                case "AcceptClient":
                    if (Int32.TryParse (call.Value, out id))
                        await api.SetClientAccepted (id);
                    break;

                case "DeclineClient":
                    if (Int32.TryParse (call.Value, out id))
                        await api.SetClientDeclined (id);
                    break;

                case "NextSample":
                    message.Handled = false;
                    break;

                case "GoToStartForm":
                    var sf = new StartForm ();
                    await this.NavigateTo (sf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render (MessageResult message) {
            if (message.Handled)
                return;

            int Count = 20; //Need move count to User.Properties
            ButtonForm btn;

            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi (message.DeviceId);
            await api.ClearCheckedClients ();

            var clients = await api.GetUnchekedClientsForActivesVkAccs (Count);

            foreach (var client in clients) {
                btn = new ButtonForm ();

                var photos = await api.GetVkProfilePhotosMaxSizesAsync (client);
                try {
                    var msgs = await this.Client.TelegramClient.SendMediaGroupAsync (photos, this.Device.DeviceId);
                    foreach (var msg in msgs)
                        this.OldMessages.Add (msg.MessageId);

                    string text = $"{client.ProfileVkId}";

                    btn.AddButtonRow (
                        new ButtonBase ("Принять", new CallbackData ("AcceptClient", $"{client.VkClientId}").Serialize ()),
                        new ButtonBase ("Отклонить", new CallbackData ("DeclineClient", $"{client.VkClientId}").Serialize ()));

                    await this.Device.Send (text, btn);
                    await Task.Delay (1000);
                } catch (Telegram.Bot.Exceptions.ApiRequestException) {
                    await api.SetClientStatusError (client.VkClientId);
                }
            }

            btn = new ButtonForm ();

            btn.AddButtonRow (
                new ButtonBase ("Следующая выборка", new CallbackData ("NextSample", "").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoToStartForm", "").Serialize ()));

            await this.Device.Send ("Click a button", btn);

        }
    }
}