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
        private readonly UDPSocket _udpPairAnnounceStream0 = null;
        private readonly UDPSocket _udpPairAnnounceStream1 = null;
        private readonly UDPSocket _udpPairDescribeStream0 = null;
        private readonly UDPSocket _udpPairDescribeStream1 = null;

        public RtspSession(string callId, SessionNumberDealer sessionNumberDealer)
        {
            _callId = callId;
            _sessionNumberDealer = sessionNumberDealer;

            _udpPairAnnounceStream0 = new UDPSocket(54000, 54020);
            _udpPairAnnounceStream0.DataReceived += _udpPairAnnounceStream0_DataReceived; ;
            _udpPairAnnounceStream0.Start();

            _udpPairAnnounceStream1 = new UDPSocket(54002, 54022);
            _udpPairAnnounceStream1.DataReceived += _udpPairAnnounceStream1_DataReceived;  
            _udpPairAnnounceStream1.Start();

            _udpPairDescribeStream0 = new UDPSocket(54004, 54024);
            _udpPairDescribeStream0.DataReceived += _udpPairDescribeStream0_DataReceived; 
            _udpPairDescribeStream0.Start();

            _udpPairDescribeStream1 = new UDPSocket(54006, 54026);
            _udpPairDescribeStream1.DataReceived += _udpPairDescribeStream1_DataReceived;
            _udpPairDescribeStream1.Start();
        }

        private void _udpPairDescribeStream1_DataReceived(object sender, RtspChunkEventArgs e)
        {
        }

        private void _udpPairDescribeStream0_DataReceived(object sender, RtspChunkEventArgs e)
        {
        }

        private void _udpPairAnnounceStream1_DataReceived(object sender, RtspChunkEventArgs e)
        {            
        }

        private void _udpPairAnnounceStream0_DataReceived(object sender, RtspChunkEventArgs e)
        {
            
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

        public UDPSocket UdpPairAnnounceStream0
        {
            get
            {
                return _udpPairAnnounceStream0;
            }
        }

        public UDPSocket UdpPairAnnounceStream1
        {
            get
            {
                return _udpPairAnnounceStream1;
            }
        }

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

        public UDPSocket UdpPairDescribeStream0
        {
            get
            {
                return _udpPairDescribeStream0;
            }
        }

        public UDPSocket UdpPairDescribeStream1
        {
            get
            {
                return _udpPairDescribeStream1;
            }
        }
    }
}
