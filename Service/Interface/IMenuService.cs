using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IMenuService : IService
    {
        string GetMenu(string userName, bool isAdmin);
    }
}
