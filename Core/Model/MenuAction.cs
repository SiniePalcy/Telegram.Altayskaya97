namespace Telegram.Altayskaya97.Core.Model
{
    public class MenuAction
    {
        public Command Command { get; set; }
        public string Description { get; set; }
        public MenuAction(Command command)
        {
            this.Command = command;
            this.Description = command.Description;
        }
    }
}
