using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class WorkForm : AutoCleanForm {
        public WorkForm() {
            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
        }
        public override async Task Action(MessageResult message) {

            var call = message.GetData<CallbackData>();

            if (call == null)
                return;

            message.Handled = true;
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            int id;
            switch (call.Method) {
                case "AcceptClient":
                    if (Int32.TryParse(call.Value, out id)) {
                        await api.SetClientAccepted(id);
                        await message.ConfirmAction("Принято");
                    }
                    break;

                case "DeclineClient":
                    if (Int32.TryParse(call.Value, out id)) {
                        await api.SetClientDeclined(id);
                        await message.ConfirmAction("Отклонено");
                    }
                    break;

                case "NextSample":
                    message.Handled = false;
                    await message.ConfirmAction("Следующая выборка");
                    break;

                case "GoToStartForm":
                    await api.ClearCheckedClients();
                    await message.ConfirmAction("Додомудохатыдоридноиматы");
                    var sf = new StartForm();
                    await this.NavigateTo(sf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render(MessageResult message) {
            if (message.Handled)
                return;

            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            var vkAccs = await api.GetVkAccs();
            int totalLimit = 0;
            int totalAddedFrinds = 0;

            var text = $"У вас {vkAccs.Count} страниц:\n";

            var i = 1;
            foreach (var vkAcc in vkAccs) {
                var status = vkAcc.VkAccStatus == ModelScoutAPI.Models.VkAcc.Status.Error ? "(Ошибка)" : "";
                text += $"{i++}) {vkAcc.FirstName} {vkAcc.LastName} {vkAcc.CountAddedFriends}/{vkAcc.FriendsLimit} "
                + $"{status}"
                + "В обработке: " + await api.GetCountAcceptedVkClients(vkAcc.VkAccId) + "\n";

                totalLimit += vkAcc.FriendsLimit;
                totalAddedFrinds += vkAcc.CountAddedFriends;
            }

            text += $"\nОбщий лимит {totalAddedFrinds}/{totalLimit}";

            await Device.Send(text);

            int Count = 20; //Need move count to User.Properties
            ButtonForm btn;


            await api.ClearCheckedClients();

            var clients = await api.GetUnchekedClientsForActivesVkAccs(Count);

            foreach (var client in clients) {
                btn = new ButtonForm();


                try {
                    var photos = await api.GetVkProfilePhotosMaxSizesAsync(client);
                    var msgs = await this.Client.TelegramClient.SendMediaGroupAsync(photos, this.Device.DeviceId);
                    foreach (var msg in msgs)
                        this.OldMessages.Add(msg.MessageId);


                    var vkUser = ModelScoutAPI.VkApisManager.GetVkUser(client);

                    text =
                        $"{vkUser.FirstName} {vkUser.LastName}({vkUser.Id})"
                        + $"\n{vkUser.City?.Title} {vkUser.BirthDate}"
                        + $"\nСкаут {client.VkAcc.FirstName} {client.VkAcc.LastName}"
                        + $" [{client.VkAcc.CountAddedFriends}"
                        + $"+{await api.GetCountAcceptedVkClients(client.VkAcc.VkAccId)}/{client.VkAcc.FriendsLimit}]";

                    btn.AddButtonRow(
                        new ButtonBase("Принять", new CallbackData("AcceptClient", $"{client.VkClientId}").Serialize()),
                        new ButtonBase("Отклонить", new CallbackData("DeclineClient", $"{client.VkClientId}").Serialize()));

                    await this.Device.Send(text, btn);
                    await Task.Delay(1000);
                } catch (Telegram.Bot.Exceptions.ApiRequestException ex) {
                    await api.SetClientStatusError(client.VkClientId);
                } catch (HttpRequestException ex) {
                    await Task.Delay(100000);
                    //await api.SetClientStatusError(client.VkClientId);
                } catch (Exception ex) {
                    await api.SetClientStatusError(client.VkClientId);
                }
            }

            btn = new ButtonForm();

            btn.AddButtonRow(
                    new ButtonBase("Следующая выборка", new CallbackData("NextSample", "").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Назад", new CallbackData("GoToStartForm", "").Serialize()));

            await this.Device.Send("Click a button", btn);

        }
    }
}
