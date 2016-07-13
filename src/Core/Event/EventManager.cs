/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Essentials.Api.Event;
using Essentials.Common;

namespace Essentials.Core.Event {

    public class EventManager : IEventManager {

        private readonly Dictionary<EventHolder, List<Delegate>> _handlerMap =
            new Dictionary<EventHolder, List<Delegate>>();

        public void RegisterAll(object instance) {
            var type = instance.GetType();

            foreach (var listenerMethod in type.GetMethods((BindingFlags) 0x3C)) {
                var eventHandlerAttrs = listenerMethod.GetCustomAttributes(typeof(SubscribeEvent), false);
                if (eventHandlerAttrs.Length < 1)
                    continue;

                var eventHandlerAttr = (SubscribeEvent) eventHandlerAttrs[0];
                var eventTarget = eventHandlerAttr.DelegateOwner;
                var targetFieldName = eventHandlerAttr.DelegateName;

                lock (_handlerMap) {
                    var holder = GetHolder(eventTarget, targetFieldName);

                    EventInfo eventInfo;
                    List<Delegate> methodDelegates;

                    if (holder == null) {
                        var eventTargetType = (Type) (eventTarget is Type
                            ? eventTarget
                            : eventTarget.GetType());

                        eventInfo = eventTargetType.GetEvent(targetFieldName);

                        holder = new EventHolder() {
                            EventInfo = eventInfo,
                            Target = eventTarget
                        };

                        methodDelegates = new List<Delegate>();
                    } else {
                        eventTarget = holder.Target;
                        eventInfo = holder.EventInfo;

                        methodDelegates = _handlerMap[holder];
                    }

                    var methodDelegate = Delegate.CreateDelegate(
                        eventInfo.EventHandlerType,
                        instance,
                        listenerMethod
                     );

                    eventInfo.AddEventHandler(eventTarget, methodDelegate);

                    methodDelegates.Add(methodDelegate);
                    _handlerMap[holder] = methodDelegates;
                }
            }
        }

        public void RegisterAll(Type type) {
            if (type.GetMethods((BindingFlags) 0x3C)
                .Any(md => md.GetCustomAttributes(typeof(SubscribeEvent), false).Length > 0)) {
                RegisterAll(EssCore.Instance.CommonInstancePool.GetOrCreate(type));
            }
        }

        public void RegisterAll<TEventType>() {
            RegisterAll(typeof(TEventType));
        }

        public void RegisterAll(Assembly asm) {
            asm.GetTypes().Where(CanHoldEvents).ForEach(RegisterAll);
        }

        public void RegisterAll(string targetNamespace) {
            GetType().Assembly.GetTypes()
                .Where(CanHoldEvents)
                .Where(t => t.Namespace.EqualsIgnoreCase(targetNamespace))
                .ForEach(RegisterAll);
        }

        public void UnregisterAll<TEventType>() {
            UnregisterAll(typeof(TEventType));
        }

        public void UnregisterAll(Type type) {
            lock (_handlerMap) {
                var unregisteredDelegates = new List<Delegate>();
                var unregisteredHolders = new List<EventHolder>();
                var handlerMapAsList = _handlerMap.ToList();

                for (var j = 0; j < handlerMapAsList.Count; j++) {
                    var handler = handlerMapAsList[j];

                    foreach (var delegateMethod in handler.Value) {
                        if (delegateMethod.Method.ReflectedType != type) continue;

                        handler.Key.EventInfo.RemoveEventHandler(handler.Key.Target, delegateMethod);
                        unregisteredDelegates.Add(delegateMethod);
                    }

                    handler.Value.RemoveAll(@delegate => unregisteredDelegates.Contains(@delegate));

                    if (handler.Value.Count == 0) unregisteredHolders.Add(handler.Key);
                }

                foreach (var holder in unregisteredHolders) {
                    _handlerMap.Remove(holder);
                }
            }
        }

        public void Unregister<T>(string methodName) {
            Unregister(typeof(T), methodName);
        }

        public void Unregister(Type type, string methodName) {
            lock (_handlerMap) {
                var unregisteredDelegates = new List<Delegate>();
                var unregisteredHolders = new List<EventHolder>();
                var handlerMapAsList = _handlerMap.ToList();

                for (var j = 0; j < handlerMapAsList.Count; j++) {
                    var handler = handlerMapAsList[j];

                    foreach (var delegateMethod in handler.Value) {
                        if (delegateMethod.Method.ReflectedType != type ||
                            !delegateMethod.Method.Name.EqualsIgnoreCase(methodName)) continue;

                        handler.Key.EventInfo.RemoveEventHandler(handler.Key.Target, delegateMethod);
                        unregisteredDelegates.Add(delegateMethod);
                    }

                    handler.Value.RemoveAll(@delegate => unregisteredDelegates.Contains(@delegate));

                    if (handler.Value.Count == 0) unregisteredHolders.Add(handler.Key);
                }

                foreach (var holder in unregisteredHolders) {
                    _handlerMap.Remove(holder);
                }
            }
        }

        public void UnregisterAll(Assembly asm) {
            asm.GetTypes().ForEach(UnregisterAll);
        }

        public void UnregisterAll(string targetNamespace) {
            GetType().Assembly.GetTypes()
                .Where(CanHoldEvents)
                .Where(t => t.Namespace.EqualsIgnoreCase(targetNamespace))
                .ForEach(RegisterAll);
        }

        private EventHolder GetHolder(object target, string fieldName) {
            lock (_handlerMap) {
                return _handlerMap.Keys.FirstOrDefault(holder => holder.Target.Equals(target) &&
                                                                 holder.EventInfo.Name.Equals(fieldName));
            }
        }

        private static bool CanHoldEvents(Type type) {
            return !type.IsAbstract && !type.ContainsGenericParameters;
        }

        public sealed class EventHolder {

            public object Target;
            public EventInfo EventInfo;

        }

    }

}