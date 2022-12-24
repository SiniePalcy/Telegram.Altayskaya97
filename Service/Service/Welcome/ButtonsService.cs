using System.Collections.Generic;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI.AvailableTypes;

namespace Telegram.SafeBot.Service
{
    public class ButtonsService : IButtonsService
    {
        private readonly Button[] _buttons = new Button[]
        {
            new LinkButton("Chat rules",  "https://telegra.ph/Pravila-chata-10-28-4", false),
            new LinkButton("Chat rules",  "https://telegra.ph/Pravila-SHpiciyalnyh-botov-10-28", true),
            new LinkButton("List chats",  "http://dze.chat"),
            new LinkButton("Safety rules in Telegram",  "https://telegra.ph/faq-09-08-4"),
            new LinkButton("Add camera to map",  "https://minsk.sous-surveillance.net"),
            new CallbackButton("I walk", CallbackActions.IWalk),
            new CallbackButton("I don't walk", CallbackActions.NoWalk)
        };


        public IEnumerable<IEnumerable<InlineKeyboardButton>> GetWelcomeButtons(string chatType)
        {
            var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();
            
            for(int i = 0; i < _buttons.Length; i++)
            {
                switch(_buttons[i])
                {
                    case LinkButton linkButton when chatType == Core.Model.ChatType.Admin && linkButton.IsAdmin.HasValue && linkButton.IsAdmin.Value:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.SetUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case LinkButton linkButton when chatType == Core.Model.ChatType.Public && linkButton.IsAdmin.HasValue && !linkButton.IsAdmin.Value:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.SetUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case LinkButton linkButton when !linkButton.IsAdmin.HasValue:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.SetUrl(linkButton.Title, linkButton.Link) });
                        break;
                    case CallbackButton callbackButton:
                        inlineKeyboardButtons.Add(new InlineKeyboardButton[1] { InlineKeyboardButton.SetCallbackData(callbackButton.Title, callbackButton.CallbackName) });
                        break;
                };
            }
            return inlineKeyboardButtons;
        }

        public string GetWelcomeMessage(string userName)
        {
            return $"Welcome to our chat, <b>{userName}</b>! Вместе с тобой мы будем строить страну для жизни!🔥" +
                   $"\nFor this menu call <a href='tg://help'>/help</a>" +
                   $"\n\nAbout #ягуляю and return to home ask me about it." +
                   $"\nConfidentialy is guaratned!😉";
        }
    }
}
