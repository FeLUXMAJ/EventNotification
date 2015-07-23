// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Microsoft.Framework.Notification.EventSources
{
    public static class EventMappingBuilderExtensions
    {
        public static IEventMappingBuilder MapData<T>(
            this IEventMappingBuilder builder,
            string parameter)
        {
            return MapData<T, T>(builder, parameter, func: null);
        }

        public static IEventMappingBuilder MapData<T, U>(
            this IEventMappingBuilder builder,
            string parameter,
            Expression<Func<T, U>> func)
        {
            var mapping = new EventDataMapping()
            {
                DestinationType = typeof(U),
                Mapping = func,
                SourceName = parameter,
                SourceType = typeof(T),
            };

            builder.AddDataMapping(mapping);
            return builder;
        }
    }
}

#endif