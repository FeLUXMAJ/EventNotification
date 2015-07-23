// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Framework.Notification.EventSources
{
    public static class EventSourceGenerator
    {
        private static MethodInfo[] WriteEventMethods;
        private static MethodInfo ParamsWriteEventMethod;

        static EventSourceGenerator()
        {
            WriteEventMethods = typeof(EventSource).GetRuntimeMethods().Where(m => m.Name == "WriteEvent").ToArray();

            foreach (var method in WriteEventMethods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 2 && parameters[1].ParameterType == typeof(object[]))
                {
                    ParamsWriteEventMethod = method;
                    break;
                }
            }
        }


        public static Type GenerateListenerType(string name, List<EventMapping> mappings)
        {
            var typeBuilder = EventSourceAssembly.DefineType(
                "Generated_EventSource_" + name,
                TypeAttributes.Class,
                typeof(EventSource),
                new Type[] { typeof(IEventSourceNotificationListener) });

            AddEventSourceAttribute(typeBuilder, name);
            AddEventSourceProperty(typeBuilder);

            foreach (var mapping in mappings)
            {
                AddEvent(typeBuilder, mapping);
            }

            return typeBuilder.CreateTypeInfo().AsType(); 
        }

        private static void AddEvent(TypeBuilder typeBuilder, EventMapping eventMapping)
        {
            var eventMethodParameters = eventMapping.DataMappings.Select(d => d.DestinationType).ToArray();
            var eventMethod = typeBuilder.DefineMethod(
                eventMapping.EventMethodName,
                MethodAttributes.Public,
                CallingConventions.HasThis,
                typeof(void),
                eventMethodParameters);

            var eventAttribute = new CustomAttributeBuilder(
                typeof(EventAttribute).GetConstructor(new Type[] { typeof(int) }),
                new object[] { eventMapping.EventId.Value, });
            eventMethod.SetCustomAttribute(eventAttribute);

            var il = eventMethod.GetILGenerator();

            var writeEventMethod = ResolveWriteEvent(eventMethodParameters);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, eventMapping.EventId.Value);
            for (var i = 0; i < eventMapping.DataMappings.Count; i++)
            {
                il.Emit(OpCodes.Ldarg, i + 1);
            }

            il.Emit(OpCodes.Callvirt, writeEventMethod);
            il.Emit(OpCodes.Ret);

            var listenerParameters = ResolveListenerParameters(eventMapping);
            var listenerMethod = typeBuilder.DefineMethod(
                eventMapping.NotificationMethodName,
                MethodAttributes.Public,
                CallingConventions.HasThis,
                typeof(void),
                listenerParameters.Select(p => p.Type).ToArray());

            for (var i = 0; i < listenerParameters.Length; i++)
            {
                 // parameter-0 is the "return parameter", so skip it.
                listenerMethod.DefineParameter(i + 1, ParameterAttributes.None, listenerParameters[i].Name);
            }

            var notificationNameAttributeBuilder = new CustomAttributeBuilder(
                typeof(NotificationNameAttribute).GetConstructor(new Type[] { typeof(string) }),
                new object[] { eventMapping.NotificationName });
            listenerMethod.SetCustomAttribute(notificationNameAttributeBuilder);

            var nonEventAttributeBuilder = new CustomAttributeBuilder(
                typeof(NonEventAttribute).GetConstructor(Type.EmptyTypes),
                new object[0]);
            listenerMethod.SetCustomAttribute(nonEventAttributeBuilder);

            il = listenerMethod.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);

            for (var i = 0; i < eventMapping.DataMappings.Count; i++)
            {
                var dataMapping = eventMapping.DataMappings[i];

                for (var j = 0; j < listenerParameters.Length; j++)
                {
                    if (dataMapping.SourceName == listenerParameters[j].Name)
                    {
                        if (dataMapping.Mapping == null)
                        {
                            il.Emit(OpCodes.Ldarg, j + 1);
                        }
                        else
                        {
#if NET45
                            var adapaterMethod = typeBuilder.DefineMethod(
                                Guid.NewGuid().ToString(),
                                MethodAttributes.Static,
                                CallingConventions.Standard,
                                dataMapping.SourceType,
                                new Type[] { dataMapping.DestinationType });

                            dataMapping.Mapping.CompileToMethod(adapaterMethod);

                            il.Emit(OpCodes.Ldarg, j + 1);
                            il.Emit(OpCodes.Call, adapaterMethod);
#else
                            var delegateType = typeof(Func<,>).MakeGenericType(dataMapping.SourceType, dataMapping.DestinationType);
                            var @delegate = dataMapping.Mapping.Compile();
                            il.Emit(OpCodes.Ldarg, j + 1);
                            il.Emit(OpCodes.Call, @delegate.GetMethodInfo());
#endif


                        }
                    }
                }
            }

            il.Emit(OpCodes.Callvirt, eventMethod);
            il.Emit(OpCodes.Ret);
        }

        private static void AddEventSourceAttribute(TypeBuilder typeBuilder, string name)
        {
            var attributeBuilder = new CustomAttributeBuilder(
                typeof(EventSourceAttribute).GetConstructor(Type.EmptyTypes),
                new object[0],
                new PropertyInfo[]
                {
                    typeof(EventSourceAttribute).GetProperty("Name"),
                },
                new object[] { name, });

            typeBuilder.SetCustomAttribute(attributeBuilder);
        }

        private static void AddEventSourceProperty(TypeBuilder typeBuilder)
        {
            var propertyBuilder = typeBuilder.DefineProperty(
                nameof(IEventSourceNotificationListener.EventSource),
                PropertyAttributes.None,
                typeof(EventSource),
                Type.EmptyTypes);

            var methodBuilder = typeBuilder.DefineMethod(
                "get_" + nameof(IEventSourceNotificationListener.EventSource),
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.HasThis,
                typeof(EventSource),
                Type.EmptyTypes);

            // "return this"
            var il = methodBuilder.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(methodBuilder);

            var property = typeof(IEventSourceNotificationListener).GetProperty(propertyBuilder.Name);
            typeBuilder.DefineMethodOverride(methodBuilder, property.GetMethod);
        }

        private static MethodInfo ResolveWriteEvent(Type[] types)
        {
            var methods = WriteEventMethods;

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                if (parameters.Length - 1 == types.Length)
                {
                    var i = 0;
                    var isMatch = true;
                    foreach (var parameter in parameters.Skip(1))
                    {
                        if (parameter.ParameterType != types[i++])
                        {
                            isMatch = false;
                            break;
                        }
                    }

                    if (isMatch)
                    {
                        return method;
                    }
                }
            }

            return ParamsWriteEventMethod;
        }

        private static ParameterDefintion[] ResolveListenerParameters(EventMapping eventMapping)
        {
            var parameters = new Dictionary<string, ParameterDefintion>();
            foreach (var dataMapping in eventMapping.DataMappings)
            {
                parameters[dataMapping.SourceName] = new ParameterDefintion()
                {
                    Name = dataMapping.SourceName,
                    Type = dataMapping.SourceType,
                };
            }

            return parameters.Values.ToArray();
        }

        private struct ParameterDefintion
        {
            public Type Type;
            public string Name;
        }
    }
}

#endif
