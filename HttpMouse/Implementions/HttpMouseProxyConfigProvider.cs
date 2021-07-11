﻿using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Yarp.ReverseProxy.Configuration;

namespace HttpMouse.Implementions
{
    /// <summary>
    /// httpMouse代理配置提供者
    /// </summary>
    sealed class HttpMouseProxyConfigProvider : IProxyConfigProvider
    {
        private volatile HttpMouseProxyConfig config = new();
        private readonly IHttpMouseRouteProvider routeConfigProvider;
        private readonly IHttpMouseClusterProvider clusterConfigProvider;

        /// <summary>
        /// 内存配置提供者
        /// </summary>
        /// <param name="httpMouseClientHandler"></param>
        /// <param name="routeConfigProvider"></param>
        /// <param name="clusterConfigProvider"></param> 
        public HttpMouseProxyConfigProvider(
            IHttpMouseClientHandler httpMouseClientHandler,
            IHttpMouseRouteProvider routeConfigProvider,
            IHttpMouseClusterProvider clusterConfigProvider)
        {
            httpMouseClientHandler.ClientsChanged += HttpMouseClientsChanged;
            this.routeConfigProvider = routeConfigProvider;
            this.clusterConfigProvider = clusterConfigProvider;
        }

        /// <summary>
        /// 客户端变化后
        /// </summary>
        /// <param name="clients"></param>
        private void HttpMouseClientsChanged(IHttpMouseClient[] clients)
        {
            var oldConfig = this.config;

            var routes = clients.Select(item => this.routeConfigProvider.Create(item)).ToArray();
            var clusters = clients.Select(item => this.clusterConfigProvider.Create(item)).ToArray();
            this.config = new HttpMouseProxyConfig(routes, clusters);

            oldConfig.SignalChange();
        }

        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        public IProxyConfig GetConfig()
        {
            return this.config;
        }


        /// <summary>
        /// HttpMouse代理配置
        /// </summary>
        private class HttpMouseProxyConfig : IProxyConfig
        {
            private readonly CancellationTokenSource cancellationToken = new();

            /// <summary>
            /// 获取路由配置
            /// </summary>
            public IReadOnlyList<RouteConfig> Routes { get; }

            /// <summary>
            /// 获取集群配置
            /// </summary>
            public IReadOnlyList<ClusterConfig> Clusters { get; }

            /// <summary>
            /// 获取变化通知令牌
            /// </summary>
            public IChangeToken ChangeToken { get; }

            /// <summary>
            /// 内存配置
            /// </summary>
            public HttpMouseProxyConfig()
                : this(Array.Empty<RouteConfig>(), Array.Empty<ClusterConfig>())
            {
            }

            /// <summary>
            /// 内存配置
            /// </summary>
            /// <param name="routes"></param>
            /// <param name="clusters"></param>
            public HttpMouseProxyConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            {
                this.Routes = routes;
                this.Clusters = clusters;
                this.ChangeToken = new CancellationChangeToken(cancellationToken.Token);
            }

            /// <summary>
            /// 通知配置变化
            /// </summary>
            public void SignalChange()
            {
                this.cancellationToken.Cancel();
            }
        }
    }
}
