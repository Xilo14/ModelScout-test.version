using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class CfgAccForm : AutoCleanForm {

        private long _vkAccId;
        public CfgAccForm (long VkAccId) {
            this._vkAccId = VkAccId;
        }

        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;
            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi (message.DeviceId);
            switch (call.Method) {
                case "RemoveVkAcc":
                    var vkAcc = await api.GetVkAcc (this._vkAccId);
                    ConfirmDialog pd = new ConfirmDialog (
                        $"Подтвердите удаление аккаунта {vkAcc.FirstName} {vkAcc.LastName}",
                        new ButtonBase ("Да, удалить", "ok"),
                        new ButtonBase ("Нет, я случайно нажала", "cancel"));

                    Boolean Confirmed = false;
                    pd.ButtonClicked += (s, en) => {
                        if (en.Button.Value == "ok")
                            Confirmed = true;
                        else if (en.Button.Value == "cancel")
                            Confirmed = false;

                    };
                    pd.Closed += async (s, en) => {
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Confirmed) {
                            await api.RemoveVkAcc (vkAcc);
                            await pd.Device.Send ("Аккаунт был удален.");
                            var alf = new AccListForm ();
                            await this.NavigateTo (alf);
                        } else {
                            await pd.Device.Send ("Аккаунт не был удален.");
                        }
                    };

                    await this.OpenModal (pd);
                    break;

                case "GoToChangeCfgAccForm":
                    var ccaf = new ChangeCfgAccForm (_vkAccId);
                    await this.NavigateTo (ccaf);
                    break;

                case "GoToAccListForm":
                    var alf = new AccListForm ();
                    await this.NavigateTo (alf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }
        }

        public override async Task Render (MessageResult message) {
            string text = "Текущий конфиг:\n...";

            ButtonForm btn = new ButtonForm ();

            btn.AddButtonRow (
                new ButtonBase ("Изменить конфиг", new CallbackData ("GoToChangeCfgAccForm", "").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Удалить аккаунт", new CallbackData ("RemoveVkAcc", "").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoToAccListForm", "").Serialize ()));

            this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnEveryCall;
            await this.Device.Send (text, btn);

        }
    }
}