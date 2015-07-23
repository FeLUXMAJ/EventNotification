// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;

namespace Microsoft.Framework.Notification.EventSources
{
    public class EventDataMapping
    {
        public string SourceName { get; set; }

        public Type SourceType { get; set; }

        public Type DestinationType { get; set; }

        public LambdaExpression Mapping { get; set; }
    }
}