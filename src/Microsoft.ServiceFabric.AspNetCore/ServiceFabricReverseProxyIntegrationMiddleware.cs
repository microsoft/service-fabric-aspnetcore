// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;

    /// <summary>
    /// A middleware to be used with Service Fabric stateful and stateless services hosted in Kestrel or WebListener.
    /// This middleware automatically adds X-ServiceFabric ResourceNotFound header, required by the Service Fabric Reverse Proxy, when 404 status code is returned
    /// </summary>
    public class ServiceFabricReverseProxyIntegrationMiddleware
    {
        private const string XServiceFabricHeader = "X-ServiceFabric";
        private const string XServiceFabricResourceNotFoundValue = "ResourceNotFound";

        private readonly RequestDelegate next;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFabricReverseProxyIntegrationMiddleware"/> class.
        /// </summary>
        /// <param name="next">Next request handler in pipeline.</param>
        public ServiceFabricReverseProxyIntegrationMiddleware(RequestDelegate next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            this.next = next;
        }

        /// <summary>
        /// Invoke.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Task for the execution by next middleware in pipeline.</returns>
        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.Response.OnStarting(() =>
            {
                if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    context.Response.Headers[XServiceFabricHeader] = XServiceFabricResourceNotFoundValue;
                }

                // TODO: When upgraded to .NET Standard 2.0 replace with Task.CompletedTask to avoid unnecessary allocation
                return Task.FromResult<object>(null);
            });

            return this.next(context);
        }
    }
}
