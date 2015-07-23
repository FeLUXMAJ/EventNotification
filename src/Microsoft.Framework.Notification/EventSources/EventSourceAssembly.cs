// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if EVENT_SOURCE_SUPPORT

using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Framework.Notification.EventSources
{
    public static class EventSourceAssembly
    {
        private static volatile int Counter = 0;

        private static AssemblyBuilder AssemblyBuilder;
        private static ModuleBuilder ModuleBuilder;

        static EventSourceAssembly()
        {
            var assemblyName = new AssemblyName("Microsoft.Framework.Notification.EventSourceAssembly");
            var access = AssemblyBuilderAccess.Run;

            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, access);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule("Microsoft.Framework.Notification.EventSourceAssembly.dll");
        }

        public static TypeBuilder DefineType(
            string name,
            TypeAttributes attributes,
            Type baseType,
            Type[] interfaces)
        {
            name = name + "_" + Counter++;
            return ModuleBuilder.DefineType(name, attributes, baseType, interfaces);
        }
    }
}
#endif