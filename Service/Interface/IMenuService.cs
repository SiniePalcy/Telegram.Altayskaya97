namespace Telegram.SafeBot.Service.Interface
{
    public interface IMenuService : IService
    {
        string GetMenu(string userName, bool isAdmin);
    }
}
