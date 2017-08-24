using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rtsp.Messages
{
    public class RtspRequestAnnounce : RtspRequest
    {
        public RtspRequestAnnounce()
        {
            Command = "ANNOUNCE * RTSP/1.0";
        }
    }
}
