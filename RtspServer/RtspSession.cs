using Rtsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspServer
{
    /// <summary>
    /// Represents a Recorder and Player
    /// Tracks all state
    /// </summary>
    class RtspSession
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SessionNumberDealer _sessionNumberDealer;
        private readonly string _callId;

        public RtspSession(string callId, SessionNumberDealer sessionNumberDealer)
        {
            _callId = callId;
            _sessionNumberDealer = sessionNumberDealer;
        }
 
        /// <summary>
        /// The Rtsp client which sends the RECORD method
        /// </summary>
        public RtspListener AnnounceRtspListener { get; private set; }
        public long AnnounceSessionId { get; private set; }

        public long SetAnnounceRtspListener(RtspListener rtspListener)
        {
            _logger.Info($"AnnounceRtspListener on {_callId} : {rtspListener}");
            AnnounceRtspListener = rtspListener;
            AnnounceSessionId = _sessionNumberDealer.GetNewSessionNumber();
            return AnnounceSessionId;
        }

        public string AnnounceClientPortsStream0 { get; set; }
        public string AnnounceClientPortsStream1 { get; set; }

        /// <summary>
        /// The Rtsp client which sends the PLAY method
        /// </summary>
        public RtspListener DescribeRtspListener { get; private set; }
        public long DescribeSessionId { get; private set; }

        public long SetDescribeRtspListener(RtspListener rtspListener)
        {
            _logger.Info($"DescribeRtspListener on {_callId} : {rtspListener}");
            DescribeRtspListener = rtspListener;
            DescribeSessionId = _sessionNumberDealer.GetNewSessionNumber();
            return DescribeSessionId;
        }


        public string DescribeClientPortsStream0 { get; set; }
        public string DescribeClientPortsStream1 { get; set; }


    }
}
