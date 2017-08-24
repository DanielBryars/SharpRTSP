using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspClientExample
{
    enum FreeSwitchClientState
    {
        Initialised,
        SentAnnounce,
        SentStream0Setup,
        SentStream1Setup,
        SentRecord
    }
}
