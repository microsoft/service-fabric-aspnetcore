// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.DependencyInjection;
    
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;

    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using FluentAssertions;
    using Moq;

    public class WebHostBuilderServiceFabricExtensionTests
    {
        private Dictionary<string, string> settings;
        private AspNetCoreCommunicationListener listener;
        IWebHostBuilder builder;

        /// <summary>
        /// Used by test to check if services were configured by WebHostBuilderServiceFabricExtension.UseServiceFabricIntegration
        /// </summary>
        private bool servicesConfigured;

        public WebHostBuilderServiceFabricExtensionTests()
        {
            settings = new Dictionary<string, string>();
            // create mock IWebHostBuilder to test functionality of Service Fabric WebHostBuilder extension
            var mockBuilder = new Mock<IWebHostBuilder>();

            // setup call backs for Getting and setting settings.
            mockBuilder.Setup(y => y.GetSetting(It.IsAny<string>())).Returns<string>(name =>
            {
                string value;
                this.settings.TryGetValue(name, out value);
                return value;
            });

            mockBuilder.Setup(y => y.UseSetting(It.IsAny<string>(), It.IsAny<string>())).Callback<string, string>((name, value) =>
            {
                this.settings.Add(name, value);
            });

            mockBuilder.Setup(y => y.ConfigureServices(It.IsAny<Action<IServiceCollection>>())).Callback(() => this.servicesConfigured = true);

            this.builder = mockBuilder.Object;
            this.servicesConfigured = false;

            var context = TestMocksRepository.GetMockStatelessServiceContext();
            this.listener = new KestrelCommunicationListener(context, (uri, listen) => BuildFunc(uri, listen));
        }

        /// <summary>
        /// Verify WebHostBuilderExtension for ServiceFabricIntegrationOptions.None
        /// </summary>
        [TestMethod]
        public void VerifyWithServiceFabricIntegrationOptions_None()
        {
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.None);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            listener.UrlSuffix.Should().BeEmpty("listener is not Configured to use UniqueServiceUrl.");

            // Call the UseServiceFabricIntegration() again and verify that its dual invocation, doesn't have adverse affect.
            builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.None);
            this.servicesConfigured.Should().BeTrue("services are configured.");
            listener.UrlSuffix.Should().BeEmpty("listener is not Configured to use UniqueServiceUrl.");
        }

        /// <summary>
        /// Verify WebHostBuilderExtension for ServiceFabricIntegrationOptions.UseUniqueServiceUrl
        /// </summary>
        [TestMethod]
        public void VerifyWithServiceFabricIntegrationOptions_UseUniqueServiceUrl()
        {
            // ServiceFabricIntegrationOptions.None doesn't adds middleware and doesn't configures listener to use UrlSuffix.
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl);
            this.servicesConfigured
                .Should()
                .BeTrue("services are configured.");
            listener.UrlSuffix
                .Should()
                .NotBeEmpty("listener is Configured to use UniqueServiceUrl.");

            // Call the UseServiceFabricIntegration() again and verify that its dual invocation, doesn't have adverse affect.
            this.builder.UseServiceFabricIntegration(this.listener, ServiceFabricIntegrationOptions.UseUniqueServiceUrl);
            this.servicesConfigured
                .Should()
                .BeTrue("services are configured.");
            listener.UrlSuffix
                .Should()
                .NotBeEmpty("listener is Configured to use UniqueServiceUrl.");
        }

        private IWebHost BuildFunc(string url, AspNetCoreCommunicationListener listener)
        {
            var mockServerAddressFeature = new Mock<IServerAddressesFeature>();
            mockServerAddressFeature.Setup(y => y.Addresses).Returns(new string[] { url });
            
            var featureColelction = new FeatureCollection();
            featureColelction.Set(mockServerAddressFeature.Object);

            // Create mock IWebHost and set required things used by this test.
            var mockWebHost = new Mock<IWebHost>();
            return mockWebHost.Object;
        }

    }
}
