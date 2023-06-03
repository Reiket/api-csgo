using System;

namespace TelegramBotExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string botToken = "6044068913:AAHXDBz5Viv0CrN8FAmm6eaicPLRnbGtMN4"; 

            var bot = new Bot(botToken);
            bot.StartAsync().Wait();

            Console.WriteLine("Bot started. Press Enter to stop...");
            Console.ReadLine();

            bot.Stop();

            Console.WriteLine("Bot stopped. Press any key to exit...");
            Console.ReadKey();
        }
    }
}






