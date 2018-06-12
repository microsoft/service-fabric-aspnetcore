// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Fabric;
    using System.Fabric.Description;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Mock implementation of the package
    /// </summary>
    public static class MockConfigurationPackage
    {
        internal static ConfigurationPackage CreateDefaultPackage(IConfiguration config, string packageName)
        {
            var package = TestHelper.CreateInstanced<ConfigurationPackage>();
            var settings = TestHelper.CreateInstanced<ConfigurationSettings>();
            var basePath = Environment.CurrentDirectory;
            package.Set("Settings", settings);
            package.Set("Path", $"{basePath}\\PackageRoot\\Config\\");

            var section = TestHelper.CreateInstanced<System.Fabric.Description.ConfigurationSection>();
            settings.Set(nameof(ConfigurationSettings.Sections), MockConfigurationSections.CreateDefault(config));

            return package;
        }
    }
}
