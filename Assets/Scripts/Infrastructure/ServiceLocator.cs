using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>(32);

        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            Debug.Log("register - " + type);
            if (_services.ContainsKey(type))
                throw new InvalidOperationException($"Service {type} already registered.");
            _services[type] = service;
        }

        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            _services.Remove(type);
        }

        public static T Get<T>() where T : class
        {

            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
                return (T)service;
            throw new InvalidOperationException($"Service {type} not registered.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var obj))
            {
                service = (T)obj;
                return true;
            }
            service = null;
            return false;
        }
    }
}