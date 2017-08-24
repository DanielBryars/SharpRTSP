using Rtsp;
using Rtsp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RtspServer
{
    class RtspSessionStore
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly SessionNumberDealer _sessionNumberDealer = new SessionNumberDealer();

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

        //TODO: use lockless programming model.
        private readonly Object _lock = new object();

        //TODO manage this list better and remove ones which have been in too long to stop memory leaks
        private List<RtspListener> _orphanedRtspListeners = new List<RtspListener>();

        private Dictionary<string, RtspSession> _sessions = new Dictionary<string, RtspSession>();

        public void AddAndStartListener(TcpClient tcpClient)
        {
            RtspListener newListener = new RtspListener(
                        new RtspTcpTransport(tcpClient));

            lock (_lock)
            {
                _orphanedRtspListeners.Add(newListener);
            }

            newListener.MessageReceived += NewListener_MessageReceived;

            newListener.Start();
        }

        private void NewListener_MessageReceived(object sender, RtspChunkEventArgs e)
        {
            // Cast the 'sender' and 'e' into the RTSP Listener (the Socket) and the RTSP Message
            RtspListener listener = sender as Rtsp.RtspListener;
            RtspMessage message = e.Message as Rtsp.Messages.RtspMessage;

            _logger.Info("RTSP message received " + message);

            if (message is RtspRequestOptions)
            {
                RtspResponse options_response = (e.Message as Rtsp.Messages.RtspRequestOptions).CreateResponse();
                listener.SendMessage(options_response);
                return;
            }

            if (message is RtspRequestAnnounce)
            {
                RtspRequestAnnounce announceMessage = (RtspRequestAnnounce)message;

                String requested_url = announceMessage.RtspUri.ToString();
                _logger.Info("Announce for " + requested_url);

                listener.RtspListenerType = RtspListenerType.Announce;

                var callId = announceMessage.RtspUri.LocalPath.Trim('/');

                RtspSession session;

                lock (_lock)
                {
                    if (_sessions.ContainsKey(callId))
                    {
                        session = _sessions[callId];
                    }
                    else
                    {
                        session = new RtspSession(callId, _sessionNumberDealer);                        
                        _sessions[callId] = session;
                    }
                }

                session.SetAnnounceRtspListener(listener);

                RtspResponse announceResponse = announceMessage.CreateResponse();

                announceResponse.AddHeader("Content-Base: " + requested_url);
                announceResponse.AddHeader("Content-Type: application/sdp");
                announceResponse.AdjustContentLength();
                announceResponse.Session = session.AnnounceSessionId.ToString();

                listener.SendMessage(announceResponse);
                return;
            }

            if (message is RtspRequestDescribe)
            {
                var describeMessage = (RtspRequestDescribe)message;

                String requested_url = describeMessage.RtspUri.ToString();
                _logger.Info("Describe for " + requested_url);

                listener.RtspListenerType = RtspListenerType.Describe;

                var callId = describeMessage.RtspUri.LocalPath.Trim('/');

                RtspSession session;
                lock (_lock)
                {
                    if (_sessions.ContainsKey(callId))
                    {
                        session = _sessions[callId];
                    }
                    else
                    {
                        session = new RtspSession(callId, _sessionNumberDealer);
                        _sessions[callId] = session;
                    }
                }

                session.SetDescribeRtspListener(listener);

                byte[] sdp_bytes = Encoding.ASCII.GetBytes(_sdp);

                // Create the reponse to DESCRIBE
                // This must include the Session Description Protocol (SDP)
                var describe_response = describeMessage.CreateResponse();

                describe_response.AddHeader("Content-Base: " + requested_url);
                describe_response.AddHeader("Content-Type: application/sdp");
                describe_response.Data = sdp_bytes;
                describe_response.AdjustContentLength();
                describe_response.Session = session.DescribeSessionId.ToString();
                listener.SendMessage(describe_response);
                return;
            }

            if (message is RtspRequestSetup)
            {
                var setupMessage = (RtspRequestSetup)message;

                _logger.Debug($"SetupMessage:{setupMessage}");

                foreach (var transport in setupMessage.GetTransports())
                {
                    _logger.Debug($"transport:{transport}");
                }

                var setup_response = setupMessage.CreateResponse();
                switch (listener.RtspListenerType)
                {
                    case RtspListenerType.Unknown:
                        throw new Exception("No RtspListenerType Set yet");
                    //case RtspListenerType.Describe:                       
                }

                listener.SendMessage(setup_response);
            }
        }


    }
}
