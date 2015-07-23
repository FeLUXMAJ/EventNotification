// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System.Diagnostics.Tracing;

namespace Microsoft.Framework.Notification.EventSources
{
    public interface IEventSourceNotificationListener
    {
        EventSource EventSource { get; }
    }
}

#endif
