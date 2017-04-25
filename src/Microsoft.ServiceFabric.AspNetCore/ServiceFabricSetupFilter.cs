// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    internal class ServiceFabricSetupFilter : IStartupFilter
    {
        private readonly string urlSuffix;
        private readonly bool enableReverseProxyIntegration;

        internal ServiceFabricSetupFilter(string urlSuffix, bool enableReverseProxyIntegration)
        {
            this.urlSuffix = urlSuffix;
            this.enableReverseProxyIntegration = enableReverseProxyIntegration;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseServiceFabricMiddleware(this.urlSuffix);
                if (enableReverseProxyIntegration)
                {
                    app.UseServiceFabricReverseProxyIntegrationMiddleware();
                }
                next(app);
            };
        }
    }
}
