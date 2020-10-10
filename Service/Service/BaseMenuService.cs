using System.Collections.Generic;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public abstract class BaseMenuService : IMenuService
    {
        protected string MakeMenu(IEnumerable<MenuAction> commands)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var command in commands)
            {
                sb.Append($"\n<code><b>{command.Command}</b></code> - {command.Description};");
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
