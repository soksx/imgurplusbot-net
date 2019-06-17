using imgurplusbot.bll.Models;
using imgurplusbot.bll.Services;
using imgurplusbot.bll.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace imgurplusbot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddScoped<IUpdateService, UpdateService>();
            services.AddSingleton<IBotService, BotService>();

            services.Configure<BotConfiguration>(Configuration.GetSection("BotConfiguration"));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc();
        }
    }
}
