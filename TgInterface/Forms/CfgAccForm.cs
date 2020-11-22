using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class CfgAccForm : AutoCleanForm {

        private int _vkAccId;
        public CfgAccForm (int VkAccId) {
            this._vkAccId = VkAccId;
        }

        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
                case "ChangeCfgAccForm":
                    var ccaf = new ChangeCfgAccForm (_vkAccId);
                    await this.NavigateTo (ccaf);
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
            ButtonForm btn = new ButtonForm ();
            
            btn.AddButtonRow (
                new ButtonBase ("Изменить конфиг", new CallbackData ("GoTo", "ChangeCfgAccForm").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoTo", "AccListForm").Serialize ()));

            await this.Device.Send ("Click a button", btn);

        }
    }
}