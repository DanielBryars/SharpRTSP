using Rtsp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtspServer
{
    public class UDPSocket
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private UdpClient data_socket = null;
        private UdpClient control_socket = null;

        private Thread data_read_thread = null;
        private Thread control_read_thread = null;

        private int _dataPort = 50000;
        private int _controlPort = 50001;

        IPAddress data_mcast_addr;
        IPAddress control_mcast_addr;

        /// <summary>
        /// Initializes a new instance of the <see cref="UDPSocket"/> class.
        /// Creates two new UDP sockets using the start and end Port range
        /// </summary>
        public UDPSocket(int start_port, int end_port)
        {
            // open a pair of UDP sockets - one for data (video or audio) and one for the status channel (RTCP messages)
            _dataPort = start_port;
            _controlPort = start_port + 1;

            bool ok = false;
            while (ok == false && (_controlPort < end_port))
            {
                // Video/Audio port must be odd and command even (next one)
                try
                {
                    data_socket = new UdpClient(_dataPort);
                    control_socket = new UdpClient(_controlPort);
                    ok = true;
                }
                catch (SocketException)
                {
                    // Fail to allocate port, try again
                    if (data_socket != null)
                        data_socket.Close();
                    if (control_socket != null)
                        control_socket.Close();

                    // try next data or control port
                    _dataPort += 2;
                    _controlPort += 2;
                }
            }

            data_socket.Client.ReceiveBufferSize = 100 * 1024;

            control_socket.Client.DontFragment = false;
        }

        public PortCouple PortCouple
        {
            get { return new PortCouple(_dataPort, _controlPort); }
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        public void Start()
        {
            if (data_socket == null || control_socket == null)
            {
                throw new InvalidOperationException("UDP Forwader host was not initialized, can't continue");
            }

            if (data_read_thread != null)
            {
                throw new InvalidOperationException("Forwarder was stopped, can't restart it");
            }

            data_read_thread = new Thread(() => DoWorkerJob(data_socket, _dataPort));
            data_read_thread.Name = "DataPort " + _dataPort;
            data_read_thread.Start();

            control_read_thread = new Thread(() => DoWorkerJob(control_socket, _controlPort));
            control_read_thread.Name = "ControlPort " + _controlPort;
            control_read_thread.Start();
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {            
            data_socket.Close();
            control_socket.Close();
        }

        /// <summary>
        /// Occurs when message is received.
        /// </summary>
        public event EventHandler<Rtsp.RtspChunkEventArgs> DataReceived;

        /// <summary>
        /// Raises the <see cref="E:DataReceived"/> event.
        /// </summary>
        /// <param name="rtspChunkEventArgs">The <see cref="Rtsp.RtspChunkEventArgs"/> instance containing the event data.</param>
        protected void OnDataReceived(Rtsp.RtspChunkEventArgs rtspChunkEventArgs)
        {
            EventHandler<Rtsp.RtspChunkEventArgs> handler = DataReceived;

            if (handler != null)
                handler(this, rtspChunkEventArgs);
        }


        /// <summary>
        /// Does the video job.
        /// </summary>
        private void DoWorkerJob(System.Net.Sockets.UdpClient socket, int data_port)
        {

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, data_port);
            try
            {
                // loop until we get an exception eg the socket closed
                while (true)
                {
                    byte[] frame = socket.Receive(ref ipEndPoint);

                    // We have an RTP frame.
                    // Fire the DataReceived event with 'frame'
                    _logger.Debug("Received RTP data on port " + data_port);

                    Rtsp.Messages.RtspChunk currentMessage = new Rtsp.Messages.RtspData();
                    // aMessage.SourcePort = ??
                    currentMessage.Data = frame;
                    ((Rtsp.Messages.RtspData)currentMessage).Channel = data_port;


                    OnDataReceived(new Rtsp.RtspChunkEventArgs(currentMessage));

                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
        }
    }
}
