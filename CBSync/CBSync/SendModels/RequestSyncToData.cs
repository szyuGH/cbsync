﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CBSync.SendModels
{
    [Serializable]
    public struct RequestSyncToData
    {
        public string Sender { get; set; }
    }
}
