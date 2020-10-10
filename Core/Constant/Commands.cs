using System.Linq;
using System.Reflection;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Core.Constant
{
    public static class Commands
    {
        //common commands
        public static Command Help { get; } = new Command("/help", "/help", "Справка");
        public static Command Helb { get; } = new Command("/helb", "/helb", "", false, false);
        public static Command Start { get; } = new Command("/start", "/start", "Вызов этого меню");

        //admin commands
        public static Command Post { get; } = new Command("/post", "/post [chatId or all] text", "Отправить объявление в чат (chatId) или все чаты (all) с текстом text", true);
        public static Command ChatList { get; } = new Command("/chatlist", "/chatlist", "Показать список чатов бота", true);
        public static Command UserList { get; } = new Command("/userlist", "/userlist", "Список пользователей", true);
        public static Command Ban { get; } = new Command("/ban", "/ban username", "Забанить пользователя с username", true);
        public static Command BanAll { get; } = new Command("/banall", "/banall", "Забанить всех пользователей", true);
        public static Command Sobachku { get; } = new Command("/sobachku", "/sobachku","", true, false); // secret word

        public static string ExtractCommandName(string command)
        {
            command = command.Trim();
            var spaceInd =command.IndexOf(' ');
            return spaceInd < 1 ? command : command.Substring(0, spaceInd);
        }

        public static Command GetCommand(string commandText)
        {
            string commandName = ExtractCommandName(commandText);

            var props = typeof(Commands).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var commands = props.Where(p => p.PropertyType == typeof(Command)).Select(p => (Command) p.GetValue(null));

            var command =  commands.FirstOrDefault(c => c.Name == commandName);
            if (command == null)
                return null;

            command.Text = commandText;
            return command;
        }
    }
}
