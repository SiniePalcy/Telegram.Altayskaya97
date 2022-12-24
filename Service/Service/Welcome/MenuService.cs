using System.Linq;
using System.Collections.Generic;
using Telegram.SafeBot.Core.Model;

namespace Telegram.SafeBot.Service
{
    public class MenuService : BaseMenuService
    {
        private readonly IList<MenuAction> _commands = new List<MenuAction>
        {
            new MenuAction(Commands.Start),
            new MenuAction(Commands.IWalk),
            new MenuAction(Commands.NoWalk)
        };

        private readonly IList<MenuAction> _adminCommands = new List<MenuAction>
        {
            new MenuAction(Commands.Ban),
            new MenuAction(Commands.BanAll),
            new MenuAction(Commands.ChangeChatType),
            new MenuAction(Commands.ChangePassword),
            new MenuAction(Commands.ChangeUserType),
            new MenuAction(Commands.ChatList),
            new MenuAction(Commands.Clear),
            new MenuAction(Commands.DeleteChat),
            new MenuAction(Commands.DeleteUser),
            new MenuAction(Commands.InActive),
            new MenuAction(Commands.Poll),
            new MenuAction(Commands.Post),
            new MenuAction(Commands.UnpinMessage),
            new MenuAction(Commands.UserList)
        };
        public MenuService()
        {
            _adminCommands = _commands.Union(_adminCommands).ToList();
        }

        public override string GetMenu(string userName, bool isAdmin)
        {
            var commandsSet = isAdmin ? _adminCommands : _commands;
            return MakeHeader(userName) + MakeMenu(commandsSet);
        }
    }
}
