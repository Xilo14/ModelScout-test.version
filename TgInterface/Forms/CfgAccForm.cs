using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ModelScoutAPI.Models;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class CfgAccForm : AutoCleanForm {

        private long _vkAccId;
        public CfgAccForm(long VkAccId) {
            _vkAccId = VkAccId;
        }

        public override async Task Action(MessageResult message) {

            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();

            if (call == null)
                return;

            message.Handled = true;
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            switch (call.Method) {
                case "RemoveVkAcc":
                    var vkAcc = await api.GetVkAcc(_vkAccId);
                    ConfirmDialog pd = new ConfirmDialog(
                        $"Подтвердите удаление аккаунта {vkAcc.FirstName} {vkAcc.LastName}",
                        new ButtonBase("Да, удалить", "ok"),
                        new ButtonBase("Нет, я случайно нажала", "cancel"));

                    Boolean Confirmed = false;
                    pd.ButtonClicked += (s, en) => {
                        if (en.Button.Value == "ok")
                            Confirmed = true;
                        else if (en.Button.Value == "cancel")
                            Confirmed = false;

                    };
                    pd.Closed += async (s, en) => {
                        DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Confirmed) {
                            await api.RemoveVkAcc(vkAcc);
                            await pd.Device.Send("Аккаунт был удален.");
                            var alf = new AccListForm();
                            await NavigateTo(alf);
                        } else {
                            await pd.Device.Send("Аккаунт не был удален.");
                        }
                    };

                    await OpenModal(pd);
                    break;

                case "GoToChangeCfgAccForm":
                    var ccaf = new ChangeCfgAccForm(_vkAccId);
                    await NavigateTo(ccaf);
                    break;

                case "GoToAccListForm":
                    var alf = new AccListForm();
                    await NavigateTo(alf);
                    break;

                case "ClearAccepted":
                    vkAcc = await api.GetVkAcc(_vkAccId);
                    await api.ClearAcceptedClients(vkAcc);
                    break;

                case "UpdateVkAcc":
                    vkAcc = await api.GetVkAcc(_vkAccId);
                    await api.UpdateVkAccStatus(vkAcc);
                    break;

                case "Pause":
                    vkAcc = await api.GetVkAcc(_vkAccId);
                    await api.SetVkAccStatus(vkAcc, VkAcc.Status.Paused);
                    break;

                case "Activate":
                    vkAcc = await api.GetVkAcc(_vkAccId);
                    await api.SetVkAccStatus(vkAcc, VkAcc.Status.Active);
                    break;

                default:
                    message.Handled = false;
                    break;
            }
        }

        public override async Task Render(MessageResult message) {
            var text = "Текущий конфиг:\n...";

            var btn = new ButtonForm();
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            var vkAcc = await api.GetVkAcc(_vkAccId);
            string changeStatusText;
            string callback;

            (changeStatusText, callback) = vkAcc.VkAccStatus switch {
                VkAcc.Status.Active => ("Приостановить", "Pause"),
                VkAcc.Status.Error => ("Проверить", "UpdateVkAcc"),
                VkAcc.Status.Paused => ("Запустить", "Activate"),
                _ => ("", "")
            };

            btn.AddButtonRow(
                    new ButtonBase(changeStatusText, new CallbackData(callback, "").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Изменить", new CallbackData("GoToChangeCfgAccForm", "").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Удалить аккаунт", new CallbackData("RemoveVkAcc", "").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Отклонить принятых", new CallbackData("ClearAccepted", "").Serialize()));
            btn.AddButtonRow(
                new ButtonBase("Назад", new CallbackData("GoToAccListForm", "").Serialize()));

            DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnEveryCall;
            await Device.Send(text, btn);

        }
    }
}
