// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Moq;
    using Xunit;

    /// <summary>
    /// Test class for HostBuilderServiceFabricExtension.
    /// </summary>
    public class HostBuilderServiceFabricExtensionTests
    {
        private readonly AspNetCoreCommunicationListener listener;
        private readonly IHostBuilder builder;

        /// <summary>
        /// Used by test to check if services were configured by WebHostBuilderServiceFabricExtension.UseServiceFabricIntegration.
        /// </summary>
        private bool servicesConfigured;

        /// <summary>
        /// Initializes a new instance of the <see cref="HostBuilderServiceFabricExtensionTests"/> class.
        /// </summary>
        public HostBuilderServiceFabricExtensionTests()
        {
            // create mock IHostBuilder to test functionality of Service Fabric HostBuilder extension
            var mockBuilder = new Mock<IHostBuilder>();

            // setup Properties for IHostBuilder.
            var propertiesDictionary = new Dictionary<object, object>();
            mockBuilder.Setup(y => y.Properties).Returns(propertiesDictionary);

            // Mocking IHostBuilder.ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
            // as mocking extension methods is not possible.
            mockBuilder.Setup(y => y.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>()))
                       .Callback(() => this.servicesConfigured = true);

            this.builder = mockBuilder.Object;
            this.servicesConfigured = false;

            var context = TestMocksRepository.GetMockStatelessServiceContext();
            this.listener = new KestrelCommunicationListener(context, (uri, listen) => this.BuildFunc(uri, listen));
        }

        /// <summary>
        /// Verify WebHostBuilderExtension for ServiceFabricIntegrationOptions.None.
        /// </summary>
        [Fact]
        public void VerifyWithServiceFabricIntegrationOptions_None()
        {
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.None);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            this.listener.UrlSuffix.Should().BeEmpty("listener is not Configured to use UniqueServiceUrl.");

            // Call the UseServiceFabricIntegration() again and verify that its dual invocation, doesn't have adverse affect.
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.None);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            this.listener.UrlSuffix.Should().BeEmpty("listener is not Configured to use UniqueServiceUrl.");
        }

        /// <summary>
        /// Verify WebHostBuilderExtension for ServiceFabricIntegrationOptions.UseUniqueServiceUrl.
        /// </summary>
        [Fact]
        public void VerifyWithServiceFabricIntegrationOptions_UseUniqueServiceUrl()
        {
            // ServiceFabricIntegrationOptions.None doesn't adds middleware and doesn't configures listener to use UrlSuffix.
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            this.listener.UrlSuffix.Should().NotBeEmpty("listener is Configured to use UniqueServiceUrl.");

            // Call the UseServiceFabricIntegration() again and verify that its dual invocation, doesn't have adverse affect.
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            this.listener.UrlSuffix.Should().NotBeEmpty("listener is Configured to use UniqueServiceUrl.");
        }

        private IHost BuildFunc(string url, AspNetCoreCommunicationListener listener)
        {
            var mockServerAddressFeature = new Mock<IServerAddressesFeature>();
            mockServerAddressFeature.Setup(y => y.Addresses).Returns(new string[] { url });
            var featureCollection = new FeatureCollection();
            featureCollection.Set(mockServerAddressFeature.Object);

            // Create mock IServer and set required things used by tests.
            var mockServer = new Mock<IServer>();
            mockServer.Setup(y => y.Features).Returns(featureCollection);

            // Create ServiceCollection and IServiceProvider and set required things used for tests.
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IServer>(mockServer.Object);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Create mock IHost and set required things used by tests.
            var mockHost = new Mock<IHost>();
            mockHost.Setup(y => y.Services).Returns(serviceProvider);

            return mockHost.Object;
        }
    }
}
