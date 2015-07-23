// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Framework.Notification.EventSources;
using Xunit;

namespace Microsoft.Framework.Notification.Test
{
    public class EventSourceTest
    {
        [Fact]
        public void MapEvent_EmptyEvent()
        {
            // Arrange
            var builder = new EventSourceBuilder("MapEvent_EmptyEvent");

            builder
                .MapEvent("TestEvent", 101);

            var notificationListener = builder.CreateListener();

            var notifier = CreateNotifier();
            notifier.EnlistTarget(notificationListener);

            var listener = new TestEventSourceListener();
            listener.EnableEvents(notificationListener.EventSource, EventLevel.Verbose);

            // Act
            notifier.Notify("TestEvent", new { });

            // Assert
            var eventData = Assert.Single(listener.Written);
            Assert.Equal(eventData.EventSource.Name, "MapEvent_EmptyEvent");
            Assert.Equal(101, eventData.EventId);
            Assert.Empty(eventData.Payload);
        }

        [Fact]
        public void MapEvent_PassThroughParameter()
        {
            // Arrange
            var builder = new EventSourceBuilder("MapEvent_PassThroughParameter");

            builder
                .MapEvent("TestEvent", 101)
                .MapData<int>("a");

            var notificationListener = builder.CreateListener();

            var notifier = CreateNotifier();
            notifier.EnlistTarget(notificationListener);

            var listener = new TestEventSourceListener();
            listener.EnableEvents(notificationListener.EventSource, EventLevel.Verbose);

            // Act
            notifier.Notify("TestEvent", new { a = 5 });

            // Assert
            var eventData = Assert.Single(listener.Written);
            Assert.Equal(eventData.EventSource.Name, "MapEvent_PassThroughParameter");
            Assert.Equal(101, eventData.EventId);

            Assert.Equal(1, eventData.Payload.Count);
            Assert.Equal(5, eventData.Payload[0]);
        }

        [Fact]
        public void MapEvent_PassThroughParameters()
        {
            // Arrange
            var builder = new EventSourceBuilder("MapEvent_PassThroughParameters");

            builder
                .MapEvent("TestEvent", 101)
                .MapData<int>("a")
                .MapData<string>("hi");

            var notificationListener = builder.CreateListener();

            var notifier = CreateNotifier();
            notifier.EnlistTarget(notificationListener);

            var listener = new TestEventSourceListener();
            listener.EnableEvents(notificationListener.EventSource, EventLevel.Verbose);

            // Act
            notifier.Notify("TestEvent", new { a = 5, hi = "hello" });

            // Assert
            var eventData = Assert.Single(listener.Written);
            Assert.Equal(eventData.EventSource.Name, "MapEvent_PassThroughParameters");
            Assert.Equal(101, eventData.EventId);

            Assert.Equal(2, eventData.Payload.Count);
            Assert.Equal(5, eventData.Payload[0]);
            Assert.Equal("hello", eventData.Payload[1]);
        }

        [Fact]
        public void MapEvent_ParameterWithAdapter()
        {
            // Arrange
            var builder = new EventSourceBuilder("MapEvent_ParameterWithAdapter");

            builder
                .MapEvent("TestEvent", 101)
                .MapData<string, int>("hi", parameter => parameter.Length);

            var notificationListener = builder.CreateListener();

            var notifier = CreateNotifier();
            notifier.EnlistTarget(notificationListener);

            var listener = new TestEventSourceListener();
            listener.EnableEvents(notificationListener.EventSource, EventLevel.Verbose);

            // Act
            notifier.Notify("TestEvent", new { hi = "hello" });

            // Assert
            var eventData = Assert.Single(listener.Written);
            Assert.Equal(eventData.EventSource.Name, "MapEvent_ParameterWithAdapter");
            Assert.Equal(101, eventData.EventId);

            Assert.Equal(1, eventData.Payload.Count);
            Assert.Equal(5, eventData.Payload[0]);
        }

        [Fact]
        public void MapEvent_ProxiedParameterWithAdapters()
        {
            // Arrange
            var builder = new EventSourceBuilder("MapEvent_ProxiedParameterWithAdapters");

            builder
                .MapEvent("TestEvent", 101)
                .MapData<IFakeHttpContext, string>("httpContext", c => c.Path)
                .MapData<IFakeHttpContext, string>("httpContext", c => c.Query);

            var notificationListener = builder.CreateListener();

            var notifier = CreateNotifier();
            notifier.EnlistTarget(notificationListener);

            var listener = new TestEventSourceListener();
            listener.EnableEvents(notificationListener.EventSource, EventLevel.Verbose);

            // Act
            notifier.Notify(
                "TestEvent", 
                new
                {
                    httpContext = new FakeHttpContext()
                    {
                        Path = "/api/Albums/Search",
                        Query = "?name=smell%25the%25glove&artist=spinal%25tap"
                    }
                });

            // Assert
            var eventData = Assert.Single(listener.Written);
            Assert.Equal(eventData.EventSource.Name, "MapEvent_ProxiedParameterWithAdapters");
            Assert.Equal(101, eventData.EventId);

            Assert.Equal(2, eventData.Payload.Count);
            Assert.Equal("/api/Albums/Search", eventData.Payload[0]);
            Assert.Equal("?name=smell%25the%25glove&artist=spinal%25tap", eventData.Payload[1]);
        }

        public class FakeHttpContext
        {
            public string Path { get; set; }

            public string Query { get; set; }
        }

        public interface IFakeHttpContext
        {
            string Path { get; }

            string Query { get; }
        }

        private static INotifier CreateNotifier()
        {
            return new Notifier(new ProxyNotifierMethodAdapter());
        }

        private class TestEventSourceListener : EventListener
        {
            public List<EventWrittenEventArgs> Written { get; } = new List<EventWrittenEventArgs>();

            protected override void OnEventWritten(EventWrittenEventArgs eventData)
            {
                Written.Add(eventData);
            }
        }
    }
}
