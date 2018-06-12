// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System.Collections.Generic;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.ServiceFabric.AspNetCore.Configuration;
    using Xunit;

    /// <summary>
    /// Test for ServiceFabricConfigurationProvider
    /// </summary>
    public class ServiceFabricConfigurationProviderTest
    {
        /// <summary>
        /// Verify that the basic types could be loaded
        /// </summary>
        [Fact]
        public void TestBasicType()
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
    }
}
