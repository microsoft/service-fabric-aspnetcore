// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Numerics;
    using System.Fabric;
    using System.Fabric.Description;
    using Moq;

    /// <summary>
    /// Contains mocks needed by Aspnet core listener unit tests.
    /// </summary>
    internal static class TestMocksRepository
    {
        internal static Guid MockPartitionID = Guid.NewGuid();
        internal static long MockReplicaOrInstanceID = 99999999999;
        internal static string MockFQDN = "MockFQDN";
        internal static string MockServiceTypeName = "MockServiceTypeName";
        internal static Uri MockServiceUri = new Uri("fabric:/MockServiceName");

        internal static StatefulServiceContext GetMockStatefulServiceContext()
        {
            return new StatefulServiceContext(GetNodeContext(),
                GetCodePackageActivationContext(),
                MockServiceTypeName,
                MockServiceUri,
                null,
                MockPartitionID,
                MockReplicaOrInstanceID);
        }

        internal static StatelessServiceContext GetMockStatelessServiceContext()
        {
            return new StatelessServiceContext(GetNodeContext(),
                GetCodePackageActivationContext(),
                MockServiceTypeName,
                MockServiceUri,
                null,
                MockPartitionID,
                MockReplicaOrInstanceID);
        }

        private static NodeContext GetNodeContext()
        {
            return new NodeContext("MockNode",
                new NodeId(BigInteger.Zero, BigInteger.Zero),
                BigInteger.Zero,
                "MockNodeType",
                MockFQDN);
        }

        private static ICodePackageActivationContext GetCodePackageActivationContext()
        {
            var endpointCollection = new KeyedCollectionImpl<string, EndpointResourceDescription>(x => x.Name);
            
            // Create mock Context and setup required things needed by tests.
            var mockContext = new Mock<ICodePackageActivationContext>();

            mockContext.Setup(x => x.GetEndpoints()).Returns(endpointCollection);
            mockContext.Setup(y => y.GetEndpoint(It.IsAny<string>())).Returns<string>(name =>
            {
                return endpointCollection[name];
            });

            return mockContext.Object;
        }        
    }
}
