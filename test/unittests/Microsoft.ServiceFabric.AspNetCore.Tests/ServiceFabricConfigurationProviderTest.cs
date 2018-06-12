// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Console;
    using Microsoft.ServiceFabric.AspNetCore.Configuration;
    using Xunit;

    /// <summary>
    /// Test for ServiceFabricConfigurationProvider
    /// </summary>
    public class ServiceFabricConfigurationProviderTest
    {
        private int valueCount = 0;
        private int sectionCount = 0;

        /// <summary>
        /// Verify that the basic types could be loaded
        /// </summary>
        [Fact]
        public void TestHappyCase()
        {
            var contextConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Xiaoxiao" },
                { "Section1:Age", "6" },
                { "Section2:Gender", "M" },
            }).Build();

            var context = new TestCodePackageActivationContext(contextConfig);

            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context);
            var config = builder.Build();

            config["Section1:Name"].Should().Be("Xiaoxiao");
            config["Section1:Age"].Should().Be("6");
            config["Gender"].Should().Be(null);
            config["Section1:Gender"].Should().Be(null);
            config["Section2:Gender"].Should().Be("M");
        }

        /// <summary>
        /// Verify the configuration updates
        /// </summary>
        [Fact]
        public void TestConfigUpdate()
        {
            var contextConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Xiaoxiao" },
                { "Section1:Age", "6" },
                { "Section1:Gender", "M" },
            }).Build();

            var context = new TestCodePackageActivationContext(contextConfig);

            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context);
            var config = builder.Build();

            config["Section1:Name"].Should().Be("Xiaoxiao");
            config["Section1:Age"].Should().Be("6");
            config["Section1:Gender"].Should().Be("M");

            // trigger config update
            context.TriggerConfigurationPackageModifiedEvent(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Lele" },
                { "Section1:Age", "3" },
                { "Section1:Gender", "M" },
            }).Build());

            config["Section1:Name"].Should().Be("Lele");
            config["Section1:Age"].Should().Be("3");
            config["Section1:Gender"].Should().Be("M");
        }

        /// <summary>
        /// Tests the empty configuration.
        /// </summary>
        [Fact]
        public void TestEmptyConfig()
        {
            var contextConfig = new ConfigurationBuilder().Build();
            var context = new TestCodePackageActivationContext(contextConfig);

            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context);
            var config = builder.Build();

            config.GetChildren().Should().BeEmpty();
        }

        /// <summary>
        /// Tests the multi configs.
        /// </summary>
        [Fact]
        public void TestMultiConfigs()
        {
            // first configuration
            var contextConfig1 = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Xiaoxiao" },
                { "Section1:Age", "6" },
                { "Section1:Gender", "M" },
            }).Build();

            // 2nd configuration
            var contextConfig2 = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section2:Name", "Xiaoxiao" },
                { "Section2:Age", "6" },
                { "Section2:Gender", "M" },
            }).Build();

            var context = new TestCodePackageActivationContext(new Dictionary<string, IConfiguration>() { { "Config1", contextConfig1 }, { "Config2", contextConfig2 } });

            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context);
            var config = builder.Build() as ConfigurationRoot;

            config["Section1:Name"].Should().Be("Xiaoxiao");
            config["Section1:Age"].Should().Be("6");
            config["Section1:Gender"].Should().Be("M");

            config["Section1:Name"].Should().Be("Xiaoxiao");
            config["Section1:Age"].Should().Be("6");
            config["Section1:Gender"].Should().Be("M");
        }

        /// <summary>
        /// Tests the security configuration.
        /// </summary>
        [Fact]
        public void TestEncryptedConfig()
        {
            var contextConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                // in the mock this section is special handled to turn IsEncrypted to true
                { "SecuritySection:SSN", "EncryptedValue" },
            }).Build();

            var context = new TestCodePackageActivationContext(contextConfig);

            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context);
            var config = builder.Build();

            config["SecuritySection:SSN"].Should().Be("EncryptedValue");
        }

        /// <summary>
        /// Tests the configuration action.
        /// </summary>
        [Fact]
        public void TestConfigAction()
        {
            ILogger logger = new ConsoleLogger("Test", null, false);
            Action<ConfigurationPackage, IDictionary<string, string>> configAction = (package, configData) =>
            {
                logger.LogInformation($"Config Update for package {package.Path} started");

                foreach (var section in package.Settings.Sections)
                {
                    this.sectionCount++;

                    foreach (var param in section.Parameters)
                    {
                        configData[$"{section.Name}{ConfigurationPath.KeyDelimiter}{param.Name}"] = param.Value;
                        this.valueCount++;
                    }
                }

                logger.LogInformation($"Config Update for package {package.Path} finished");
            };

            var contextConfig = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Xiaoxiao" },
                { "Section1:Age", "6" },
            }).Build();

            // initial load
            var context = new TestCodePackageActivationContext(contextConfig);
            this.valueCount = 0;
            this.sectionCount = 0;
            var builder = new ConfigurationBuilder();
            builder.AddServiceFabricConfiguration(context, "Config", configAction);
            var config = builder.Build();
            config["Section1:Name"].Should().Be("Xiaoxiao");
            config["Section1:Age"].Should().Be("6");
            this.sectionCount.Should().Be(1);
            this.valueCount.Should().Be(2);

            this.valueCount = 0;
            this.sectionCount = 0;

            // trigger config update
            context.TriggerConfigurationPackageModifiedEvent(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Section1:Name", "Lele" },
            }).Build());

            config["Section1:Name"].Should().Be("Lele");
            this.sectionCount.Should().Be(1);
            this.valueCount.Should().Be(1);
        }
    }
}
