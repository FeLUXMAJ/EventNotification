// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System;

namespace Microsoft.Framework.Notification.EventSources
{
    internal class EventMappingBuilder : IEventMappingBuilder
    {
        private EventMapping _mapping;

        public EventMappingBuilder(EventMapping mapping)
        {
            _mapping = mapping;
        }

        public void AddDataMapping(EventDataMapping mapping)
        {
            _mapping.DataMappings.Add(mapping);
        }
    }
}

#endif