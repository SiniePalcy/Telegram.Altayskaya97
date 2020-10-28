using System.Collections.Generic;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Core.Model;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Altayskaya97.Service
{
    public class WelcomeService : IWelcomeService
    {
        private readonly Button[] _buttons = new Button[]
        {
            new LinkButton("Правила чата",  "https://telegra.ph/Pravila-chata-10-28-4", false),
            new LinkButton("Правила чата",  "https://telegra.ph/Pravila-SHpiciyalnyh-botov-10-28", true),
            new LinkButton("Список районных чатов",  "http://dze.chat"),
            new LinkButton("Анонимность в Телеграм",  "https://telegra.ph/faq-09-08-4"),
            new LinkButton("Добавить камеру на карту",  "https://minsk.sous-surveillance.net"),
            new CallbackButton("Я гуляю", CallbackActions.IWalk)
          //  new CallbackButton("Я не гуляю", CallbackActions.NoWalk)
        };


        public IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons(string chatType)
        {
            var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();
            
            for(int i = 0; i < _buttons.Length; i++)
            {
                switch(_buttons[i])
                {
                    case LinkButton linkButton when chatType == ChatType.Admin && linkButton.IsAdmin.HasValue && linkButton.IsAdmin.Value:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case LinkButton linkButton when chatType == ChatType.Public && linkButton.IsAdmin.HasValue && !linkButton.IsAdmin.Value:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case LinkButton linkButton when !linkButton.IsAdmin.HasValue:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case CallbackButton callbackButton:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.WithCallbackData(callbackButton.Title, callbackButton.CallbackName) });
                        break;
                };
            }
            return inlineKeyboardButtons;
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
