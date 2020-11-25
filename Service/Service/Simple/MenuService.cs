using System.Linq;
using System.Collections.Generic;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Core.Constant;

namespace Telegram.Altayskaya97.Service
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
            new MenuAction(Commands.Post),
            new MenuAction(Commands.Poll),
            new MenuAction(Commands.UserList),
            new MenuAction(Commands.ChatList),
            new MenuAction(Commands.InActive),
            new MenuAction(Commands.Ban),
            new MenuAction(Commands.BanAll),
            new MenuAction(Commands.DeleteChat),
            new MenuAction(Commands.Clear)
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
