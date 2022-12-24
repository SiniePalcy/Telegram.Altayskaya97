namespace Telegram.SafeBot.Core.Model
{
    public class LinkButton : Button
    {
        public string Link { get; set; }
        public bool? IsAdmin { get; set; }
        public LinkButton(string title, string link, bool? isAdmin = null)
        {
            Title = title;
            Link = link;
            IsAdmin = isAdmin;
        }
    }
}
