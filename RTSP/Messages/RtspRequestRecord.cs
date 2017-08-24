using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rtsp.Messages
{
    public class RtspRequestRecord : RtspRequest
    {
        public RtspRequestRecord()
        {
            Command = "RECORD * RTSP/1.0";
        }
    }
}
