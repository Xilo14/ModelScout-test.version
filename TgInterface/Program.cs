using System;

namespace TgInterface {
    class Program {
        static string _apiKey = "965933220:AAF89Gzw8wPZvOTFlrKCXZ_yIOZTy5OTCPI";
        static void Main (string[] args) {
            var bot = new TelegramBotBase.BotBase<Forms.StartForm> (_apiKey);

            bot.StateMachine = new TelegramBotBase.States.JSONStateMachine (
                AppContext.BaseDirectory + "config\\states.json");

            bot.Start ();

            Console.WriteLine ("Bot started");

            Console.ReadLine ();
            bot.Stop ();
        }
    }
}