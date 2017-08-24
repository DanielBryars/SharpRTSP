using Rtsp;
using Rtsp.Messages;
using System;
using System.Text;
using System.Threading;

namespace RtspClientExample
{
    class FreeSwitchRstpClient
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _sdp = @"v=0
o=- 0 0 IN IP4 127.0.0.1
s=No Name
c=IN IP4 172.16.6.170
t=0 0
a=tool:libavformat 56.1.0
m=audio 0 RTP/AVP 0
b=AS:64
a=control:streamid=0
m=audio 0 RTP/AVP 0
b=AS:64
a=control:streamid=1";

        private readonly RtspTcpTransport _rtspSocket = null; 
        private readonly RtspListener _rtspClient = null;        
        private readonly UDPSocket _udpPairStream0 = null;
        private readonly UDPSocket _udpPairStream1 = null;

        private readonly String _rstpUrl = "";                
        private readonly Timer _keepaliveTimer = null;

        private string _session;
        FreeSwitchClientState _freeSwitchClientState = FreeSwitchClientState.Initialised;
        int _rtpCount = 0;

        public FreeSwitchRstpClient(String url)
        {
            _logger.Debug("Connecting to " + url);
            _rstpUrl = url;

            RtspUtils.RegisterUri();
            Uri uri = new Uri(url);
            
            _rtspSocket = new RtspTcpTransport(uri.Host, uri.Port);
            
            if (_rtspSocket.Connected == false)
            {
                throw new Exception("Error - did not connect");
            }

            // Connect a RTSP Listener to the RTSP Socket (or other Stream) to send RTSP messages and listen for RTSP replies
            _rtspClient = new RtspListener(_rtspSocket);
            _rtspClient.MessageReceived += Rtsp_MessageReceived;
            _rtspClient.Start(); // start listening for messages from the server (messages fire the MessageReceived event)
            
            _udpPairStream0 = new UDPSocket(50000, 50020);
            _udpPairStream0.DataReceived += Rtp_DataReceivedStream0;
            _udpPairStream0.Start();

            _udpPairStream1 = new UDPSocket(50000, 50020);
            _udpPairStream1.DataReceived += Rtp_DataReceivedStream1;
            _udpPairStream1.Start();

            RtspRequest options_message = new RtspRequestOptions();
            options_message.RtspUri = new Uri(url);
            _rtspClient.SendMessage(options_message);
        }

        public void Stop()
        {
            SendTearDown();
            _udpPairStream0.Stop();
            _udpPairStream1.Stop();
            _rtspClient.Stop();
        }

        public void Rtp_DataReceivedStream0(object sender, RtspChunkEventArgs e)
        {
            RtspData data_received = e.Message as RtspData;
            _logger.Debug("Data Received Stream 0");
        }

        public void Rtp_DataReceivedStream1(object sender, RtspChunkEventArgs e)
        {
            RtspData data_received = e.Message as RtspData;
            _logger.Debug("Data Received Stream 1");
        }

        private void Rtsp_MessageReceived(object sender, Rtsp.RtspChunkEventArgs e)
        {
            RtspResponse message = (RtspResponse)e.Message;

            _logger.Debug("Received " + message.OriginalRequest.ToString());

            switch (_freeSwitchClientState)
            {
                case FreeSwitchClientState.Initialised:
                    SendAnnounce();
                    break;
                case FreeSwitchClientState.SentAnnounce:                    
                    _session = message.Session;
                    SentStream0Setup();
                    break;
                case FreeSwitchClientState.SentStream0Setup:
                    SentStream1Setup();
                    break;
                case FreeSwitchClientState.SentStream1Setup:
                    SendRecord();
                    break;
                case FreeSwitchClientState.SentRecord:
                    _logger.Debug("Start sending the media!!!!!!!");
                    break;
            }
            
        }

        private void SendTearDown()
        {
            RtspRequest teardown_message = new RtspRequestTeardown();
            teardown_message.RtspUri = new Uri(_rstpUrl);
            _rtspClient.SendMessage(teardown_message);
        }

        private void SendAnnounce()
        {
            var announce = new RtspRequestAnnounce();
            announce.RtspUri = new Uri(_rstpUrl);
            byte[] sdp_bytes = Encoding.ASCII.GetBytes(_sdp);

            announce.AddHeader("Content-Base: " + _rstpUrl);
            announce.AddHeader("Content-Type: application/sdp");
            announce.Data = sdp_bytes;
            announce.AdjustContentLength();

            _rtspClient.SendMessage(announce);

            _freeSwitchClientState = FreeSwitchClientState.SentAnnounce;
        }

        private void SentStream0Setup()
        {
            RtspRequestSetup stream0Setup = new RtspRequestSetup();
            stream0Setup.RtspUri = new Uri(_rstpUrl + "/streamId=0");
            stream0Setup.Session = _session;

            RtspTransport transport = null;

            transport = new RtspTransport()
            {
                LowerTransport = RtspTransport.LowerTransportType.UDP,
                IsMulticast = false,
                ClientPort = new PortCouple(_udpPairStream0.data_port, _udpPairStream0.control_port)
            };

            stream0Setup.AddTransport(transport);
            _rtspClient.SendMessage(stream0Setup);

            _freeSwitchClientState = FreeSwitchClientState.SentStream0Setup;
        }

        private void SentStream1Setup()
        {
            RtspRequestSetup stream1Setup = new RtspRequestSetup();
            stream1Setup.RtspUri = new Uri(_rstpUrl + "/streamId=1");
            stream1Setup.Session = _session;

            RtspTransport transport = null;

            transport = new RtspTransport()
            {
                LowerTransport = RtspTransport.LowerTransportType.UDP,
                IsMulticast = false,
                ClientPort = new PortCouple(_udpPairStream0.data_port, _udpPairStream0.control_port)
            };

            stream1Setup.AddTransport(transport);
            _rtspClient.SendMessage(stream1Setup);

            _freeSwitchClientState = FreeSwitchClientState.SentStream1Setup;
        }

        private void SendRecord()
        {
            var message = new RtspRequestRecord();
            message.RtspUri = new Uri(_rstpUrl);
            message.Session = _session;
            _rtspClient.SendMessage(message);
            _freeSwitchClientState = FreeSwitchClientState.SentRecord;
        }

    }
}
