using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class AddAccForm : AutoCleanForm {

        public override async Task Action (MessageResult message) {

            var call = message.GetData<CallbackData> ();

            await message.ConfirmAction ();

            if (call == null)
                return;

            message.Handled = true;

            switch (call.Value) {
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
                new ButtonBase ("Отмена", new CallbackData ("GoTo", "AccListForm").Serialize ()));

            await this.Device.Send ("Click a button", btn);

        }
    }
}