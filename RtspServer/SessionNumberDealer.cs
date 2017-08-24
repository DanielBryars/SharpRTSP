using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspServer
{
    class SessionNumberDealer
    {
        private long _lastSessionNumber = 124;

        public long GetNewSessionNumber()
        {
            return _lastSessionNumber++;
        }
    }
}
