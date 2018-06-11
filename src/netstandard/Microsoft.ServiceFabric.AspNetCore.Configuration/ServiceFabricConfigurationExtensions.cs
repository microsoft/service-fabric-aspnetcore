// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Text;
    using Microsoft.Extensions.Configuration;

    public static class ServiceFabricConfigurationExtensions
    {
        /// <summary>
        /// Add configuration for all the configuration packages declared for current service.
        /// </summary>
        public static IConfigurationBuilder AddServiceFabricConfiguration(this IConfigurationBuilder builder)
        {
            ICodePackageActivationContext activationContext = FabricRuntime.GetActivationContext();

            // DO NOT dispose activation context as it will be saved to DI container and used later.
            // using (activationContext)
            {
                return builder.AddServiceFabricConfiguration(activationContext);
            }   
        }

        /// <summary>
        /// Add configuration for all the configuration packages declared for current code package.
        /// </summary>
        public static IConfigurationBuilder AddServiceFabricConfiguration(this IConfigurationBuilder builder, ICodePackageActivationContext context)
        {
            foreach (var packageName in context.GetConfigurationPackageNames())
            {
                builder.Add(new ServiceFabricConfigurationSource(context, packageName));
            }

            return builder;
        }

        /// <summary>
        /// Add configuration for given configuration package from packageName parameter
        /// </summary>
        public static IConfigurationBuilder AddServiceFabricConfiguration(this IConfigurationBuilder builder, ICodePackageActivationContext context, string packageName)
        {
            return builder.Add(new ServiceFabricConfigurationSource(context, packageName));
        }

        /// <summary>
        /// Add configuration for given configuration package from packageName parameter as well as customize config action to populate the Data.
        /// </summary>
        public static IConfigurationBuilder AddServiceFabricConfiguration(
            this IConfigurationBuilder builder, ICodePackageActivationContext context, string packageName, 
            Action<ConfigurationPackage, IDictionary<string, string>> configAction)
        {
            return builder.Add(new ServiceFabricConfigurationSource(context, packageName, configAction));
        }
    }
}
