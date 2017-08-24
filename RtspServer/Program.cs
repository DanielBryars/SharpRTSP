using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RtspServer
{
    class Program
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            var port = int.Parse(ConfigurationManager.AppSettings["Port"]);

            _logger.Info($"Starting to listen on {port}");
            RtspServer monServeur = new RtspServer(port);

            monServeur.StartListen();
            _logger.Info("Started");
            Console.ReadLine();
        }
    }
}
