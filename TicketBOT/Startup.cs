using Hangfire;
using Hangfire.MemoryStorage;
using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.Globalization;
using TicketBOT.Core.Helpers;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.Helpers;
using TicketBOT.JIRA.Services;
using TicketBOT.Services.BotServices;
using TicketBOT.Services.DBServices;
using TicketBOT.Services.FacebookServices;
using TicketBOT.Services.Interfaces;

namespace TicketBOT
{
    public class Startup
    {
        private static readonly ILog _logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _logger.Info("[TicketBOT] Ticket Bot Service (Starting...)");
        }

        public IConfiguration Configuration { get; }
        public ApplicationSettings applicationSettings;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register DI
            services.AddSingleton<TicketSysUserMgmtService>();
            services.AddSingleton<CompanyService>();
            services.AddSingleton<ClientCompanyService>();
            services.AddScoped<BotService>();
            services.AddScoped<OneTimeNotificationService>();
            //services.AddScoped<ISenderCacheService, SenderCacheService>();
            services.AddScoped<IConversationService, ConversationService>();
            services.AddScoped<ITicketSysNotificationService, UserCaseNotifService>();

            // Register AppSettings
            applicationSettings = new ApplicationSettings();
            Configuration.GetSection(nameof(ApplicationSettings)).Bind(applicationSettings);
            services.AddSingleton(applicationSettings);

            // Register FB service
            services.AddSingleton<IFbApiClientService, FbApiClientService>();

            //Register JIRA case management service
            services.AddSingleton<ICaseMgmtService, JiraCaseMgmtService>();

            // Register Swagger
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.AddHangfire(c => c.UseMemoryStorage());
            services.AddLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                new CultureInfo("en-SG")
                //,new CultureInfo("en-GB")
                //,new CultureInfo("de-DE"
            };
                options.DefaultRequestCulture = new RequestCulture("en-SG", "en-SG");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting 
                // numbers, dates, etc.

                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, 
                // i.e. we have localized resources for.

                options.SupportedUICultures = supportedCultures;
            });

            // CORS
            services.AddCors(o => o.AddPolicy("_myAllowSpecificOrigins", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseSwagger();
            // Swagger.io
            app.UseSwaggerUI(c =>
            {
                string swaggerJsonBasePath = string.IsNullOrWhiteSpace(c.RoutePrefix) ? "." : "..";
                c.SwaggerEndpoint($"{swaggerJsonBasePath}/swagger/v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseCors("_myAllowSpecificOrigins");


            app.UseHangfireServer();
            var notifSett = applicationSettings.NotificationSettings;
            // Recurring job to blast notification
            // Call notification endpoint periodically
            // Notification URL & interval are configured in appsettings
            RecurringJob.AddOrUpdate(() => RestApiHelper.GetAsync(string.Format(notifSett.NotificationApiPath)), $"*/{notifSett.RefreshIntervalMins} * * * *");

            _logger.Info("[TicketBOT] Ticket Bot Service (Started)");
        }
    }
}
