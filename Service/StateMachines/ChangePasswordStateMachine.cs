using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Enum;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Service.Interface;
using Telegram.BotAPI.AvailableTypes;
using Telegram.SafeBot.Service.StateMachines.UserStates;

namespace Telegram.SafeBot.Service.StateMachines
{
    public class ChangePasswordStateMachine :
        BaseStateMachine<ChangePasswordUserState, ChangePasswordState>
    {
        private IPasswordService PasswordService { get; }

        public ChangePasswordStateMachine(IPasswordService passwordService)
        {
            this.PasswordService = passwordService;
        }

        public override async Task<CommandResult> ExecuteStage(long userId, Message message = null)
        {
            if (!(GetUserStateFlow(userId) is ChangePasswordUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            userState.ExecuteNextStage();

            return userState.CurrentState switch
            {
                ChangePasswordState.Start => await StartState(),
                ChangePasswordState.PasswordTypeChoice => await PasswordTypeChoice(userId, message.Text),
                ChangePasswordState.NewPasswordInput => NewPasswordInput(userId, message.Text),
                ChangePasswordState.Confirmation => ConfirmationState(userId, message.Text),
                _ => default
            };
        }

        public async Task<CommandResult> StartState()
        {
            var passwords = await PasswordService.GetList();
            var buttonsList = passwords.Select(c => new KeyboardButton(c.ChatType)).ToList();
            buttonsList.Add(new KeyboardButton(Messages.Cancel));

            var buttonsReplyList = buttonsList.Select(b => new KeyboardButton[1] { b });
            return new CommandResult(Messages.SelectChatType, CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(buttonsReplyList));
        }

        public async Task<CommandResult> PasswordTypeChoice(long userId, string chatType)
        {
            var password = await PasswordService.GetByType(chatType);
            if (password == null)
            {
                StopUserStateFlow(userId);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }

            var userState = GetUserStateFlow(userId);
            userState.ChatType = password.ChatType;

            return new CommandResult(Messages.InputNewPassword, CommandResultType.TextMessage);
        }

        public CommandResult NewPasswordInput(long userId, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                StopUserStateFlow(userId);
                return new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);
            }

            //if (!newPassword.StartsWith("/"))
            //{
            //    StopUserStateFlow(userId);
            //    return new CommandResult(Messages.Cancelled + ": password must starts from '/'",
            //        CommandResultType.TextMessage);
            //}

            var userState = GetUserStateFlow(userId);
            userState.NewPassword = newPassword;


            KeyboardButton[] confirmButtons = new KeyboardButton[]
            {
                            new KeyboardButton(Messages.OK),
                            new KeyboardButton(Messages.Cancel)
            };
            return new CommandResult(Messages.Confirmation, CommandResultType.TextMessage,
                new ReplyKeyboardMarkup(confirmButtons));
        }

        private CommandResult ConfirmationState(long userId, string messageText)
        {
            if (!(GetUserStateFlow(userId) is ChangePasswordUserState userState))
                return new CommandResult(Messages.UnknownError, CommandResultType.TextMessage);

            CommandResult commandResult = messageText == Messages.OK ?
                new CommandResult("", CommandResultType.ChangePassword)
                {
                    Properties = new Dictionary<string, object>
                    {
                        { "ChatType", userState.ChatType },
                        { "NewPassword", userState.NewPassword }
                    }
                } :
                new CommandResult(Messages.Cancelled, CommandResultType.TextMessage);

            StopUserStateFlow(userId);

            return commandResult;
        }
    }
}
