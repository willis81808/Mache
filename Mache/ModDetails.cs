using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mache
{
    public class ModDetails
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public Action OnMenuShow { get; set; }
        public Action OnMenuHide { get; set; }

        public Action<GameObject> OnFinishedCreating { get; set; }

        internal GameObject DetailsView { get; set; }
    }
}
