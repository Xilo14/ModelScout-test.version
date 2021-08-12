using System;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ModelScoutAPI;

using Serilog;
using Serilog.Context;
using Serilog.Formatting.Compact;

namespace TgInterface {
    class Program {
        static void Main(string[] args) {
            //Serilog logger config
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(new CompactJsonFormatter(), "./logs/msTgInterface/json/log.json", rollingInterval: RollingInterval.Day)
                .WriteTo.File("./logs/msTgInterface/txt/log.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            //Microsoft.Logging for VkNet config and setup
            using var loggerFactory = LoggerFactory.Create(builder => {
                builder
                    .ClearProviders()
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddSerilog(dispose: true);
            });

            VkApisManager.logger = loggerFactory.CreateLogger<VkNet.VkApi>();

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var configuration = builder.Build();

            var modelScoutAPIOptions = new ModelScoutAPIOptions();

            configuration.GetSection(ModelScoutAPIOptions.ModelScout)
                .Bind(modelScoutAPIOptions);

            var apiKey = configuration.GetSection("TgInterface")
                .GetSection("ApiKey")
                .Value;

            var bot = new TelegramBotBase.BotBase<Forms.StartForm>(apiKey);

            ModelScoutAPIPooler.DefaultOptions = modelScoutAPIOptions;
            VkApisManager.modelScoutAPIOptions = modelScoutAPIOptions;

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(
                (object sender, UnhandledExceptionEventArgs args) => {
                    var e = (Exception)args.ExceptionObject;

                    using (LogContext.PushProperty("exception", e)) {
                        Log.Fatal("Unhandled exception!" +
                            $"\n{e.GetType()}" +
                            $"\nMessage: {e.Message}" +
                            $"\nSource: {e.Source}" +
                            $"\nStackTrace: {e.StackTrace}");
                    }
                });


            bot.StateMachine = new TelegramBotBase.States.JSONStateMachine(
                AppContext.BaseDirectory + "config\\states.json");

            bot.SetSetting(TelegramBotBase.Enums.eSettings.LogAllMessages, true);
            bot.Message += (s, en) => {
                using (LogContext.PushProperty("message", en.Message)) {
                    Log.Information(
                        "New msg from @{Username}({ChatId}) \"{Text}\" {RawData}",
                        en.Message.Message.From.Username,
                        en.DeviceId,
                        en.Message.MessageText,
                        en.Message.RawData ?? "");
                }
            };

            bot.Start();
            Log.Information("Bot started");

            Console.ReadLine();

            bot.Stop();
            Log.Information("Bot stopped");
        }


    }
}
