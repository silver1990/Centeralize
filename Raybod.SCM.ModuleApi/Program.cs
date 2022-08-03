using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;
using Serilog.Exceptions;
using System;
using System.Reflection;
using Hangfire;
using Newtonsoft.Json;
using Hangfire.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Raybod.SCM.Services.Core;
using Raybod.SCM.Services.Utilitys;
using Microsoft.AspNetCore.Http;
using System.Net;
using Serilog.Sinks.Email;

namespace Raybod.SCM.ModuleApi
{
    public class Program
    {
        //public static void Main(string[] args)
        //{
        //    CreateHostBuilder(args).Build().Run();
        //}

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureWebHostDefaults(webBuilder =>
        //        {
        //            webBuilder.UseStartup<Startup>();
        //        });

        public static void Main(string[] args)
        {
            //configure logging first
            ConfigureLogging();

            CreateHost(args);
        }

        private static void CreateHost(string[] args)
        {
            try
            {
            //    var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            //    IConfigurationRoot configuration = new
            //    ConfigurationBuilder().AddJsonFile("appsettings.json",
            //    optional: false, reloadOnChange: true).Build();
            //    GlobalConfiguration.Configuration
            //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            //.UseSimpleAssemblyNameTypeSerializer()
            //.UseRecommendedSerializerSettings()
            //.UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
            //.UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
            //{
            //    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //    QueuePollInterval = TimeSpan.Zero,
            //    UseRecommendedIsolationLevel = true,
            //    DisableGlobalLocks = true
            //});
                var host = CreateHostBuilder(args).Build();

                //using (var serviceScope = host.Services.CreateScope())
                //{
                //    var services = serviceScope.ServiceProvider;

                //    try
                //    {

                //        var security = services.GetRequiredService<ISecurity>();
                //        var webHosting = services.GetRequiredService<IWebHostEnvironment>();

                //        CheckLicenseKey checkLicense = new CheckLicenseKey(security, webHosting, configuration);

                //        RecurringJob.AddOrUpdate(

                //        () => checkLicense.CheckLicense(),
                //       Cron.Daily());
                //    }
                //    catch (Exception ex)
                //    {
                //        var logger = services.GetRequiredService<ILogger<Program>>();
                //        logger.LogError("Somethings wrong", ex);
                //    }
                //}
                host.Run();
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to start {Assembly.GetExecutingAssembly().GetName().Name}" + ex.StackTrace, ex);
                throw;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).ConfigureAppConfiguration(configuration =>
                {
                    configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    configuration.AddJsonFile(
                        $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                        optional: true);
                }).UseSerilog();

        private static void ConfigureLogging()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
                    optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
            //    .WriteTo.Email(new EmailConnectionInfo
            //{
            //    EmailSubject = "Error Happend",
            //    EnableSsl = false,
            //    FromEmail = "notifications@raybodravesh.com",
            //    IsBodyHtml = true,
            //    MailServer = "mail.raybodravesh.com",
            //    Port = 587,
            //    ToEmail = "heidar.kakaei@raybodravesh.com",
            //    NetworkCredentials = new NetworkCredential
            //    {
            //        UserName = "notifications@raybodravesh.com",
            //        Password = "Notif@2020"
            //    },

            //}, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {NewLine} [{Level}] {NewLine} {Message}{NewLine}{NewLine}{Exception}")
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .Enrich.WithMachineName()
                //.WriteTo.Debug(Serilog.Events.LogEventLevel.Information)
                .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
                
                .WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
                .Enrich.WithProperty("Environment", environment)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
        {
            return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
            };
        }
    }
}
