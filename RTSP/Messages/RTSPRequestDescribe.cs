using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rtsp.Messages
{
    public class RtspRequestDescribe : RtspRequest
    {
        public RtspRequestDescribe()
        {
            Command = "DESCRIBE * RTSP/1.0";
        }
    }
}
