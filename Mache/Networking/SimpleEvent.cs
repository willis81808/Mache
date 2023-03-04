using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mache.Networking
{
    [Serializable]
    public abstract class SimpleEvent<T>
    {
        public abstract void OnReceived();

        public static void OnEventRaised(object evnt)
        {
            ((SimpleEvent<T>)evnt).OnReceived();
        }

        public static string Serialize(object evnt)
        {
            return JsonSerializer.Serialize(evnt);
        }

        public static T Deserialize(string payload)
        {
            return JsonSerializer.Deserialize<T>(payload);
        }
    }
}
