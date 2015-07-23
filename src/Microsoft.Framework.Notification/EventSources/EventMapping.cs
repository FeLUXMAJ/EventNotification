// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System.Collections.Generic;

namespace Microsoft.Framework.Notification.EventSources
{
    public class EventMapping
    {
        public List<EventDataMapping> DataMappings { get; set; } = new List<EventDataMapping>();

        public int? EventId { get; set; }

        public string EventName { get; set; }

        public string EventMethodName
        {
            get
            {
                if (EventName == null)
                {
                    return EventName;
                }
                else
                {
                    return "Event_" + EventName;
                }
            }
        }

        public string NotificationName { get; set; }

        public string NotificationMethodName
        {
            get
            {
                if (NotificationName == null)
                {
                    return null;
                }
                else if (NotificationName.LastIndexOf('.') == -1)
                {
                    return "Notification_" + NotificationName;
                }
                else
                {
                    return "Notification_" + NotificationName.Substring(NotificationName.LastIndexOf('.') + 1);
                }
            }
        }
    }
}

#endif