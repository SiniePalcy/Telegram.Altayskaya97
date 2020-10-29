using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
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
            return $"Приветствую тебя, <b>{userName}</b>!\nКомманды управления:";
        }

        public abstract string GetMenu(string userName, bool isAdmin);
    }
}
