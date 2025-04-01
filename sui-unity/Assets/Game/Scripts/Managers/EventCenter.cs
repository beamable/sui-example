using System;
using System.Collections.Generic;

namespace MoeBeam.Game.Scripts.Managers
{
    //Currently supports only one parameter
    public static class EventCenter
    {
        private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();
        
        public static void ResetEventCenter()
        {
            eventDictionary.Clear();
        }

        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] = delegate { }; // Initialize the event
            }
            eventDictionary[eventName] += listener;
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;
            }
        }

        public static void InvokeEvent(string eventName, object eventData = null)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName]?.Invoke(eventData);
            }
        }
    }

}