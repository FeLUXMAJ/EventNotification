// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Reflection;
using Xunit;

namespace Microsoft.Framework.Notification.EventSources
{
    public class EventSourceGeneratorTest
    {
        [Fact]
        public void GenerateListenerType_Empty()
        {
            // Arrange
            var name = "TestName";
            var mappings = new List<EventMapping>();

            // Act
            var listenerType = EventSourceGenerator.GenerateListenerType(name, mappings);

            // Assert
            var listener = (IEventSourceNotificationListener)Activator.CreateInstance(listenerType);
            Assert.Equal(name, listener.EventSource.Name);
            Assert.Equal(new Guid("1cc2314e-2604-5195-deaa-9fd626af0e2d"), listener.EventSource.Guid);
        }

        [Fact]
        public void GenerateListenerType_EmptyEvent()
        {
            // Arrange
            var name = "TestName";
            var mappings = new List<EventMapping>()
            {
                new EventMapping()
                {
                    EventId = 101,
                    EventName = "TestEvent",
                    NotificationName = "Microsoft.Framework.TestEvents.NumberOne",
                },
            };

            // Act
            var listenerType = EventSourceGenerator.GenerateListenerType(name, mappings);

            // Assert
            var notificationMethod = listenerType.GetTypeInfo().GetDeclaredMethod(mappings[0].NotificationMethodName);
            Assert.NotNull(notificationMethod);

            var notificationNameAttribute = notificationMethod.GetCustomAttribute<NotificationNameAttribute>();
            Assert.NotNull(notificationNameAttribute);
            Assert.Equal(mappings[0].NotificationName, notificationNameAttribute.Name);

            var nonEventAttribute = notificationMethod.GetCustomAttribute<NonEventAttribute>();
            Assert.NotNull(nonEventAttribute);

            var eventMethod = listenerType.GetTypeInfo().GetDeclaredMethod(mappings[0].EventMethodName);
            Assert.NotNull(eventMethod);

            var eventAttribute = eventMethod.GetCustomAttribute<EventAttribute>();
            Assert.NotNull(eventAttribute);
            Assert.Equal(mappings[0].EventId.Value, eventAttribute.EventId);
        }

        [Fact]
        public void GenerateListenerType_EventWithParameter()
        {
            // Arrange
            var name = "TestName";
            var mappings = new List<EventMapping>()
            {
                new EventMapping()
                {
                    EventId = 101,
                    EventName = "TestEvent",
                    NotificationName = "Microsoft.Framework.TestEvents.NumberOne",
                    DataMappings = new List<EventDataMapping>()
                    {
                        new EventDataMapping()
                        {
                            SourceType = typeof(int),
                            SourceName = "a",
                            DestinationType = typeof(int),
                        }
                    }
                },
            };

            // Act
            var listenerType = EventSourceGenerator.GenerateListenerType(name, mappings);

            // Assert
            var notificationMethod = listenerType.GetTypeInfo().GetDeclaredMethod(mappings[0].NotificationMethodName);
            Assert.NotNull(notificationMethod);

            var parameters = notificationMethod.GetParameters();

            var parameter = Assert.Single(parameters);
            Assert.Equal("a", parameter.Name);
            Assert.Equal(typeof(int), parameter.ParameterType);

            var eventMethod = listenerType.GetTypeInfo().GetDeclaredMethod(mappings[0].EventMethodName);
            Assert.NotNull(eventMethod);

            parameters = eventMethod.GetParameters();

            parameter = Assert.Single(parameters);
            Assert.Equal(typeof(int), parameter.ParameterType);
        }
    }
}
