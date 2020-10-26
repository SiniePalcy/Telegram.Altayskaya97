namespace Telegram.Altayskaya97.Service.Interface
{
    public interface IMenuService : IService
    {
        string GetMenu(string userName, bool isAdmin);
    }
}
