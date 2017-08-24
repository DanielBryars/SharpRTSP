using System;
using System.Diagnostics.Contracts;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Rtsp;

namespace RtspServer
{    
    public class RtspServer : IDisposable
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly RtspSessionStore _rtspSessionStore = new RtspSessionStore();

        private TcpListener _RTSPServerListener;
        private ManualResetEvent _Stopping;
        private Thread _ListenTread;

        /// <summary>
        /// Initializes a new instance of the <see cref="RTSPServer"/> class.
        /// </summary>
        /// <param name="aPortNumber">A numero port.</param>
        public RtspServer(int portNumber)
        {
            if (portNumber < System.Net.IPEndPoint.MinPort || portNumber > System.Net.IPEndPoint.MaxPort)
                throw new ArgumentOutOfRangeException("aPortNumber", portNumber, "Port number must be between System.Net.IPEndPoint.MinPort and System.Net.IPEndPoint.MaxPort");
            Contract.EndContractBlock();

            RtspUtils.RegisterUri();
            _RTSPServerListener = new TcpListener(IPAddress.Any, portNumber);
        }

        /// <summary>
        /// Starts the listen.
        /// </summary>
        public void StartListen()
        {
            _RTSPServerListener.Start();
            
            _Stopping = new ManualResetEvent(false);
            _ListenTread = new Thread(new ThreadStart(AcceptConnection));
            _ListenTread.Start();
        }

        /// <summary>
        /// Accepts the connection.
        /// </summary>
        private void AcceptConnection()
        {
            try
            {
                while (!_Stopping.WaitOne(0))
                {
                    TcpClient oneClient = _RTSPServerListener.AcceptTcpClient();
                    _rtspSessionStore.AddAndStartListener(oneClient);
                }
            }
            catch (SocketException error)
            {
                _logger.Warn("Got an error listening, I have to handle the stopping which also throw an error", error);
            }
            catch (Exception error)
            {
                _logger.Error("Got an error listening...", error);
                throw;
            }
        }

      
        public void StopListen()
        {
            _RTSPServerListener.Stop();
            _Stopping.Set();
            _ListenTread.Join();
        }

        #region IDisposable Membres

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopListen();
                _Stopping.Dispose();
            }
        }

        #endregion
    }

    public class RTPSession
    {
        public Rtsp.RtspListener listener = null;  // The RTSP client connection
        public UInt16 sequence_number = 1;         // 16 bit RTP packet sequence number used with this client connection
        public String session_id = "";             // RTSP Session ID used with this client connection
        public uint ssrc = 1;                       // SSRC value used with this client connection
        public bool play = false;                  // set to true when Session is in Play mode
        public Rtsp.Messages.RtspTransport client_transport; // Transport: string from the client to the server
        public Rtsp.Messages.RtspTransport transport_reply; // Transport: reply from the server to the client

    }
}
