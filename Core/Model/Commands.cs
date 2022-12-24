using System.Linq;
using System.Reflection;

namespace Telegram.SafeBot.Core.Model
{
    public static class Commands
    {
        public static Command Unknown => new Command(name: "/unsaved", isShown: false);

        //common commands
        public static Command Help => new Command(name: "/help", description: "Help");
        public static Command Helb => new Command(name: "/xelb", isShown: false);
        public static Command Start => new Command(name: "/start", 
            description: "Call this menu");
        public static Command IWalk => new Command(name: "/Iwalk", 
            description: "I walk"); 
        public static Command NoWalk => new Command(name: "/nowalk", 
            description: "I don't walk"); 
        public static Command Return => new Command(name: "/return", 
            isShown: false, isSecret: true);

        //admin commands
        public static Command Post => new Command(name: "/post", 
            description: "Send a post", isAdmin: true);
        public static Command Poll => new Command(name: "/poll", 
            description: "Send a pool", isAdmin: true);
        public static Command ChatList => new Command(name: "/chatlist", 
            description: "Chat list", isAdmin: true);
        public static Command UserList => new Command(name: "/userlist", 
            description: "User list", isAdmin: true);
        public static Command InActive => new Command(name: "/inactive", 
            description: "Unactive users", isAdmin: true);
        public static Command Ban => new Command(name: "/ban", template: "/ban [username|id]", 
            description: "Ban user by username or id", isAdmin: true);
        public static Command BanAll => new Command(name: "/banall", 
            description: "Ban all users", isAdmin: true);
        public static Command DeleteChat => new Command(name: "/deletechat", 
            template: "/deletechat chatname", description: "Удалить чат", isAdmin: true);
        public static Command DeleteUser => new Command(name: "/deleteuser", 
            template: "/deleteuser [username|id]", description: "Удалить пользователя", isAdmin: true);
        public static Command Clear => new Command(name: "/clear", 
            description: "Clear chat", isAdmin: true);
        public static Command ChangePassword => new Command(name: "/changepass", 
            description: "Change password for chat", isAdmin: true);
        public static Command ChangeChatType => new Command(name: "/changechattype",
            description: "Change chat type", isAdmin: true);
        public static Command ChangeUserType => new Command(name: "/changeusertype",
            template: "/changeusertype [Admin|Member|Coordinator] [id|username]",
            description: "Change permissions for user", isAdmin: true);
        public static Command UnpinMessage => new Command(name: "/unpin",
            description: "Unpin message", isAdmin: true);
        public static Command Backup => new Command(name: "/backup",
            isShown: false, isAdmin: true);
        public static Command Restore => new Command(name: "/restore",
            isShown: false, isAdmin: true);
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
