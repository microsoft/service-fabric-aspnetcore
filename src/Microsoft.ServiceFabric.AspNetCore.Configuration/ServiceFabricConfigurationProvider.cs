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

    internal class ServiceFabricConfigurationProvider : ConfigurationProvider
    {
        private readonly ICodePackageActivationContext context;
        private readonly ServiceFabricConfigurationOptions options;

        public ServiceFabricConfigurationProvider(ICodePackageActivationContext activationContext, ServiceFabricConfigurationOptions options)
        {
            this.context = activationContext;

            this.options = options ?? throw new ArgumentNullException(nameof(options));

            this.context.ConfigurationPackageModifiedEvent += (sender, e) =>
            {
                this.HandleNewPackage(e.NewPackage);
            };

            this.context.ConfigurationPackageAddedEvent += (sender, e) =>
            {
                this.HandleNewPackage(e.Package);
            };
        }

        /// <summary>
        /// Load the configuration.
        /// </summary>
        public override void Load()
        {
            var config = this.context.GetConfigurationPackageObject(this.options.PackageName);
            this.LoadPackage(config);
        }

        private void HandleNewPackage(ConfigurationPackage package)
        {
            // Load configuration from new package only if it is the ConfigPackage mapped to this provider
            if (package.Description.Name == this.options.PackageName)
            {
                this.LoadPackage(package, reload: true);
                this.OnReload(); // Notify the change
            }
        }

        private void LoadPackage(ConfigurationPackage config, bool reload = false)
        {
            if (reload)
            {
                this.Data.Clear();  // Remove the old keys on re-load
            }

            // call the delegate action to populate the Data
            this.options.ConfigAction(config, this.Data);
        }
    }
}
