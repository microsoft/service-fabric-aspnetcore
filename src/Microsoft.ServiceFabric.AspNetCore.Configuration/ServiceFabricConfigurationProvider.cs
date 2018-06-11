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
        private readonly string packageName;
        private readonly ICodePackageActivationContext context;
        private readonly Action<ConfigurationPackage, IDictionary<string, string>> configAction;

        public ServiceFabricConfigurationProvider(ICodePackageActivationContext activationContext, string packageName, Action<ConfigurationPackage, IDictionary<string, string>> configAction = default(Action<ConfigurationPackage, IDictionary<string, string>>))
        {
            this.packageName = packageName;
            this.context = activationContext;

            if (configAction == null)
            {
                configAction = this.DefaultConfigDelegate;
            }

            this.configAction = configAction;

            this.context.ConfigurationPackageModifiedEvent += (sender, e) =>
            {
                this.LoadPackage(e.NewPackage, reload: true);
                this.OnReload(); // Notify the change
            };

            this.context.ConfigurationPackageAddedEvent += (sender, e) =>
            {
                this.LoadPackage(e.Package, reload: true);
                this.OnReload(); // Notify the change
            };
        }

        /// <summary>
        /// Load the configuration
        /// </summary>
        public override void Load()
        {
            var config = this.context.GetConfigurationPackageObject(this.packageName);
            this.LoadPackage(config);
        }

        private void DefaultConfigDelegate(ConfigurationPackage config, IDictionary<string, string> data)
        {
            foreach (var section in config.Settings.Sections)
            {
                foreach (var param in section.Parameters)
                {
                    // see https://github.com/Azure/service-fabric-aspnetcore/issues/9
                    // A typical safety guideline is to keep encrypted string encrypted in memory, and then decrypt (briefly) at time of use.
                    // With this reason, will treat encrypted value the same as plain text,
                    // user will need to handle encrypted string separately to compliant with security best practice.
                    // Data[$"{section.Name}{ConfigurationPath.KeyDelimiter}{param.Name}"] = param.IsEncrypted ? param.DecryptValue().ToUnsecureString() : param.Value;
                    data[$"{section.Name}{ConfigurationPath.KeyDelimiter}{param.Name}"] = param.Value;
                }
            }
        }

        private void LoadPackage(ConfigurationPackage config, bool reload = false)
        {
            if (reload)
            {
                this.Data.Clear();  // Rememove the old keys on re-load
            }

            // call the delegate action to populate the Data
            this.configAction(config, this.Data);
        }
    }
}
