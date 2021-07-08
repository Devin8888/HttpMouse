using HttpMouse.Implementions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace HttpMouse
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
                .AddReverseProxy()
                .AddClientDomainOptionsTransform();

            services
                .AddSingleton<IProxyConfigProvider, MomoryConfigProvider>()
                .AddSingleton<IMainConnectionService, MainConnectionService>()
                .AddSingleton<IReverseConnectionService, ReverseConnectionService>()
                .AddSingleton<IForwarderHttpClientFactory, ReverseHttpClientFactory>();

            services
                .AddOptions<HttpMouseOptions>()
                .Bind(this.Configuration.GetSection("HttpMouse"));
        }

        /// <summary>
        /// �����м��
        /// </summary>
        /// <param name="app"></param>
        /// <param name="mainConnectionService"></param> 
        public void Configure(IApplicationBuilder app, IHostEnvironment hostEnvironment, IMainConnectionService mainConnectionService)
        {
            app.UseWebSockets();
            app.Use(mainConnectionService.HandleConnectionAsync);

            if (hostEnvironment.IsDevelopment())
            {
                app.UseSerilogRequestLogging();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapReverseProxy();
                endpoints.MapReverseProxyFallback();
            });
        }
    }
}
