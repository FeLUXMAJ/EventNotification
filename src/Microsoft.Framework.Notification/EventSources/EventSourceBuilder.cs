// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Framework.Notification.EventSources
{
    public class EventSourceBuilder : IEventSourceBuilder
    {
        private List<EventMapping> _mappings = new List<EventMapping>();
        private readonly string _name;

        public EventSourceBuilder(string name)
        {
            _name = name;
        }

        public IEventMappingBuilder MapEvent(string name, int eventId)
        {
            var mapping = new EventMapping()
            {
                NotificationName = name,
                EventName = name,
                EventId = eventId,
            };

            _mappings.Add(mapping);
            return new EventMappingBuilder(mapping);
        }

        public IEventSourceNotificationListener CreateListener()
        {
            var type = EventSourceGenerator.GenerateListenerType(_name, _mappings);
            return (IEventSourceNotificationListener)Activator.CreateInstance(type);
        } 
    }
}
#endif
