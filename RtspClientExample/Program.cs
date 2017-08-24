using Rtsp.Messages;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtspClientExample
{
    class Program
    {
        static void Main(string[] args)
        {            
            String url = ConfigurationManager.AppSettings["RtspUrl"];
            String mode = ConfigurationManager.AppSettings["Mode"];

            switch (mode)
            {
                case "FreeSWITCH":
                    DoFreeSWITCH(url);
                    break;
                case "Nexidia":
                    DoNexidia(url);
                    break;
            }
        }

        private static void DoFreeSWITCH(string rtspUrl)
        {
            var c = new FreeSwitchRstpClient(rtspUrl);

            Console.WriteLine("FreeSWITCH Mode. Press ENTER to exit");
            Console.ReadLine();

            c.Stop();
        }

        private static void DoNexidia(string rtspUrl)
        {
            var c = new NexidiaRtspClient(rtspUrl);

            // Wait for user to terminate programme
            Console.WriteLine("Nexidia Mode. Press ENTER to exit");
            String dummy = Console.ReadLine();

            c.Stop();
        }
    }    
}
