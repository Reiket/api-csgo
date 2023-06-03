using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Telegram.Bot;
using Newtonsoft.Json;
using Telegram.Bot.Exceptions;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExample
{
    public partial class Bot
    {
        private async Task<List<Match>> GetScheduleAsync()
        {
            List<Match> schedule = new List<Match>();

            try
            {
                string url = "https://hltv-api.vercel.app/api/matches.json"; // Замініть URL на відповідний API-URL

                string json = await httpClient.GetStringAsync(url);
                JArray matchesArray = JArray.Parse(json);

                foreach (JToken matchToken in matchesArray)
                {
                    Match match = new Match
                    {
                        Id = matchToken.Value<int>("id"),
                        Time = matchToken.Value<DateTime>("time"),
                        Stars = matchToken.Value<int>("stars"),
                        Maps = matchToken.Value<string>("maps"),
                        Event = matchToken["event"].ToObject<Event>(),
                        Teams = matchToken["teams"].ToObject<List<Team>>()
                    };

                    schedule.Add(match);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося отримати розклад: {ex.Message}");
            }

            return schedule;
        }
        private async Task<string> DeleteDataFromApi(string apiUrl, string successMessage, string errorMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    return successMessage;
                }
                else
                {
                    return $"{errorMessage}: {response.StatusCode}";
                }
            }
        }


        private async Task<string> GetTeamRankingsAsync()
        {
            string url = "https://hltv-api.vercel.app/api/player.json";

            try
            {
                string json = await httpClient.GetStringAsync(url);
                JArray teamRankingsArray = JArray.Parse(json);

                string teamRankingsMessage = "Рейтинг команд:\n\n";
                foreach (JToken teamRankingToken in teamRankingsArray)
                {
                    int teamRank = teamRankingToken.Value<int>("ranking");
                    string teamName = teamRankingToken.Value<string>("name");
                    string teamRankingInfo = $"Рейтинг: {teamRank}\nКоманда: {teamName}\n\n";
                    teamRankingsMessage += teamRankingInfo;
                }

                return teamRankingsMessage;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося отримати рейтинг команд: {ex.Message}");
                return "Не вдалося отримати рейтинг команд.";
            }
        }

        private async Task<List<Match>> GetTeamResultsAsync()
        {
            List<Match> resultTeams = new List<Match>();

            try
            {
                string url = "https://hltv-api.vercel.app/api/results.json"; // Замініть URL на відповідний API-URL

                string json = await httpClient.GetStringAsync(url);
                JArray teamResultsArray = JArray.Parse(json);


                foreach (JToken teamResultToken in teamResultsArray)
                {
                    Match match = new Match
                    {
                        Id = teamResultToken.Value<int>("id"),
                        Time = teamResultToken.Value<DateTime>("time"),
                        Stars = teamResultToken.Value<int>("stars"),
                        Maps = teamResultToken.Value<string>("maps"),
                        Event = teamResultToken["event"].ToObject<Event>(),
                        Teams = teamResultToken["teams"].ToObject<List<Team>>()
                    };

                    resultTeams.Add(match);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не вдалося отримати розклад: {ex.Message}");
            }

            return resultTeams;
        }

        private async Task<string> GetDataFromApi(string apiUrl)
        {
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();

                // Повернути отриманий вміст
                return content;
            }
        }
        private async Task<string> GetUsernameFromApi(string username)
        {
            var apiUrl = $"http://localhost:5156/api/user?username={username}";
            return await GetDataFromApi(apiUrl);
        }

        private async Task<string> GetTeamFromApi(string teams)
        {
            var apiUrl = $"http://localhost:5156/api/team?teams={teams}";
            return await GetDataFromApi(apiUrl);
        }

        private async Task<string> GetPlayerFromApi(string players)
        {
            var apiUrl = $"http://localhost:5156/api/players?players={players}";
            return await GetDataFromApi(apiUrl);
        }
        public async Task UpdateDataAsync<TRequest>(long chatId, TRequest request, string apiEndpoint, string successMessage, string errorMessage)
        {
            using (var client = new HttpClient())
            {
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"http://localhost:5156/api/{apiEndpoint}", content);

                if (response.IsSuccessStatusCode)
                {
                    await SendMessageAsync(botClient, chatId, new List<string> { successMessage });
                }
                else
                {
                    await SendMessageAsync(botClient, chatId, new List<string> { errorMessage });
                }
            }
        }
        public async Task UpdateUsernameAsync(long chatId, string newUsername)
        {
            var request = new UpdateUsernameRequest
            {
                ChatId = chatId,
                NewUsername = newUsername
            };

            await UpdateDataAsync(chatId, request, "user/updateusername", $"Ваш нікнейм змінено на '{newUsername}'.", "Сталася помилка при оновленні нікнейму.");
        }

        public async Task UpdateTeamAsync(long chatId, string team)
        {
            var request = new UpdateTeamRequest
            {
                ChatId = chatId,
                NewName = team
            };

            await UpdateDataAsync(chatId, request, "team/updateteam", $"Вашу команду змінено на '{team}'.", "Сталася помилка при оновленні команди.");
        }

        public async Task UpdatePlayerAsync(long chatId, string player)
        {
            var request = new UpdatePlayerRequest
            {
                ChatId = chatId,
                NewName = player
            };

            await UpdateDataAsync(chatId, request, "players/updateplayers", $"Вашого гравця змінено на '{player}'.", "Сталася помилка при оновленні гравця.");
        }
        private async Task SaveDataAsync<TRequest>(long chatId, TRequest request, string apiEndpoint)
        {
            using (var client = new HttpClient())
            {
                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"http://localhost:5156/api/{apiEndpoint}", content);
                System.Console.WriteLine(response);
            }
        }

        private async Task SaveUsername(long chatId, string username)
        {
            var request = new UpdateUsernameRequest
            {
                ChatId = chatId,
                NewUsername = username
            };

            await SaveDataAsync(chatId, request, "user/setusername");
        }

        private async Task SaveTeam(long chatId, string team)
        {
            var request = new UpdateTeamRequest
            {
                ChatId = chatId,
                NewName = team
            };

            await SaveDataAsync(chatId, request, "team/setteam");
        }

        private async Task SavePlayer(long chatId, string player)
        {
            var request = new UpdatePlayerRequest
            {
                ChatId = chatId,
                NewName = player
            };

            await SaveDataAsync(chatId, request, "players/setplayers");
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
