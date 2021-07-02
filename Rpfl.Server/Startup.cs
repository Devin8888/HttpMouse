using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rpfl.Server.Applications;

namespace Rpfl.Server
{
    sealed class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// ���÷���
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddHttpForwarder()
                .AddSingleton<HttpForwarderService>()
                .AddSingleton<ConnectionService>()
                .AddSingleton<TransportChannelService>();

            services
                .AddOptions<ListenOptions>().Bind(this.Configuration.GetSection("Listen"));
        }

        /// <summary>
        /// �����м��
        /// </summary>
        /// <param name="app"></param>
        /// <param name="connectionService"></param>
        /// <param name="httpForwarderService"></param> 
        public void Configure(IApplicationBuilder app, ConnectionService connectionService, HttpForwarderService httpForwarderService)
        {
            app.UseWebSockets();
            app.Use(connectionService.OnConnectedAsync);
            app.Use(httpForwarderService.SendAsync);
        }
    }
}
