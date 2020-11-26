using System.Linq;
using System.Reflection;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Core.Model
{
    public static class Commands
    {
        //common commands
        public static Command Help { get; } = new Command("/help", "/help", "Справка");
        public static Command Helb { get; } = new Command("/xelb", "/xelb", "", false, false);
        public static Command Start { get; } = new Command("/start", "/start", "Вызов этого меню");
        public static Command IWalk { get; } = new Command("/Iwalk", "/Iwalk", "Я гуляю"); 
        public static Command NoWalk { get; } = new Command("/nowalk", "/nowalk", "Я не гуляю"); 
        public static Command Return { get; } = new Command("/triton", "/triton", "", false, false); // secret word

        //admin commands
        public static Command Post { get; } = new Command("/post", "/post", "Отправить объявление", true);
        public static Command Poll { get; } = new Command("/poll", "/poll", "Отправить опрос", true);
        public static Command ChatList { get; } = new Command("/chatlist", "/chatlist", "Список чатов бота", true);
        public static Command UserList { get; } = new Command("/userlist", "/userlist", "Список пользователей", true);
        public static Command InActive { get; } = new Command("/inactive", "/inactive", "Неактивные пользователи", true);
        public static Command Ban { get; } = new Command("/ban", "/ban [username|id]", "Забанить пользователя с username или id", true);
        public static Command BanAll { get; } = new Command("/banall", "/banall", "Забанить всех пользователей", true);
        public static Command DeleteChat { get; } = new Command("/deletechat", "/deletechat chatname", "Удалить чат", true);
        public static Command DeleteUser { get; } = new Command("/deleteuser", "/deleteuser [username|id]", "Удалить пользователя", true);
        public static Command Clear { get; } = new Command("/clear", "/clear", "Очистка чата", true);
        public static Command GrantAdmin { get; } = new Command("/shpic", "/shpic", "", true, false); // secret word

        private static string ExtractCommandName(string command)
        {
            command = command.Trim().ToLower();
            var spaceInd =command.IndexOf(' ');
            return spaceInd < 1 ? command : command.Substring(0, spaceInd);
        }

        public static Command GetCommand(string commandText)
        {
            string commandName = ExtractCommandName(commandText);

            var props = typeof(Commands).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var commands = props.Where(p => p.PropertyType == typeof(Command)).Select(p => (Command) p.GetValue(null));

            var command =  commands.FirstOrDefault(c => c.Name.ToLower() == commandName);
            if (command == null)
                return null;

            command.Text = commandText;
            return command;
        }
    }
}
