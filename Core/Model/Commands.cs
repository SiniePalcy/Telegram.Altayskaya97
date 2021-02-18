using System.Linq;
using System.Reflection;
using Telegram.Altayskaya97.Core.Model;

namespace Telegram.Altayskaya97.Core.Model
{
    public static class Commands
    {
        public static Command Unknown => new Command(name: "/unsaved", isShown: false);

        //common commands
        public static Command Help => new Command(name: "/help", description: "Справка");
        public static Command Helb => new Command(name: "/xelb", isShown: false);
        public static Command Start => new Command(name: "/start", 
            description: "Вызов этого меню");
        public static Command IWalk => new Command(name: "/Iwalk", 
            description: "Я гуляю"); 
        public static Command NoWalk => new Command(name: "/nowalk", 
            description: "Я не гуляю"); 
        public static Command Return => new Command(name: "/return", 
            isShown: false, isSecret: true);

        //admin commands
        public static Command Post => new Command(name: "/post", 
            description: "Отправить объявление", isAdmin: true);
        public static Command Poll => new Command(name: "/poll", 
            description: "Отправить опрос", isAdmin: true);
        public static Command ChatList => new Command(name: "/chatlist", 
            description: "Список чатов бота", isAdmin: true);
        public static Command UserList => new Command(name: "/userlist", 
            description: "Список пользователей", isAdmin: true);
        public static Command InActive => new Command(name: "/inactive", 
            description: "Неактивные пользователи", isAdmin: true);
        public static Command Ban => new Command(name: "/ban", template: "/ban [username|id]", 
            description: "Забанить пользователя с username или id", isAdmin: true);
        public static Command BanAll => new Command(name: "/banall", 
            description: "Забанить всех пользователей", isAdmin: true);
        public static Command DeleteChat => new Command(name: "/deletechat", 
            template: "/deletechat chatname", description: "Удалить чат", isAdmin: true);
        public static Command DeleteUser => new Command(name: "/deleteuser", 
            template: "/deleteuser [username|id]", description: "Удалить пользователя", isAdmin: true);
        public static Command Clear => new Command(name: "/clear", 
            description: "Очистка чата", isAdmin: true);
        public static Command ChangePassword => new Command(name: "/changepass", 
            description: "Сменить пароль чата", isAdmin: true);
        public static Command ChangeChatType => new Command(name: "/changechattype",
            description: "Сменить тип чата", isAdmin: true);
        public static Command ChangeUserType => new Command(name: "/changeusertype",
            template: "/changeusertype [Admin|Member|Coordinator] [id|username]",
            description: "Сменить тип пользователя", isAdmin: true);

        public static Command UnpinMessage => new Command(name: "/unpin",
            description: "Открепить сообщения", isAdmin: true);

        public static Command GrantAdmin => new Command(name: "/grantadmin", 
            isAdmin: true, isShown: false, isSecret: true);

        private static string ExtractCommandName(string command)
        {
            command = command.Trim().ToLower();
            var spaceInd =command.IndexOf(' ');
            return spaceInd < 1 ? command : command.Substring(0, spaceInd);
        }

        public static Command GetCommand(string commandText)
        {
            string commandName = ExtractCommandName(commandText);
            if (!IsCommand(commandName))
                return null;

            var props = typeof(Commands).GetProperties(BindingFlags.Public | BindingFlags.Static);
            var commands = props.Where(p => p.PropertyType == typeof(Command)).Select(p => (Command) p.GetValue(null));

            var command =  commands.FirstOrDefault(c => !c.IsSecret && c.Name.ToLower() == commandName);
            if (command == null)
                command = Commands.Unknown;

            command.Text = commandText;
            return command;
        }

        private static bool IsCommand(string commandName)
        {
            return commandName.StartsWith("/");
        }
    }
}
