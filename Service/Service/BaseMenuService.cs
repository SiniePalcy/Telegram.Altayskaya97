using System.Collections.Generic;
using System.Text;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Service.Interface;

namespace Telegram.SafeBot.Service
{
    public abstract class BaseMenuService : IMenuService
    {
        protected string MakeMenu(IEnumerable<MenuAction> menuActions)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var menuAction in menuActions)
            {
                sb.Append($"\n<code><b>{menuAction.Command.Template}</b></code> - {menuAction.Command.Description};");
            }
            return sb.ToString();
        }

        protected string MakeHeader(string userName)
        {
            return $"Hi, <b>{userName}</b>!\nCommands:";
        }

        public abstract string GetMenu(string userName, bool isAdmin);
    }
}
