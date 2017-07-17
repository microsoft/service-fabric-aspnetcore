// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License (MIT). See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.AspNetCore.Tests
{
    using System;
    using System.Collections.ObjectModel;

    public class KeyedCollectionImpl<TKey, TItem> : KeyedCollection<TKey, TItem>
    {
        private Func<TItem, TKey> getKey;

        protected internal KeyedCollectionImpl(Func<TItem, TKey> getKeyCallback)
            : base()
        {
            this.getKey = getKeyCallback;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return this.getKey(item);
        }

    }
}
