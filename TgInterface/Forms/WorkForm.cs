using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class WorkForm : AutoCleanForm {
        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
                case "Repeat":
                    //var nf = new WorkForm ();
                    //await this.NavigateTo (nf);
                    break;

                case "StartForm":
                    var sf = new StartForm ();
                    await this.NavigateTo (sf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render (MessageResult message) {
            ButtonForm btn = new ButtonForm ();

            btn.AddButtonRow (
                new ButtonBase ("Следующая выборка", new CallbackData ("This", "Repeat").Serialize ()));
            btn.AddButtonRow (
                new ButtonBase ("Назад", new CallbackData ("GoTo", "StartForm").Serialize ()));

            await this.Device.Send ("Click a button", btn);

        }
    }
}