using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class AccListForm : AutoCleanForm {

        
        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
                case "CfgAccForm":
                    var wf = new CfgAccForm (0);
                    await this.NavigateTo (wf);
                    break;

                case "StartForm":
                    var alf = new StartForm ();
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
                new ButtonBase ("Конфиг", new CallbackData ("GoTo", "CfgAccForm").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoTo", "StartForm").Serialize ()));

            await this.Device.Send ("Click a button", btn);

        }
    }
}