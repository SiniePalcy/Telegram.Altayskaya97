namespace Telegram.Altayskaya97.Core.Model
{
    public class MenuAction
    {
        public string Command { get; set; }
        public string Description { get; set; }
        public MenuAction(Command command)
        {
            this.Command = command.Name;
            this.Description = command.Description;
        }
    }
}
