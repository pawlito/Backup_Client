using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
namespace NASClientTCP
{
    class PingServer
    {
        static IProgress<string> progress;
        public PingServer()
        {

        }
        public PingServer(IProgress<string> Progress)
        {
            
            progress = Progress;
        }

        public async Task TestPing()
        {
            Ping pinger = new Ping();
            PingReply reply = await pinger.SendPingAsync("127.0.0.1");
            DisplayPingReplyInfo(reply);
            pinger.PingCompleted += pinger_PingCompleted;
            pinger.SendAsync("127.0.0.1", "backup server ping");
        }

        private static void DisplayPingReplyInfo(PingReply reply)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("results from pinging" + reply.Address.ToString()).AppendLine();
            builder.Append($"\tFragmentation allowed: {!reply.Options.DontFragment}");
            builder.Append($"\tTime to live: {reply.Options.Ttl}");
            builder.Append($"\tRoundtrip took: {reply.RoundtripTime}");
            builder.Append($"\tStatus: {reply.Status.ToString()}");
            progress.Report(builder.ToString());

        }

        private static void pinger_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            PingReply reply = e.Reply;
            //DisplayPingReplyInfo(reply);
            if (e.Cancelled)
                Console.WriteLine($"Ping for {e.UserState.ToString()} was cancelled");
            else
                Console.WriteLine($"Exception thrown during ping: {e.Error?.ToString()}");
        }
    }
}
