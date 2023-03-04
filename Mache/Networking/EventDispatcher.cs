using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mache.Networking
{
    public class EventDispatcher
    {
        private static List<EventRegistration> eventData = new List<EventRegistration>();

        private class EventRegistration
        {
            public string EventId { get; set; }
            public Func<object, string> SerializeData { get; set; }
            public Func<string, object> DeserializeData { get; set; }
            public Action<object> OnEventReceived { get; set; }
        }

        public static void RegisterEvent<T>(string eventId) where T : SimpleEvent<T>
        {
            RegisterEvent(eventId, SimpleEvent<T>.Serialize, SimpleEvent<T>.Deserialize, SimpleEvent<T>.OnEventRaised);
        }

        public static void RegisterEvent(string eventId, Func<object, string> serializeEventData, Func<string, object> deserializeEventData, Action<object> onEventReceived)
        {
            if (eventData.Any(d => d.EventId == eventId))
            {
                MachePlugin.Instance.Log.LogError($"A NetworkEvent has already been registered with the id: {eventId}");
                return;
            }

            var registration = new EventRegistration
            {
                EventId = eventId,
                SerializeData = serializeEventData,
                DeserializeData = deserializeEventData,
                OnEventReceived = onEventReceived
            };

            eventData.Add(registration);
        }

        public static void RaiseEvent(string eventId, object payload)
        {
            if (eventData.FirstOrDefault(d => d.EventId == eventId) is EventRegistration data)
            {
                var serialized = data.SerializeData?.Invoke(payload);

                var eventToRaise = debugCommand.Raise(Bolt.GlobalTargets.Everyone);
                eventToRaise.input = eventId;
                eventToRaise.input2 = serialized;

                eventToRaise.Send();
            }
        }

        internal static void OnReceiveEvent(string id, string payload)
        {
            if (eventData.FirstOrDefault(d => d.EventId == id) is EventRegistration data)
            {
                var result = data.DeserializeData?.Invoke(payload);
                data.OnEventReceived?.Invoke(result);
            }
        }
    }
}
