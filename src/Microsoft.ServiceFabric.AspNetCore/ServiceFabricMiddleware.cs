// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Builder;

    /// <summary>
    /// A middleware to be used with Service Fabric stateful and stateless services hosted in Kestrel or WebListener.
    /// This middleware examines the Microsoft.AspNetCore.Http.HttpRequest.Path in request to determine if the request is intended for this replica.
    /// </summary>
    /// <remarks>
    /// This middleware when used with Kestrel and WebListener based Service Fabric Communication Listeners allows handling of scenarios when
    /// the Replica1 listening on Node1 and por11 has moved and another Replica2 is opened on Node1 got Port1. 
    /// A client which has resolved Replica1 before it moved, will send the request to Node1:Port1. Using this middleware 
    /// Replica2 can reject calls which were meant for Replica1 by examining the Path in incoming request.
    /// </remarks>
    public class ServiceFabricMiddleware
    {
        private readonly RequestDelegate next;
        private readonly string urlSuffix;

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceFabricMiddleware"/>
        /// </summary>
        /// <param name="next">Next request handler in pipeline.</param>
        /// <param name="urlSuffix"></param>
        public ServiceFabricMiddleware(RequestDelegate next, string urlSuffix)
        {
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }

            if (urlSuffix == null)
            {
                throw new ArgumentNullException("urlSuffix");
            }

            this.urlSuffix = urlSuffix;
            this.next = next;
        }

        /// <summary>
        /// Invoke.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Task for the execution by next middleware in pipeline.</returns>
        public async Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            if (this.urlSuffix.Equals(string.Empty))
            {
                // when urlSuffix is empty string, just call the next middleware in pipeline.
                await this.next(context);
            }
            else
            {
                // If this middleware is enabled by specifying UseServiceFabricIntegration(), CommunicationListnerBehavior is:
                //   With ServiceFabricIntegrationOptions.UseUniqueServiceUrl (urlSuffix is /PartitionId/ReplicaOrInstanceId)
                //      - Url given to WebServer is http://+:port
                //      - Url given to Service Fabric Runtime is http://ip:port/PartitionId/ReplicaOrInstanceId


                // Since when registering with IWebHost, only http://+:port is provided:
                //    - HttpRequest.Path contains everything in url after http://+:port, and it must start with urlSuffix

                // So short circuit and return StatusCode 410 if (message isn't intended for this replica,):
                //    - HttpRequest.Path doesn't start with urlSuffix

                PathString matchedPath;
                PathString remainingPath;

                if (!context.Request.Path.StartsWithSegments(urlSuffix, out matchedPath, out remainingPath))
                {
                    context.Response.StatusCode = StatusCodes.Status410Gone;
                    return;
                }

                // All good, change Path, PathBase and call next middleware in the pipeline
                var originalPath = context.Request.Path;
                var originalPathBase = context.Request.PathBase;
                context.Request.Path = remainingPath;
                context.Request.PathBase = originalPathBase.Add(matchedPath);

                try
                {
                    await this.next(context);
                }
                finally
                {
                    context.Request.Path = originalPath;
                    context.Request.PathBase = originalPathBase;
                }
            }
        }
    }

    /// <summary>
    /// Extension class to use ServiceFabricKestrelMiddleware for Service Fabric stateful or stateless service
    /// using Kestrel or WebListener as WebServer.
    /// </summary>
    public static class ServiceFabricMiddlewareExtensions
    {
        /// <summary>
        /// Extension method to use ServiceFabricKestrelMiddleware for Service Fabric stateful or stateless service
        /// using Kestrel or WebListener as WebServer.
        /// </summary>
        /// <param name="builder">Microsoft.AspNetCore.Builder.IApplicationBuilder</param>        
        /// <param name="urlSuffix"></param>
        /// <returns>Microsoft.AspNetCore.Builder.IApplicationBuilder</returns>
        public static IApplicationBuilder UseServiceFabricMiddleware(this IApplicationBuilder builder, string urlSuffix)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }
            if (urlSuffix == null)
            {
                throw new ArgumentNullException("urlSuffix");
            }

            return builder.UseMiddleware<ServiceFabricMiddleware>(urlSuffix);
        }
    }

    /// <summary>
    /// PathBuilder extension as aspnet core 1.0.0 binaries doesn't have StartsWithSegments() method with an out param for matched string.
    /// </summary>
    internal static class PathBuilderExtension
    {
        internal static bool StartsWithSegments(this PathString pathString, PathString other,
            out PathString matched, out PathString remaining)
        {
            var value1 = pathString.Value ?? string.Empty;
            var value2 = other.Value ?? string.Empty;

            if (value1.StartsWith(value2, StringComparison.OrdinalIgnoreCase))
            {
                if (value1.Length == value2.Length || value1[value2.Length] == '/')
                {
                    matched = new PathString(value1.Substring(0, value2.Length));
                    remaining = new PathString(value1.Substring(value2.Length));
                    return true;
                }
            }

            remaining = PathString.Empty;
            matched = PathString.Empty;
            return false;
        }
    }
}