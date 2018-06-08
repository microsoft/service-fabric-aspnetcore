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
    /// PathString extension as aspnet core 1.0.0 binaries doesn't have StartsWithSegments() method with an out param for matched string.
    /// </summary>
    internal static class PathStringExtensions
    {
        internal static bool StartsWithSegments(
            this PathString pathString,
            PathString other,
            out PathString matched,
            out PathString remaining)
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
