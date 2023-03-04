using Bolt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mache.Networking
{
    internal class MacheGlobalEventListener : GlobalEventListener
    {
        public void Start()
        {
            BoltNetwork.AddGlobalEventListener(this);
        }

        public override void OnEvent(debugCommand evnt)
        {
            EventDispatcher.OnReceiveEvent(evnt.input, evnt.input2);
        }
    }
}
