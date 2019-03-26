// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.ObjectModel;
    using System.Fabric.Description;
    using Microsoft.Extensions.Configuration;
    using ConfigurationSection = System.Fabric.Description.ConfigurationSection;

    /// <summary>
    /// Mock implementation of the sections
    /// </summary>
    public class MockConfigurationSections : KeyedCollection<string, ConfigurationSection>
    {
        internal static MockConfigurationSections CreateDefault(IConfiguration config)
        {
            var sections = new MockConfigurationSections();

            foreach (var item in config.GetChildren())
            {
                var section = TestHelper.CreateInstanced<ConfigurationSection>();
                section.Set("Name", item.Key);
                section.Set(nameof(ConfigurationSection.Parameters), MockConfigurationProperties.CreateDefault(item));
                sections.Add(section);
            }

            return sections;
        }

        /// <inheritdoc/>
        protected override string GetKeyForItem(ConfigurationSection item)
        {
            return item.Name;
        }
    }
}
