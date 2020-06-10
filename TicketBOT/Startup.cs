using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketBOT.BotAgent;
using TicketBOT.Models;
using TicketBOT.Services.FacebookServices;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;
using Microsoft.Extensions.Caching.Redis;
using EasyCaching.Core.Configurations;
using TicketBOT.Services.RedisServices;

namespace TicketBOT
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Register DI
            services.AddScoped<ICaseMgmtService, JiraCaseMgmtService>();
            services.AddSingleton<JiraUserMgmtService>();
            services.AddSingleton<CompanyService>();
            services.AddSingleton<ClientCompanyService>();
            services.AddSingleton<ConversationService>();
            services.AddScoped<Bot>();
            services.AddScoped<ISenderCacheService, SenderCacheService>();

            // Register AppSettings
            ApplicationSettings applicationSettings = new ApplicationSettings();
            Configuration.GetSection(nameof(ApplicationSettings)).Bind(applicationSettings);
            services.AddSingleton<ApplicationSettings>(applicationSettings);

            // Register HttpClient
            services.AddHttpClient<IFbApiClientService, FbApiClientService>(client =>
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                });

            // Register Redis       
            services.AddEasyCaching(option =>
            {
                services.AddEasyCaching(option =>
                {
                    option.UseRedis(config =>
                    {
                        // Setup endpoint
                        config.DBConfig.Endpoints.Add(new ServerEndPoint(applicationSettings.RedisSettings.Host, applicationSettings.RedisSettings.Port));

                        // Setup password
                        config.DBConfig.Password = applicationSettings.RedisSettings.Password;

                        // Allow admin opration
                        config.DBConfig.AllowAdmin = true;
                    }, applicationSettings.RedisSettings.CachingProvider);
                    
                });
            });
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
