using System;
using TLSharp;

namespace Api
{
    class Program
    {
        private static readonly string _apiIdKey = "api_id";
        private static readonly string _apiHashKey = "api_hash";
        private static TelegramClient _client = null;
        static void Main(string[] args)
        {

            _client = CreateClient();
            _client.ConnectAsync().Wait();
            //_client.
            TLSchema.Channels.TLChannelParticipants p = new TLSchema.Channels.TLChannelParticipants();
            //TLSchema.TLAbsChat chat = new TLSchema.TLAbsChat();

            Console.WriteLine("Hello World!");
        }

        private static TelegramClient CreateClient()
        {
            int apiId = Int32.Parse(Environment.GetEnvironmentVariable(_apiIdKey));
            string apiHash = Environment.GetEnvironmentVariable(_apiHashKey);
            return new TelegramClient(apiId, apiHash);
            
        }


    }
}
