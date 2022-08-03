using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Raybod.SCM.DataAccess.Context;
using Raybod.SCM.DataAccess.Core;
using Raybod.SCM.ModuleApi.Helper.Authentication;
using Raybod.SCM.Services.Core;
using Swashbuckle.AspNetCore.SwaggerGen;
using Raybod.SCM.Utility.Filters;
using Raybod.SCM.ModuleApi.Model;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Raybod.SCM.DataTransferObject;
using System;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Http.Features;
using Raybod.SCM.Services.Utilitys.MailService;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using Microsoft.Extensions.Hosting;
using EFSecondLevelCache.Core;
using CacheManager.Core;
using Newtonsoft.Json;
using Microsoft.OpenApi.Any;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Runtime.Loader;
using DinkToPdf.Contracts;
using DinkToPdf;
using Hangfire;
using Hangfire.SqlServer;
using Raybod.SCM.Services.Implementation;

using Microsoft.AspNetCore.Hosting.Server.Features;
using Raybod.SCM.Utility.Utility;
using System.Text.Json;
using Raybod.SCM.Services.Utilitys.Raybod.SCM.Services.Core;
using Microsoft.AspNetCore.Http;

//using Raybod.SCM.Services.Core.DocumentManagement;
//using Raybod.SCM.Services.Implementation.DocumentManagement;

namespace Raybod.SCM.ModuleApi
{
    public class Startup
    {
        private readonly string _contentRootPath;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            _contentRootPath = env.ContentRootPath;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOriginsPolicy", // I introduced a string constant just as a label "AllowAllOriginsPolicy"
                builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                });
            });

            var context = new CustomAssemblyLoadContext();
            context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "libwkhtmltox.dll"));
            services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

            // hangfire config
            services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSerializerSettings(new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore })
            .UseSqlServerStorage(Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

            services.AddHangfireServer();
            services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            //services.AddMvcCore()
            //         .AddApiExplorer();
            //            services.AddDistributedRedisCache(options =>
            //            {
            //                options.Configuration = "localhost:6379";
            //                options.InstanceName = "masterScm";
            //            });

            services.AddEFSecondLevelCache();
            addInMemoryCacheServiceProvider(services);

            //services.AddDbContext<SqlServerSCMContext>(option => option.UseSqlServer(Configuration.GetConnectionString("ApplicationDbContextConnection")));
            services.AddDbContext<SqlServerSCMContext>((serviceProvider,optionsBuilder) =>
            {
                var useInMemoryDatabase = Configuration["UseInMemoryDatabase"].Equals("true", StringComparison.OrdinalIgnoreCase);
                if (useInMemoryDatabase)
                {
                    optionsBuilder.UseInMemoryDatabase("TestDb");
                }
                else
                {
                    var connectionString = Configuration["ConnectionStrings:ApplicationDbContextConnection"];
                    if (connectionString.Contains("%CONTENTROOTPATH%"))
                    {
                        connectionString = connectionString.Replace("%CONTENTROOTPATH%", _contentRootPath);
                    }
                    var httpContext = serviceProvider.GetService<IHttpContextAccessor>().HttpContext;
                    HttpRequest httpRequest;
                    string databaseQuerystringParameter = null;
                    if (httpContext != null)
                    {
                        httpRequest = httpContext.Request;
                        databaseQuerystringParameter = httpRequest.Headers["CompanyCode"].ToString();
                        var companyCode = "";
                        httpRequest.Cookies.TryGetValue("CompanyCode",out companyCode);
                        if (String.IsNullOrEmpty(databaseQuerystringParameter))
                            databaseQuerystringParameter = companyCode;
                    }
                    if (!string.IsNullOrEmpty(databaseQuerystringParameter))
                    {
                        // We have a 'database' param, stick it in.
                        databaseQuerystringParameter = databaseQuerystringParameter.ToLower() + "DbContextConnection";
                        connectionString = Configuration[$"ConnectionStrings:{databaseQuerystringParameter}"];
                    }
                    optionsBuilder.UseSqlServer(
                        connectionString
                        , serverDbContextOptionsBuilder =>
                        {
                            var minutes = (int)TimeSpan.FromMinutes(20).TotalSeconds;
                            serverDbContextOptionsBuilder.CommandTimeout(minutes);
                        });
                    optionsBuilder.EnableSensitiveDataLogging();
                    optionsBuilder.ConfigureWarnings(w =>
                    {
                    });
                }
            });
            services.AddControllersWithViews();
            services.AddSwaggerGen(c =>
            {

                c.SwaggerDoc("Setting", new OpenApiInfo { Title = "Setting APIs ", Version = "1.0" });
                c.SwaggerDoc("sale", new OpenApiInfo { Title = "Project APIs", Version = "1.0" });
                c.SwaggerDoc("documentManagement", new OpenApiInfo { Title = "Document Management APIs", Version = "1.0" });
                c.SwaggerDoc("procurementManagement", new OpenApiInfo { Title = "Procurement Management APIs", Version = "1.0" });
                c.SwaggerDoc("operationManagement", new OpenApiInfo { Title = "Operation Management APIs", Version = "1.0" });
                c.SwaggerDoc("purchase", new OpenApiInfo { Title = "Purchase Engineering APIs", Version = "1.0" });
                c.SwaggerDoc("raybodPanel", new OpenApiInfo { Title = "Panel APIs", Version = "1.0" });
                c.SwaggerDoc("SCMCustomerPanelManagement", new OpenApiInfo { Title = "Customer Panel APIs", Version = "1.0" });
                c.SwaggerDoc("fileDrive", new OpenApiInfo { Title = "File Drive APIs", Version = "1.0" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement {
                                                           {
                                                             new OpenApiSecurityScheme
                                                             {
                                                               Reference = new OpenApiReference
                                                               {
                                                                 Type = ReferenceType.SecurityScheme,
                                                                 Id = "Bearer"
                                                               }
                                                              },
                                                              new string[] { }
                                                            }
                                                          });
                c.OperationFilter<DefaultHeaderFilter>();
                //c.AddSecurityDefinition("Bearer", new ApiKeyScheme()
                //{
                //    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});
                // c.AddSecurityDefinition("Bearer",
                //new OpenApiSecurityScheme
                //{
                //    In = ParameterLocation.Header,
                //    Description = "The Bearer Token needed for Authorizing requests",
                //    Name = "",
                //    Type = SecuritySchemeType.ApiKey
                //});

                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;

                    var versions = methodInfo.DeclaringType
                        .GetCustomAttributes(typeof(SwaggerAreaAttribute), true).Cast<SwaggerAreaAttribute>().ToList();

                    return versions.Any(v => v.AreaName == docName);
                });
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }

          );

            services.Configure<TokenManagement>(Configuration.GetSection("tokenManagement"));
            var token = Configuration.GetSection("tokenManagement").Get<TokenManagement>();
            var secret = Encoding.ASCII.GetBytes(token.Secret);
            // IdentityModelEventSource.ShowPII = true;
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(secret),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero //the default for this setting is 5 minutes
                };
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                        {
                            context.Response.Headers.Add("Token-Expired", "true");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            services.AddScoped<IAuthenticate, Authentication>();

            services.Scan(scan => scan
               .FromAssemblyOf<IProductService>()
           .AddClasses()
           .AsImplementedInterfaces()
           .WithScopedLifetime());

            services.Scan(scan => scan
            .FromAssemblyOf<IUnitOfWork>()
            .AddClasses()
            .AsImplementedInterfaces()
            .WithScopedLifetime());

            //services.AddAutoMapper(typeof(SCMBackMapProfile));
            services.AddEmail(Configuration);
            //services.Configure<AppSettingsModel>(Configuration.GetSection("AppKey"));
            services.Configure<CompanyAppSettingsDto>(Configuration.GetSection("CompanyAppSetting"));
            services.AddOptions();
            services.AddHttpContextAccessor();
            services.AddScoped<IViewRenderService, ViewRenderService>();
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 104857600;
            });
        }

        private static void addInMemoryCacheServiceProvider(IServiceCollection services)
        {
            var jss = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            services.AddSingleton(typeof(ICacheManagerConfiguration),
                new CacheManager.Core.ConfigurationBuilder()
                    .WithJsonSerializer(serializationSettings: jss, deserializationSettings: jss)
                    .WithMicrosoftMemoryCacheHandle(instanceName: "MemoryCache1")
                    .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(20))
                    .DisablePerformanceCounters()
                    .DisableStatistics()
                    .Build());
            services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
            services.AddMvc().AddJsonOptions(options => options.JsonSerializerOptions.MaxDepth = 50);
        }

        //private static void addRedisCacheServiceProvider(IServiceCollection services)
        //{
        //    var jss = new JsonSerializerSettings
        //    {
        //        NullValueHandling = NullValueHandling.Ignore,
        //        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        //    };

        //    const string redisConfigurationKey = "redis";
        //    services.AddSingleton(typeof(ICacheManagerConfiguration),
        //        new CacheManager.Core.ConfigurationBuilder()
        //            .WithJsonSerializer(serializationSettings: jss, deserializationSettings: jss)
        //            .WithUpdateMode(CacheUpdateMode.Up)
        //            .WithRedisConfiguration(redisConfigurationKey, config =>
        //            {
        //                config.WithAllowAdmin()
        //                    .WithDatabase(0)
        //                    .WithEndpoint("localhost", 6379);
        //            })
        //            .WithMaxRetries(100)
        //            .WithRetryTimeout(50)
        //            .WithRedisCacheHandle(redisConfigurationKey)
        //            .WithExpiration(ExpirationMode.Absolute, TimeSpan.FromMinutes(10))
        //            .Build());
        //    services.AddSingleton(typeof(ICacheManager<>), typeof(BaseCacheManager<>));
        //}
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IBackgroundJobClient backgroundJob,
            IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
           
             var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
              .SetBasePath(env.ContentRootPath)
              .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/raybodPanel/swagger.json", "Panel");
                c.SwaggerEndpoint("/swagger/sale/swagger.json", "Project");
                c.SwaggerEndpoint("/swagger/documentManagement/swagger.json", "Document Management");
                c.SwaggerEndpoint("/swagger/procurementManagement/swagger.json", "Procurement Management");
                c.SwaggerEndpoint("/swagger/Setting/swagger.json", "Setting");
                c.SwaggerEndpoint("/swagger/SCMCustomerPanelManagement/swagger.json", "Customer Panel");
                c.SwaggerEndpoint("/swagger/operationManagement/swagger.json", "Operation Management");
                c.SwaggerEndpoint("/swagger/fileDrive/swagger.json", "File Drive");
                c.DocumentTitle = "PMIS APIs";
            });
            //Configuration = builder.Build();
            //if (Configuration.GetSection("AppKey").GetValue<string>("EnableSwagger").ToLower() == "true")
            //{

            //}

            app.UseStaticFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                Path.Combine(Directory.GetCurrentDirectory(), "Files")),
                RequestPath = "/Files"
            });
            app.UseCookieToAuthorize();
            //hangfire dashboard config
            app.UseHangfireDashboard();

            app.UseHttpsRedirection();

            app.UseRouting();

            // Use the CORS policy
            app.UseCors("AllowAllOriginsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //app.UseHttpsRedirection();
            //app.UseMvc();
            //app.UseMvc(routes =>
            //{
            //    routes.MapRoute(
            //      name: "areas",
            //      template: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
            //    );
            //});
        }

    }

    public static class StartupExtensions
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMailKit(optionBuilder =>
            {
                var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
                var mailKitOptions = new MailKitOptions()
                {
                    // get options from secrets.json
                    Server = emailConfig.Server,
                    Port = emailConfig.Port,
                    SenderName = emailConfig.SenderName,
                    SenderEmail = emailConfig.SenderEmail,
                    // can be optional with no authentication 
                    Account = emailConfig.Account,
                    Password = emailConfig.Password,
                    Security = emailConfig.Security,
                };

                if (mailKitOptions.Server == null)
                {
                    throw new InvalidOperationException("Please specify SmtpServer in appsettings");
                }
                if (mailKitOptions.Port == 0)
                {
                    throw new InvalidOperationException("Please specify Smtp port in appsettings");
                }

                if (mailKitOptions.SenderEmail == null)
                {
                    throw new InvalidOperationException("Please specify SenderEmail in appsettings");
                }

                optionBuilder.UseMailKit(mailKitOptions);
            });
            services.AddScoped<IAppEmailService, AppEmailService>();
            return services;
        }

    }
    public class DefaultHeaderFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters == null) operation.Parameters = new List<OpenApiParameter>();

            var exceptionController = new List<string>
            {
            "SCMAccount",
            "File",
            "Contract",
            "ContractSubject",
            "Bom",
            };

            var descriptor = context.ApiDescription.ActionDescriptor as ControllerActionDescriptor;

            string controllerName = descriptor == null ? string.Empty : descriptor.ControllerName.Replace("Controller", "");

            if (controllerName != null && !exceptionController.Any(c => c == controllerName))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "CurrentTeamWork",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("")
                });
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "CompanyCode",
                    In = ParameterLocation.Header,
                    Required = false,
                    Example = new OpenApiString("")
                });
            }
        }
    }

    internal class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }
    }
}

