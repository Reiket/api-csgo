namespace TelegramBotExample
{
    public class Team
{
    public string Name { get; set; }
    public string Logo { get; set; }

    public Team(string name, string logo)
    {
        Name = name;
        Logo = logo;
    }
}
}