// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Class containing Service Fabric related extension methods for Microsoft.Extensions.Hosting.IHostBuilder.
    /// </summary>
    public static class HostBuilderServiceFabricExtension
    {
        private static readonly string SettingName = "UseServiceFabricIntegration";

        /// <summary>
        /// Configures the Service to use ServiceFabricMiddleware and tells the listener that middleware is configured for the service so that it can
        /// suffix PartitionId and ReplicaOrInstanceId  to url before providing it to Service Fabric Runtime.
        /// </summary>
        /// <param name="hostBuilder">The Microsoft.Extensions.Hosting.IHostBuilder to configure.</param>
        /// <param name="listener">The <see cref="AspNetCoreCommunicationListener"/> to configure.</param>
        /// <param name="options">Options to configure ServiceFabricMiddleware and AspNetCoreCommunicationListener.</param>
        /// <returns>The Microsoft.Extensions.Hosting.IHostBuilder.</returns>
        public static IHostBuilder UseServiceFabricIntegration(this IHostBuilder hostBuilder, AspNetCoreCommunicationListener listener, ServiceFabricIntegrationOptions options)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException("hostBuilder");
            }

            // Check if 'UseServiceFabricIntegration' has already been called.
            if (hostBuilder.Properties.ContainsKey(SettingName))
            {
                return hostBuilder;
            }

            hostBuilder.Properties.Add(SettingName, true.ToString());

            // Configure listener to use PartitionId and ReplicaId as urlSuffix only when specified in options.
            if (options.HasFlag(ServiceFabricIntegrationOptions.UseUniqueServiceUrl))
            {
                // notify listener to use urlSuffix when giving url to Service Fabric Runtime from OpenAsync()
                listener.ConfigureToUseUniqueServiceUrl();
            }

            hostBuilder.ConfigureServices(services =>
            {
                // Configure MiddleWare
                services.AddSingleton<IStartupFilter>(new ServiceFabricSetupFilter(listener.UrlSuffix, options));
            });

            return hostBuilder;
        }
    }
}
