using System.Linq;
using System.Collections.Generic;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Core.Constant;

namespace Telegram.Altayskaya97.Service
{
    public class MenuService : BaseMenuService
    {
        private IList<MenuAction> _commands = new List<MenuAction>
        {
            new MenuAction(Commands.Start.Template, Commands.Start.Description),
        };

        private IList<MenuAction> _adminCommands = new List<MenuAction>
        {
            new MenuAction(Commands.UserList.Template, Commands.UserList.Description),
            new MenuAction(Commands.ChatList.Template, Commands.ChatList.Description),
            new MenuAction(Commands.Ban.Template, Commands.Ban.Description),
            new MenuAction(Commands.BanAll.Template, Commands.BanAll.Description)
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
