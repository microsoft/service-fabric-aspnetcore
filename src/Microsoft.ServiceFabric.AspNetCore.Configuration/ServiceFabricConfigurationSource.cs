// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using Microsoft.Extensions.Configuration;

    internal class ServiceFabricConfigurationSource : IConfigurationSource
    {
        private readonly Action<ConfigurationPackage, IDictionary<string, string>> configAction;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFabricConfigurationSource"/> class.
        /// </summary>
        /// <param name="activationContext">the code package activation context.</param>
        /// <param name="packageName">the name of the package.</param>
        /// <param name="configAction">the action to take to populate the configuration</param>
        public ServiceFabricConfigurationSource(ICodePackageActivationContext activationContext, string packageName, Action<ConfigurationPackage, IDictionary<string, string>> configAction = default(Action<ConfigurationPackage, IDictionary<string, string>>))
        {
            this.PackageName = packageName;
            this.ActivationContext = activationContext;
            this.configAction = configAction;
        }

        public string PackageName { get; }

        public ICodePackageActivationContext ActivationContext { get; }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new ServiceFabricConfigurationProvider(this.ActivationContext, this.PackageName, this.configAction);
        }
    }
}
