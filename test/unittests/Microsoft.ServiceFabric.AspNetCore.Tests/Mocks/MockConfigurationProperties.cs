// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Fabric.Description;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Mock implementation of ConfigurationProperties.
    /// </summary>
    public class MockConfigurationProperties : KeyedCollection<string, ConfigurationProperty>
    {
        /// <summary>
        /// Creates the default.
        /// </summary>
        /// <returns>the mock configuration properties.</returns>
        internal static MockConfigurationProperties CreateDefault(IConfigurationSection section)
        {
            var parameters = new MockConfigurationProperties();

            foreach (var item in section.GetChildren())
            {
                var parameter = TestHelper.CreateInstanced<ConfigurationProperty>();
                parameter.Set("Name", item.Key);
                parameter.Set("Value", item.Value);
                parameters.Add(parameter);
            }

            return parameters;
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(ConfigurationProperty item)
        {
            return item.Name;
        }
    }
}
