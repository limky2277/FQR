using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TicketBOT.Services.FacebookServices;
using TicketBOT.Services.Interfaces;
using TicketBOT.Services.JiraServices;

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
            services.AddScoped<IUserRegistrationService, JiraUserRegistrationService>();

            // Register HttpClient
            string fbApiBaseUrl = "https://graph.facebook.com/";

            services.AddHttpClient<IFbApiClientService, FbApiClientService>(client =>
                {
                    // client.BaseAddress = new Uri(fbApiBaseUrl);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
