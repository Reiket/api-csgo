using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using MongoDB.Driver;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using MongoDB.Bson;

namespace TelegramBotExample
{
    public partial class Bot
    {
        private readonly TelegramBotClient botClient;
        private readonly CancellationTokenSource cts;
        private readonly HttpClient httpClient;
        private Dictionary<string, List<Match>> eventMatches;
        private Dictionary<long, string> userUsernames = new Dictionary<long, string>();
        private Dictionary<long, string> userTeams = new Dictionary<long, string>();
        private Dictionary<long, string> userPlayers = new Dictionary<long, string>();

        public Bot(string botToken)
        {
            botClient = new TelegramBotClient(botToken);
            cts = new CancellationTokenSource();
            httpClient = new HttpClient();
            MongoClient client = new MongoClient("mongodb+srv://kursova:kursova@cluster0.xwqcgys.mongodb.net/?retryWrites=true&w=majority");
            IMongoDatabase database = client.GetDatabase("test");
            usersCollection = database.GetCollection<User>("users");
            teamsUserCollection = database.GetCollection<TeamsUser>("teams");
            playersUserCollection = database.GetCollection<PlayerUser>("players");
        }
        public async Task StartAsync()
        {
            ReceiverOptions receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            botClient.StartReceiving(HandleUpdateAsync, HandlePollingErrorAsync, receiverOptions, cts.Token);

            var me = await botClient.GetMeAsync();
            Console.WriteLine($"Start listening for @{me.Username}");
        }

        public void Stop()
        {
            cts.Cancel();
        }

    }
}