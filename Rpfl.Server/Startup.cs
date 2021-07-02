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
                .AddHttpProxy()
                .AddSingleton<HttpProxyService>()
                .AddSingleton<ConnectionService>()
                .AddSingleton<TransportChannelService>();

            services
                .AddOptions<ListenOptions>().Bind(this.Configuration.GetSection("Listen"));
        }

        /// <summary>
        /// �����м��
        /// </summary>
        /// <param name="app"></param>
        /// <param name="httpProxyService"></param>
        /// <param name="connectionService"></param>
        public void Configure(IApplicationBuilder app, HttpProxyService httpProxyService, ConnectionService connectionService)
        {
            app.UseWebSockets();
            app.Use(connectionService.OnConnectedAsync);
            app.Use(httpProxyService.ProxyAsync);
        }
    }
}
