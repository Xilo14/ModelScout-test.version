using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModelScoutAPI;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Quartz.Listener;
using Serilog;
using Serilog.Formatting.Compact;

namespace ModelScoutBackend {
    public class Program {
        public static ModelScoutAPIOptions MainOptions;

        private static async Task Main (string[] args) {
            // config Serilog logger 
            Log.Logger = new LoggerConfiguration ()
                .MinimumLevel.Debug ()
                .Enrich.FromLogContext ()
                .WriteTo.Console ()
                .WriteTo.File (new CompactJsonFormatter (), "./logs/msBackend/json/log.json", rollingInterval : RollingInterval.Day)
                .WriteTo.File ("./logs/msBackend/txt/log.txt", rollingInterval : RollingInterval.Day)
                .CreateLogger ();

            // config and setup Microsoft.Logging for Quartz 
            using var loggerFactory = LoggerFactory.Create (builder => {
                builder
                    .ClearProviders ()
                    .AddFilter ("Microsoft", Microsoft.Extensions.Logging.LogLevel.Warning)
                    .AddFilter ("System", Microsoft.Extensions.Logging.LogLevel.Warning)
                    .SetMinimumLevel (Microsoft.Extensions.Logging.LogLevel.Debug)
                    .AddSerilog (dispose: true);
            });

            Quartz.Logging.LogContext.SetCurrentLogProvider (loggerFactory);

            var builder = new ConfigurationBuilder ()
                .SetBasePath (Directory.GetCurrentDirectory ())
                .AddJsonFile ("appsettings.json");

            var configuration = builder.Build ();

            MainOptions = new ModelScoutAPIOptions ();

            configuration.GetSection (ModelScoutAPIOptions.ModelScout)
                .Bind (MainOptions);

            // Grab the Scheduler instance from the Factory
            StdSchedulerFactory factory = new StdSchedulerFactory ();
            IScheduler scheduler = await factory.GetScheduler ();

            // define the job and tie it to our HelloJob class
            JobKey mainJobKey = new JobKey ("MainJob", "group1");
            IJobDetail job = JobBuilder.Create<MainJob> ()
                .WithIdentity (mainJobKey)
                .Build ();

            // Trigger the job to run now, and then repeat every 10 seconds
            ITrigger trigger = TriggerBuilder.Create ()
                .WithIdentity ("trigger1", "group1")
                .StartNow ()
                .Build ();

            JobChainingJobListener chain = new JobChainingJobListener ("testChain");
            chain.AddJobChainLink (mainJobKey, mainJobKey);

            // Tell quartz to schedule the job using our trigger
            await scheduler.ScheduleJob (job, trigger);

            scheduler.ListenerManager.AddJobListener (chain, GroupMatcher<JobKey>.AnyGroup ());

            Console.WriteLine ("Press any key to start the schedular");
            await Console.In.ReadLineAsync ();

            // and start it off
            await scheduler.Start ();

            // some sleep to show what's happening
            Console.WriteLine ("Press any key to close the application");
            await Console.In.ReadLineAsync ();

            // and last shut down the scheduler when you are ready to close your program
            await scheduler.Shutdown ();
            Console.ReadKey ();

        }
    }
}