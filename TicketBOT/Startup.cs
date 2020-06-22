using log4net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using TicketBOT.Core.Models;
using TicketBOT.Core.Services.Interfaces;
using TicketBOT.JIRA.Services;
using TicketBOT.Middleware;
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
            services.AddScoped<UserCaseNotifService>();

            // Register AppSettings
            ApplicationSettings applicationSettings = new ApplicationSettings();
            Configuration.GetSection(nameof(ApplicationSettings)).Bind(applicationSettings);
            services.AddSingleton(applicationSettings);

            // Register FB service
            services.AddSingleton<IFbApiClientService, FbApiClientService>();

            //Register JIRA case management service
            services.AddSingleton<ICaseMgmtService, JiraCaseMgmtService>();
           
            // Register API Logging middleware
            services.AddTransient<ApiLoggingMiddleware>();

            // Register Swagger
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Middleware Extension
            app.UseFactoryBasedLoggingMiddleware();

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

            _logger.Info("[TicketBOT] Ticket Bot Service (Started)");
        }
    }
}
