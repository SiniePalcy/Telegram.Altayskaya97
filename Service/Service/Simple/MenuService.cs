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
            //new MenuAction(Command.Place, "координаты мест"),
            //new MenuAction(Command.List, "получить листовки"),
            //new MenuAction(Command.Adverb, "получить объявления")
        };

        private IList<MenuAction> _adminCommands = new List<MenuAction>
        {
            //new MenuAction(Command.AddPlace, "добавить место"),
            //new MenuAction(Command.RemovePlace, "удалить место с id"),
            //new MenuAction(Command.AddList, "добавить листовку"),
            //new MenuAction(Command.RemoveList, "удалить листовку с id"),
            //new MenuAction(Command.AddAdverb, "добавить объявление"),
            //new MenuAction(Command.RemoveAdverb, "удалить объявление с id"),
            new MenuAction(Commands.UserList.Template, Commands.UserList.Description),
            //new MenuAction(Commands.Post.Template, Commands.Post.Description),
            new MenuAction(Commands.ChatList.Template, Commands.ChatList.Description),
            new MenuAction(Commands.Ban.Template, Commands.Ban.Description),
            new MenuAction(Commands.BanAll.Template, Commands.BanAll.Description)
            //new MenuAction(Commands.Unban.Template, Commands.Unban.Description)
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
