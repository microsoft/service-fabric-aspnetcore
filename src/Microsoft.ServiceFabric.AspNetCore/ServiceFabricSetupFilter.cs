// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------
namespace Microsoft.ServiceFabric.Services.Communication.AspNetCore
{
    using System;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;

    internal class ServiceFabricSetupFilter : IStartupFilter
    {
        private readonly string urlSuffix;

        internal ServiceFabricSetupFilter(string urlSuffix)
        {
            this.urlSuffix = urlSuffix;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseServiceFabricMiddleware(this.urlSuffix);
                next(app);
            };
        }
    }
}
