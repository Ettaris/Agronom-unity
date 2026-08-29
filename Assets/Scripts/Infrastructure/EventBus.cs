using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public static class EventBus
    {
        private static readonly Dictionary<Type, Delegate> _events = new Dictionary<Type, Delegate>(32);

        public static void Subscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var existing))
                _events[type] = Delegate.Combine(existing, listener);
            else
                _events[type] = listener;
        }

        public static void Unsubscribe<T>(Action<T> listener) where T : struct
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var existing))
            {
                var newDelegate = Delegate.Remove(existing, listener);
                if (newDelegate == null)
                    _events.Remove(type);
                else
                    _events[type] = newDelegate;
            }
        }

        public static void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            if (_events.TryGetValue(type, out var delegateObj))
            {
                (delegateObj as Action<T>)?.Invoke(eventData);
            }
        }
    }
}