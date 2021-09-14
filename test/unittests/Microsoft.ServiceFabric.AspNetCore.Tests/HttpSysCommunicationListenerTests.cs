// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using FluentAssertions;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Xunit;

    /// <summary>
    /// Test class for HttpSysCommunicationListener.
    /// </summary>
    public class HttpSysCommunicationListenerTests : AspNetCoreCommunicationListenerTests
    {
        /// <summary>
        /// Tests Url for ServiceFabricIntegrationOptions.UseUniqueServiceUrl
        /// 1. When no endpointRef is provided:
        ///   a. url given to Func to create IWebHost should be http://+:0
        ///   b. url returned from OpenAsync should be http://IPAddressOrFQDN:0/PartitionId/ReplicaId
        ///
        ///
        /// 2. When endpointRef is provided (protocol and port comes from endpoint.) :
        ///   a. url given to Func to create IWebHost should be protocol://+:port.
        ///   b. url returned from OpenAsync should be protocol://IPAddressOrFQDN:port/PartitionId/ReplicaId.
        ///
        /// </summary>
        [Fact]
        public void VerifyWithUseUniqueServiceUrlOption()
        {
            var context = TestMocksRepository.GetMockStatelessServiceContext();
            context.CodePackageActivationContext.GetEndpoints().Add(this.GetTestEndpoint());
            this.Listener = new HttpSysCommunicationListener(context, EndpointName, (uri, listen) => this.BuildFunc(uri, listen));
            this.UseUniqueServiceUrlOptionVerifier();
        }

        /// <summary>
        /// Tests Url for ServiceFabricIntegrationOptions.None
        /// 1. When endpoint name is provided (protocol and port comes from endpoint.) :
        ///   a. url given to Func to create IWebHost should be protocol://+:port.
        ///   b. url returned from OpenAsync should be protocol://IPAddressOrFQDN:port.
        ///
        /// </summary>
        [Fact]
        public void VerifyWithoutUseUniqueServiceUrlOption()
        {
            var context = TestMocksRepository.GetMockStatelessServiceContext();
            context.CodePackageActivationContext.GetEndpoints().Add(this.GetTestEndpoint());
            this.Listener = new HttpSysCommunicationListener(context, EndpointName, (uri, listen) => this.BuildFunc(uri, listen));
            this.WithoutUseUniqueServiceUrlOptionVerifier();
        }

        /// <summary>
        /// Verify Listener Open and Close.
        /// </summary>
        [Fact]
        public void VerifyListenerOpenClose()
        {
            var context = TestMocksRepository.GetMockStatelessServiceContext();
            context.CodePackageActivationContext.GetEndpoints().Add(this.GetTestEndpoint());
            this.Listener = new HttpSysCommunicationListener(context, EndpointName, (uri, listen) => this.BuildFunc(uri, listen));

            this.ListenerOpenCloseVerifier();
        }

        /// <summary>
        /// InvalidOperationException is thrown when Endpoint is not found in service manifest.
        /// </summary>
        [Fact]
        public void ExceptionForEndpointNotFound()
        {
            this.Listener = new HttpSysCommunicationListener(TestMocksRepository.GetMockStatelessServiceContext(), "NoEndPoint", (uri, listen) => this.BuildFunc(uri, listen));
            this.ExceptionForEndpointNotFoundVerifier();
        }

        /// <summary>
        /// ArgumentException is thrown when endpointName is null or empty string.
        /// </summary>
        [Fact]
        public void VerifyExceptionForNullOrEmptyEndpointName()
        {
            Action action =
                () =>
                    new HttpSysCommunicationListener(
                        TestMocksRepository.GetMockStatelessServiceContext(),
                        null,
                        this.BuildFunc);
            action.Should().Throw<ArgumentException>();

            action =
                () =>
                    new HttpSysCommunicationListener(
                        TestMocksRepository.GetMockStatelessServiceContext(),
                        string.Empty,
                        this.BuildFunc);
            action.Should().Throw<ArgumentException>();
        }
    }
}
