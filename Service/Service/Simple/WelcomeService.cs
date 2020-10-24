using System.Collections.Generic;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Service
{
    public class WelcomeService : IWelcomeService
    {
        private LinkButton[] _linkButtons = new LinkButton[]
        {
            new LinkButton("Правила чата",  "https://telegra.ph/Pravila-chata-Altajskaya-09-22"),
            new LinkButton("Список районных чатов",  "http://dze.chat"),
            new LinkButton("Анонимность в Телеграм",  "https://telegra.ph/faq-09-08-4"),
            new LinkButton("Добавить камеру на карту",  "https://minsk.sous-surveillance.net")
        };


        public IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons()
        {
            var inlineKeyboardArray = new InlineKeyboardButton[_linkButtons.Length][];
            for(int i = 0; i < inlineKeyboardArray.Length; i++)
            {
                inlineKeyboardArray[i] = new InlineKeyboardButton[1];
                inlineKeyboardArray[i][0] = InlineKeyboardButton.WithUrl(_linkButtons[i].Title, _linkButtons[i].Link);
            }
            return inlineKeyboardArray;
        }

        public string GetWelcomeMessage(string userName)
        {
            return $"Добро пожаловать в наш чат, <b>{userName}</b>! Вместе с тобой мы будем строить страну для жизни!🔥" +
                   $"\nДля вызова этого меню набери <a href='tg://help'>/help</a>" +
                   $"\n\nПо вопросам #ягуляю и возврата обращайся к {GlobalEnvironment.BotName}. " +
                   $"\nКонфиденциальность гаратируется!😉";
        }
    }
}
