namespace Telegram.Altayskaya97.Core.Model
{
    public class LinkButton
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public LinkButton(string title, string link)
        {
            this.Title = title;
            this.Link = link;
        }
    }
}
