using System;
using System.Collections.Generic;

namespace TelegramBotExample
{
    public class Match
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public int Stars { get; set; }
        public string Maps { get; set; }
        public Event Event { get; set; }
        public List<Team> Teams { get; set; }
    }
}