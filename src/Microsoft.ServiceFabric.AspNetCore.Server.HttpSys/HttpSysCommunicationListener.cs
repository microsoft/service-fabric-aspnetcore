﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Fabric;
    using System.Globalization;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// An AspNetCore HttpSys based communication listener for Service Fabric stateless or stateful service.
    /// </summary>
    public class HttpSysCommunicationListener : AspNetCoreCommunicationListener
    {
        private readonly string endpointName;

        /// <summary>
        /// Constructs a AspNetCore HttpSys based communication listener.
        /// </summary>
        /// <param name="serviceContext">The context of the service for which this communication listener is being constructed.</param>
        /// <param name="endpointName">Name of endpoint resource defined in service manifest that should be used to create the address for listener.</param>
        /// <param name="build">Delegate to build Microsoft.AspNetCore.Hosting.IWebHost, endpoint url generated by the listener is given as input to this delegate.
        /// This gives the flexibility to change the url before creating Microsoft.AspNetCore.Hosting.IWebHost if needed.</param>
        public HttpSysCommunicationListener(ServiceContext serviceContext, string endpointName, Func<string, AspNetCoreCommunicationListener, IWebHost> build)
            : base(serviceContext, build)
        {
            if (string.IsNullOrEmpty(endpointName))
            {
                throw new ArgumentException(SR.EndpointNameNullOrEmptyExceptionMessage);
            }

            this.endpointName = endpointName;
        }

        /// <summary>
        /// Gets url for the listener. Listener url is created using the endpointName passed in the constructor.
        /// </summary>
        /// <returns>url for the listener.</returns>
        protected override string GetListenerUrl()
        {
            var serviceEndpoint = this.GetEndpointResourceDescription(this.endpointName);
            var listenUrl = string.Format(CultureInfo.InvariantCulture, "{0}://+:{1}",
                serviceEndpoint.Protocol.ToString().ToLower(), serviceEndpoint.Port);

            return listenUrl;
        }
    }
}