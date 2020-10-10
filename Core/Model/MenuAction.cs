namespace Telegram.Altayskaya97.Core.Model
{
    public class MenuAction
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public MenuAction(string command, string description)
        {
            this.Command = command;
            this.Description = description;
        }
    }
}
