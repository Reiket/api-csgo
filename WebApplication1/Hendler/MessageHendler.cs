using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExample
{
    public partial class Bot
    {

        private IMongoCollection<User> usersCollection;
        private IMongoCollection<TeamsUser> teamsUserCollection;
        private IMongoCollection<PlayerUser> playersUserCollection;

        async Task SendMessageAsync(ITelegramBotClient botClient, long chatId, List<string> messages, ReplyKeyboardMarkup? replyMarkup = null)
        {
            foreach (string message in messages)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    replyMarkup: replyMarkup
                );
            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message is not { } message)
                return;
            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");
            switch (messageText.ToLower())
            {
                case "/start":
                    await SendMessageAsync(botClient, chatId, new List<string> { "Добрий день! Це Telegram Bot по грі CS:GO. Якщо ви хочете продовжити, натисніть потрібну вам кнопку." },
                        replyMarkup: CreateKeyboardMarkup());
                    break;
                case "/help":
                    await SendMessageAsync(botClient, chatId, new List<string> { "Я бот, який надає інформацію про КС ГО. Ось деякі команди, які ви можете використовувати: \n /start - Почати спілкування \n Розклад мачів - Отримати розклад майбутніх матчів \n Рейтинг команд - Отримати рейтинг команд\nРезультати мачів- Отримати результати  мачів\n Мій Профіль - Інформація про тебе\n  \n /help - Відобразити цей довідник" },
                    replyMarkup: CreateKeyboardMarkup());

                    break;
                case "":
                    await SendMessageAsync(botClient, chatId, new List<string> { "Введіть команду /help" },
                    replyMarkup: CreateKeyboardMarkup());
                    break;
                case "⏪ назад":
                    await SendMessageAsync(botClient, chatId, new List<string> { "Головне меню:" }, replyMarkup: CreateKeyboardMarkup());
                    break;
                case "рейтинг команди":
                    string teamRankings = await GetTeamRankingsAsync();
                    await SendMessageAsync(botClient, chatId, new List<string> { teamRankings });
                    break;
                case "результати мачів":
                    List<Match> teamResults = await GetTeamResultsAsync();
                    eventMatches = GroupMatchesByEvent(teamResults);
                    var eventResButtons = GetEventButtons(teamResults);
                    await SendMessageAsync(botClient, chatId, new List<string> { "Оберіть турнір:" }, replyMarkup: new ReplyKeyboardMarkup(eventResButtons));
                    break;
                case "розклад мачів":
                    List<Match> matches = await GetScheduleAsync();
                    eventMatches = GroupMatchesByEvent(matches);
                    var eventButtons = GetEventButtons(matches);
                    await SendMessagesAsync(botClient, chatId, new List<string> { "Оберіть турнір:" }, replyMarkup: new ReplyKeyboardMarkup(eventButtons));
                    break;
                case "обновити":
                    await SendMessageAsync(botClient, chatId, new List<string> { "Введіть що вам потрібно оновити:\n /newname нік\n /newteam команда\n /newplayer гравець" });
                    break;
                case "мій профіль":
                    List<KeyboardButton[]> profileButtons = new List<KeyboardButton[]>
                {
                    new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
                    new KeyboardButton[] { new KeyboardButton("Нікнейм") },
                    new KeyboardButton[] { new KeyboardButton("Улюблена команда") },
                    new KeyboardButton[] { new KeyboardButton("Улюблений гравець") }
                };
                    await SendMessageAsync(botClient, chatId, new List<string> { "Виберіть одну з категорій:" }, replyMarkup: new ReplyKeyboardMarkup(profileButtons));
                    break;
                case "нікнейм":
                    string userUsername = await GetUsername(chatId);
                    if (!string.IsNullOrEmpty(userUsername))
                    {
                        List<KeyboardButton[]> usernameButtons = new List<KeyboardButton[]>
                        {
                            new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
                            new KeyboardButton[] { new KeyboardButton("Обновити") },
                            new KeyboardButton[] { new KeyboardButton("Видалити username") }
                        };
                        await SendMessageAsync(botClient, chatId, new List<string> { $"Ваш нікнейм: '{userUsername}'" }, replyMarkup: new ReplyKeyboardMarkup(usernameButtons));
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть свій нікнейм у вигляді '/username нік'" });
                    }
                    break;
                case "улюблений гравець":
                    string userPlayer = await GetPlayers(chatId);
                    if (!string.IsNullOrEmpty(userPlayer))
                    {
                        List<KeyboardButton[]> usernameButtons = new List<KeyboardButton[]>
                        {
                            new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
                            new KeyboardButton[] { new KeyboardButton("Обновити") },
                            new KeyboardButton[] { new KeyboardButton("Видалити улюбленого гравця") }
                        };
                        await SendMessageAsync(botClient, chatId, new List<string> { $"Ваша улюблений гравець: '{userPlayer}'" }, replyMarkup: new ReplyKeyboardMarkup(usernameButtons));
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть свого улюбленого гравця  у вигляді '/player гравець'" });
                    }
                    break;
                case "улюблена команда":
                    string userTeams = await GetTeams(chatId);
                    if (!string.IsNullOrEmpty(userTeams))
                    {
                        List<KeyboardButton[]> usernameButtons = new List<KeyboardButton[]>
                        {
                            new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
                            new KeyboardButton[] { new KeyboardButton("Обновити") },
                            new KeyboardButton[] { new KeyboardButton("Видалити улюблену команду") }
                        };
                        await SendMessageAsync(botClient, chatId, new List<string> { $"Ваша улюблена команда: '{userTeams}'" }, replyMarkup: new ReplyKeyboardMarkup(usernameButtons));
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть свою улюблену у вигляді '/team команда'" });
                    }
                    break;
                case "видалити username":
                    string userName = await GetUsername(chatId);
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var apiUrl = $"http://localhost:5156/api/user?username={userName}";
                        var successMessage = $"Username '{userName}' був успішно видалений";
                        var errorMessage = "Помилка під час видалення username";
                        await HandleDeleteAction(botClient, chatId, apiUrl, successMessage, errorMessage);
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть свій нікнейм у вигляді '/username нік'" });
                    }
                    break;
                case "видалити улюбленого гравця":
                    string userPlayers = await GetPlayers(chatId);
                    if (!string.IsNullOrEmpty(userPlayers))
                    {
                        var apiUrl = $"http://localhost:5156/api/players?players={userPlayers}";
                        var successMessage = $"Улюблений гравець '{userPlayers}' був успішно видалений";
                        var errorMessage = "Помилка під час видалення гравця";
                        await HandleDeleteAction(botClient, chatId, apiUrl, successMessage, errorMessage);
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть cвого улюбленого гравця у вигляді '/players гравець'" });
                    }
                    break;

                case "видалити улюблену команду":
                    string favTeam = await GetTeams(chatId);
                    if (!string.IsNullOrEmpty(favTeam))
                    {
                        var apiUrl = $"http://localhost:5156/api/team?teams={favTeam}";
                        var successMessage = $"Улюблена команда '{favTeam}' була успішно видалена";
                        var errorMessage = "Помилка під час видалення команди";
                        await HandleDeleteAction(botClient, chatId, apiUrl, successMessage, errorMessage);
                    }
                    else
                    {
                        await SendMessageAsync(botClient, chatId, new List<string> { "Будь ласка, введіть свою улюблену команду у вигляді '/team команда'" });
                    }
                    break;

                default:
                    string[] words = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    List<KeyboardButton[]> buttons = new List<KeyboardButton[]>
                        {
                            new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
                            new KeyboardButton[] { new KeyboardButton("Нікнейм") },
                            new KeyboardButton[] { new KeyboardButton("Улюблена команда") },
                            new KeyboardButton[] { new KeyboardButton("Улюблений гравець") }
                        };

                    if (messageText.StartsWith("/username"))
                    {
                        if (words.Length >= 2)
                        {
                            string newUsername = words[1];
                            await SaveUsername(chatId, newUsername);
                            await SendMessageAsync(botClient, chatId, new List<string> { $"Ваш нікнейм '{newUsername}' був збережений." }, replyMarkup: new ReplyKeyboardMarkup(buttons));
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення нікнейму. Будь ласка, введіть команду у форматі '/username [нікнейм]'." });
                        }
                    }
                    else if (messageText.StartsWith("/newname"))
                    {
                        if (words.Length >= 2)
                        {
                            string newUsername = words[1];
                            await UpdateUsernameAsync(chatId, newUsername);
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення нікнейму. Будь ласка, введіть команду у форматі '/newname [нікнейм]'." });
                        }
                    }
                    else if (messageText.StartsWith("/team"))
                    {
                        if (words.Length >= 2)
                        {
                            string newTeam = words[1];
                            await SaveTeam(chatId, newTeam);
                            await SendMessageAsync(botClient, chatId, new List<string> { $"Ваша команда '{newTeam}' була збережена." }, replyMarkup: new ReplyKeyboardMarkup(buttons));
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення команди. Будь ласка, введіть команду у форматі '/team [команда]'." });
                        }
                    }
                    else if (messageText.StartsWith("/newteam"))
                    {
                        if (words.Length >= 2)
                        {
                            string newTeam = words[1];
                            await UpdateTeamAsync(chatId, newTeam);
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення команди. Будь ласка, введіть команду у форматі '/newteam [команда]'." });
                        }
                    }
                    else if (messageText.StartsWith("/player"))
                    {
                        if (words.Length >= 2)
                        {
                            string newPlayer = words[1];
                            await SavePlayer(chatId, newPlayer);
                            await SendMessageAsync(botClient, chatId, new List<string> { $"Ваш улюблений гравець '{newPlayer}' був збережений." }, replyMarkup: new ReplyKeyboardMarkup(buttons));
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення гравця. Будь ласка, введіть команду у форматі '/player [гравець]'." });
                        }
                    }
                    else if (messageText.StartsWith("/newplayer"))
                    {
                        if (words.Length >= 2)
                        {
                            string newPlayer = words[1];
                            await UpdatePlayerAsync(chatId, newPlayer);
                        }
                        else
                        {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректне введення гравця. Будь ласка, введіть команду у форматі '/newplayer [гравець]'." });
                        }
                    }
                    else if (eventMatches != null && eventMatches.ContainsKey(messageText) && eventMatches[messageText] != null)
                    {
                        List<Match> selectedMatches = eventMatches[messageText];
                        await SendMessageAsync(botClient, chatId, new List<string> { GetMatchesText(selectedMatches) });
                    }
                    else
                    {
                            await SendMessageAsync(botClient, chatId, new List<string> { "Некоректна команда. Будь ласка, введіть команду /help" });
                    }

                    break;
            }
        }

        private async Task<string> GetUsername(long chatId)
        {
            var user = await usersCollection.Find(u => u.ChatId == chatId).FirstOrDefaultAsync();
            return user?.Username;
        }
        private async Task<string> GetTeams(long chatId)
        {
            var teamsUsers = await teamsUserCollection.Find(u => u.ChatId == chatId).FirstOrDefaultAsync();
            return teamsUsers?.Name;
        }
        private async Task<string> GetPlayers(long chatId)
        {
            var playersUsers = await playersUserCollection.Find(u => u.ChatId == chatId).FirstOrDefaultAsync();
            return playersUsers?.Name;
        }
        private async Task HandleDeleteAction(ITelegramBotClient botClient, long chatId, string apiUrl, string successMessage, string errorMessage)
        {
            var deleteResult = await DeleteDataFromApi(apiUrl, successMessage, errorMessage);

            List<KeyboardButton[]> updateButtons = new List<KeyboardButton[]>
    {
        new KeyboardButton[] { new KeyboardButton("⏪ Назад") },
        new KeyboardButton[] { new KeyboardButton("Нікнейм") },
        new KeyboardButton[] { new KeyboardButton("Улюблена команда") },
        new KeyboardButton[] { new KeyboardButton("Улюблений гравець") }
    };

            await SendMessageAsync(botClient, chatId, new List<string> { deleteResult }, replyMarkup: new ReplyKeyboardMarkup(updateButtons));
        }
        private string GetMatchesText(List<Match> matches)
        {
            StringBuilder sb = new StringBuilder();

            foreach (Match match in matches)
            {
                sb.AppendLine("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
                sb.AppendLine($"⏰ Time: {match.Time}");
                sb.AppendLine($"📌 Event: {match.Event.Name}");
                sb.AppendLine($"🗺 Maps: {match.Maps}");
                sb.AppendLine("🎉 Teams:");

                foreach (Team team in match.Teams)
                {
                    sb.AppendLine($"📍{team.Name}");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
        private string GetTeamsInfo(List<Team> teams)
        {
            string teamsInfo = "";
            foreach (Team team in teams)
            {
                string teamInfo = $"Грають: {team.Name}\n";
                teamsInfo += teamInfo;
            }
            return teamsInfo;
        }

        private Dictionary<string, List<Match>> GroupMatchesByEvent(List<Match> matches)
        {
            Dictionary<string, List<Match>> groupedMatches = new Dictionary<string, List<Match>>();

            foreach (Match match in matches)
            {
                string eventName = match.Event.Name;

                if (groupedMatches.ContainsKey(eventName))
                {
                    groupedMatches[eventName].Add(match);
                }
                else
                {
                    groupedMatches[eventName] = new List<Match> { match };
                }
            }

            return groupedMatches;
        }
        private List<KeyboardButton[]> GetEventButtons(List<Match> matches)
        {
            List<KeyboardButton[]> eventButtons = new List<KeyboardButton[]>();

            // Додаємо кнопку "Назад"
            eventButtons.Add(new KeyboardButton[] { new KeyboardButton("⏪ Назад") });

            Dictionary<string, List<Match>> groupedMatches = GroupMatchesByEvent(matches);

            foreach (var eventName in groupedMatches.Keys)
            {
                eventButtons.Add(new KeyboardButton[] { new KeyboardButton(eventName) });
            }

            return eventButtons;
        }

        private ReplyKeyboardMarkup CreateKeyboardMarkup()
        {
            var keyboardButtons = new[]
            {
                    new[]
                    {
                        new KeyboardButton("Розклад мачів"),
                        new KeyboardButton("Результати мачів"),
                    },
                    new[]
                    {
                        new KeyboardButton("Рейтинг команди"),
                        new KeyboardButton("Мій Профіль"),
                    }
                };
            return new ReplyKeyboardMarkup(keyboardButtons);
        }

        async Task SendMessagesAsync(ITelegramBotClient botClient, long chatId, List<string> messages, ReplyKeyboardMarkup? replyMarkup = null)
        {
            foreach (string message in messages)
            {
                await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: message,
                    replyMarkup: replyMarkup
                );
            }
        }
        public class User
        {
            [BsonId]
            public ObjectId Id { get; set; }
            public long ChatId { get; set; }
            public string Username { get; set; }
        }
        public class TeamsUser
        {
            public ObjectId Id { get; set; }
            public long ChatId { get; set; }
            public string Name { get; set; }
        }
        public class PlayerUser
        {
            public ObjectId Id { get; set; }
            public long ChatId { get; set; }
            public string Name { get; set; }
        }
        public class UpdateUsernameRequest
        {
            public long ChatId { get; set; }
            public string NewUsername { get; set; }
        }
        public class UpdateTeamRequest
        {
            public long ChatId { get; set; }
            public string NewName { get; set; }
        }
        public class UpdatePlayerRequest
        {
            public long ChatId { get; set; }
            public string NewName { get; set; }
        }
    }
}