using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using TelegramBotBase.Base;
using TelegramBotBase.Form;

namespace TgInterface.Forms {
    public class ChangeCfgAccForm : AutoCleanForm {

        private long _vkAccId;

        public ChangeCfgAccForm(long VkAccId) {
            this._vkAccId = VkAccId;
            this.DeleteSide = TelegramBotBase.Enums.eDeleteSide.Both;
        }

        public override async Task Action(MessageResult message) {

            var call = message.GetData<CallbackData>();

            await message.ConfirmAction();

            if (call == null)
                return;

            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            var vkAcc = await api.GetVkAcc(_vkAccId);

            message.Handled = true;

            PromptDialog pd;
            switch (call.Method) {
                case "ChangeCity":
                    pd = new PromptDialog("Введите id города:");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0) {
                            vkAcc.City = result;
                            await this.Device.Send("Город изменен");
                            await api.SaveVkAcc(vkAcc);

                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeCountry":
                    pd = new PromptDialog("Введите id страны:");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0) {
                            vkAcc.Country = result;
                            await this.Device.Send("Страна изменена");
                            await api.SaveVkAcc(vkAcc);
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeBirthDay":
                    pd = new PromptDialog("Введите день рождения (0 - любой):");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0 && result <= 31) {
                            vkAcc.BirthDay = result;
                            await api.SaveVkAcc(vkAcc);
                            await this.Device.Send("День рождения изменен");
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeBirthMonth":
                    pd = new PromptDialog("Введите месяц рождения (0 - любой)::");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0 && result <= 12) {
                            vkAcc.BirthMonth = result;
                            await api.SaveVkAcc(vkAcc);
                            await this.Device.Send("Месяц рождения изменен");
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeAgeFrom":
                    pd = new PromptDialog("Введите нижний предел возраста:");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0) {
                            vkAcc.AgeFrom = result;
                            await api.SaveVkAcc(vkAcc);
                            await this.Device.Send("Нижний предел возраста изменен");
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeAgeTo":
                    pd = new PromptDialog("Введите верхний предел возраста:");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0 && result <= 200) {
                            vkAcc.AgeTo = result;
                            await api.SaveVkAcc(vkAcc);
                            await this.Device.Send("Верхний предел возраста изменен");
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeSex":
                    pd = new PromptDialog("Введите пол (0 - любой, 1 - ж, 2 - м):");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0 && result <= 2) {
                            vkAcc.Sex = result;
                            await this.Device.Send("Пол изменен");
                            await api.SaveVkAcc(vkAcc);
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;
                case "ChangeLimit":
                    pd = new PromptDialog(
                        "Введите лимит на добавление друзей\n" +
                        "(Этот лимит ограничивает кол-во заявок в друзья в день):");
                    pd.Closed += async (s, en) => {
                        int result;
                        this.DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnLeavingForm;
                        if (Int32.TryParse(pd.Value, out result) && result >= 0) {
                            vkAcc.FriendsLimit = result;
                            await this.Device.Send("Лимит изменен");
                            await api.SaveVkAcc(vkAcc);
                            await api.ClearCheckedClients();
                            await api.ClearUncheckedClients();
                        } else
                            await this.Device.Send("Неверный ввод");

                    };
                    await this.OpenModal(pd);
                    break;

                case "GoToCfgAccForm":
                    var ccaf = new CfgAccForm(_vkAccId);
                    await this.NavigateTo(ccaf);
                    break;

                default:
                    message.Handled = false;
                    break;
            }

        }

        public override async Task Render(MessageResult message) {
            var btn = new ButtonForm();

            var api = await ModelScoutAPI.ModelScoutAPIPooler.GetOrCreateApi(message.DeviceId);
            var vkAcc = await api.GetVkAcc(_vkAccId);

            btn.AddButtonRow(
                new ButtonBase($"Город ({vkAcc.City})", new CallbackData("ChangeCity", "").Serialize()),
                new ButtonBase($"Страна ({vkAcc.Country})", new CallbackData("ChangeCountry", "").Serialize())
            );
            btn.AddButtonRow(
                new ButtonBase($"День ({vkAcc.BirthDay})", new CallbackData("ChangeBirthDay", "").Serialize()),
                new ButtonBase($"Месяц ({vkAcc.BirthMonth})", new CallbackData("ChangeBirthMonth", "").Serialize())
            );
            btn.AddButtonRow(
                new ButtonBase($"В от ({vkAcc.AgeFrom})", new CallbackData("ChangeAgeFrom", "").Serialize()),
                new ButtonBase($"В до ({vkAcc.AgeTo})", new CallbackData("ChangeAgeTo", "").Serialize()),
                new ButtonBase($"Пол ({vkAcc.Sex})", new CallbackData("ChangeSex", "").Serialize())
            );
            btn.AddButtonRow(
                new ButtonBase($"Лимит ({vkAcc.FriendsLimit})", new CallbackData("ChangeLimit", "").Serialize())
            );

            btn.AddButtonRow(
                new ButtonBase("Готово", new CallbackData("GoToCfgAccForm", "").Serialize()));

            DeleteMode = TelegramBotBase.Enums.eDeleteMode.OnEveryCall;

            var text = $"{vkAcc.FirstName} {vkAcc.LastName}";


            await Device.Send(text, btn);

        }
    }
}
