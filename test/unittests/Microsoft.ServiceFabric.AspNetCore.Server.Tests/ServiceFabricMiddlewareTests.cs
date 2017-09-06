// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using FluentAssertions;

    public class ServiceFabricMiddlewareTests
    {
        private HttpContext httpContext;        
        private AspNetCoreCommunicationListener listener;

        public ServiceFabricMiddlewareTests()
        {            
            // setup HttpRequest mock
            var mockHttpRequest = new Mock<HttpRequest>();
            mockHttpRequest.SetupAllProperties();

            // setup HttpResponse mock
            var mockHttpResponse = new Mock<HttpResponse>();
            mockHttpResponse.SetupAllProperties();

            // setup HttpContext mock
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(y => y.Response).Returns(mockHttpResponse.Object);
            mockHttpContext.Setup(y => y.Request).Returns(mockHttpRequest.Object);
            this.httpContext = mockHttpContext.Object;            

            var context = TestMocksRepository.GetMockStatelessServiceContext();
            this.listener = new KestrelCommunicationListener(context, BuildFunc);
        }
        
        /// <summary>
        /// Verify ErrorCode 410 is returned from Middleware, when UrlSuffix in Middleware doesn't match with what listener used
        /// when constructing url before returning to Naming Service.
        /// </summary>
        [TestMethod]
        public void VerifyReturnCode410()
        {
            bool nextCalled = false;

            this.listener.ConfigureToUseUniqueServiceUrl();
            var middleware = new ServiceFabricMiddleware((httpContext) =>
            {
                nextCalled = true;
                return Task.FromResult(true);
            }, this.listener.UrlSuffix);

            
            // send a request in which Path is different than urlSuffix
            this.httpContext.Request.Path = this.listener.UrlSuffix + "xyz";
            middleware.Invoke(this.httpContext).GetAwaiter().GetResult();

            this.httpContext.Response.StatusCode
                .Should()
                .Be(StatusCodes.Status410Gone, "status code should be 410 when path base is different from url suffix.");
            nextCalled
                .Should()
                .BeFalse("next RequestDelegate is not called by middleware.");
        }

        /// <summary>
        /// Verify next RequestDelegate invocation when Path is valid.
        /// </summary>
        [TestMethod]
        public void VerifyNextInvocationWithUrlSuffix()
        {
            // configure listener useUniqueServiceUrl
            this.listener.ConfigureToUseUniqueServiceUrl();
            this.VerifyNextInvocation();
        }

        /// <summary>
        /// Verify next RequestDelegate invocation when Path is valid.
        /// </summary>
        [TestMethod]
        public void VerifyNextInvocatioWithoutUrlSuffix()
        {
            // do not configure listener useUniqueServiceUrl
            this.VerifyNextInvocation();
        }

        /// <summary>
        /// Verify Path and PathBase in next RequestDelegate invocation.
        /// </summary>
        [TestMethod]
        public void VerifyPathsInNextInvocationWithUrlSuffix()
        {
            // configure listener useUniqueServiceUrl
            // In this case urlSuffix will be /PArtitionId/ReplicaId
            this.listener.ConfigureToUseUniqueServiceUrl();
            this.VerifyPathsInNextInvocation();
        }

        /// <summary>
        /// Verify Path and PathBase in next RequestDelegate invocation.
        /// </summary>
        [TestMethod]
        public void VerifyPathsInNextInvocationWithoutUrlSuffix()
        {
            // do not configure listener useUniqueServiceUrl
            // In this case urlSuffix will be empty

            this.VerifyPathsInNextInvocation();
        }

        /// <summary>
        /// Verify next RequestDelegate invocation when Path is valid.
        /// </summary>
        [TestMethod]
        public void VerifyNextInvocation()
        {
            bool nextCalled = false;

            var middleware = new ServiceFabricMiddleware((httpContext) =>
            {
                nextCalled = true;
                Console.WriteLine("In Next Request Delegate: HttpRequest.Path: " + httpContext.Request.Path);
                Console.WriteLine("In Next Request Delegate: HttpRequest.PathBase: " + httpContext.Request.PathBase);
                return Task.FromResult(true);
            }, this.listener.UrlSuffix);

            // send a request in which Path is same as urlSuffix
            Console.WriteLine("UrlSuffix is: " + this.listener.UrlSuffix);

            this.httpContext.Request.Path = this.listener.UrlSuffix;

            Console.WriteLine("Before Invoke: HttpRequest.Path: " + this.httpContext.Request.Path);
            Console.WriteLine("Before Invoke: HttpRequest.PathBase: " + this.httpContext.Request.PathBase);
            middleware.Invoke(this.httpContext).GetAwaiter().GetResult();

            nextCalled.Should()
                .BeTrue("next RequestDelegate is called by middleware");
        }

        /// <summary>
        /// Verify Path and PathBase in next RequestDelegate invocation.
        /// </summary>
        private void VerifyPathsInNextInvocation()
        {
            PathString pathInNext = null;
            PathString pathBaseInNext = null;
            var nextCalled = false;            

            var middleware = new ServiceFabricMiddleware((httpContext) =>
            {                
                pathInNext = httpContext.Request.Path;
                pathBaseInNext = httpContext.Request.PathBase;
                Console.WriteLine("In Next Request Delegate: HttpRequest.Path: " + httpContext.Request.Path);
                Console.WriteLine("In Next Request Delegate: HttpRequest.PathBase: " + httpContext.Request.PathBase);

                nextCalled = true;
                return Task.FromResult(true);
            }, this.listener.UrlSuffix);

            // send a request in which Path is different than urlSuffix, but has extra segment after it.
            // This extra segment should become Path for next delegate, and Path should become PathBase for next delegate.
            Console.WriteLine("UrlSuffix is: " +this.listener.UrlSuffix);

            var requestPathSuffix = "/abc";
            var requestPath = this.listener.UrlSuffix + requestPathSuffix;
            this.httpContext.Request.Path = requestPath;

            Console.WriteLine("Before Invoke: HttpRequest.Path: " + this.httpContext.Request.Path);
            Console.WriteLine("Before Invoke: HttpRequest.PathBase: " + this.httpContext.Request.PathBase);

            middleware.Invoke(this.httpContext).GetAwaiter().GetResult();

            nextCalled
                .Should()
                .BeTrue("next RequestDelegate is called by middleware");

            pathBaseInNext.ToString()
                .Should()
                .Be(this.listener.UrlSuffix, "pathBase for next RequestDelegate is changed by middleware.");
            
            pathInNext.ToString()
                .Should()
                .Be(requestPathSuffix, "Path for next RequestDelegate is changed by middleware.");

            // Verify Path and PathBase again when returned from next delegate.
            this.httpContext.Request.Path.ToString()
                .Should()
                .Be(requestPath, "Path after next RequestDelegate has been called should be the original requestPath");
            
            this.httpContext.Request.PathBase.ToString()
                .Should()
                .BeEmpty("PathBase after next RequestDelegate has been called should be empty");
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