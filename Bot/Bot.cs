using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Telegram.SafeBot.Core.Constant;
using Telegram.SafeBot.Core.Model;
using Telegram.SafeBot.Model;
using Telegram.SafeBot.Service;
using Telegram.SafeBot.Service.Extensions;
using Telegram.SafeBot.Service.Interface;
using Telegram.SafeBot.Service.StateMachines;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableMethods.FormattingOptions;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.GettingUpdates;
using Telegram.BotAPI.InlineMode;
using Chat = Telegram.BotAPI.AvailableTypes.Chat;
using ChatType = Telegram.BotAPI.AvailableTypes.ChatType;
using User = Telegram.BotAPI.AvailableTypes.User;

namespace Telegram.SafeBot.Bot
{
    public class Bot : TelegramBotBase<BotProperties>
    {
        private static bool _isFirstRun = true;

        private readonly ILogger<Bot> _logger;
        private readonly Configuration _configuration;
        private readonly IStateMachineContainer _stateMachineContainer;
       
        public IButtonsService WelcomeService { get; } 
        public IMenuService MenuService { get;}
        public IPasswordService PasswordService { get; }
        public IUserService UserService { get; }
        public IChatService ChatService { get; }
        public IUserMessageService UserMessageService { get; }
        public IDateTimeService DateTimeService { get; }

        public Bot(
            ILogger<Bot> logger, 
            BotProperties botProperties,
            IButtonsService welcomeService,
            IMenuService menuService,
            IUserService userService,
            IChatService chatService,
            IUserMessageService userMessageService,
            IPasswordService passwordService,
            IDateTimeService dateTimeService,
            IConfiguration configuration,
            IStateMachineContainer stateMachineContainer) 
            : base(botProperties)
        {
            _logger = logger;

            this.WelcomeService = welcomeService;
            this.MenuService = menuService;
            this.UserService = userService;
            this.ChatService = chatService;
            this.UserMessageService = userMessageService;
            this.PasswordService = passwordService;
            this.DateTimeService = dateTimeService;
            _stateMachineContainer = stateMachineContainer;

            _configuration = configuration.Get<Configuration>();           
        }

        public override void OnUpdate(Update update)
        {
#if DEBUG
            _logger.LogInformation("New update with id: {0}. Type: {1}", update?.UpdateId, update?.Type.ToString("F"));
#endif

            base.OnUpdate(update);
        }

        protected override void OnMessage(Message message)
        {
            // Ignore user 777000 (Telegram)
            if (message!.From?.Id == TelegramConstants.TelegramId)
            {
                return;
            }

            var hasText = !string.IsNullOrEmpty(message.Text); 

            _logger.LogInformation("New message from chat id: {ChatId}, Message: {MessageContent}", 
                message!.Chat.Id,
                hasText ? message.Text : "No text");

            Task.Run(() => RecieveMessage(message));

            base.OnMessage(message);
        }

        protected override void OnCommand(Message message, string commandName, string commandParameters)
        {
        }

        protected override void OnInlineQuery(InlineQuery inlineQuery)
        {
            Api.SendMessage(
                chatId: inlineQuery.From.Id,
                text: inlineQuery.Query);
            base.OnInlineQuery(inlineQuery);
        }

        protected override void OnCallbackQuery(CallbackQuery callbackQuery)
        {
            Task.Run(() => RecieveCallbackData(callbackQuery.Message.Chat, callbackQuery.From, callbackQuery.Data));
            base.OnCallbackQuery(callbackQuery);
        }

        protected override void OnBotException(BotRequestException exp)
        {
            _logger.LogError("BotRequestException: {Message}", exp.Message);
        }

        protected override void OnException(Exception exp)
        {
            _logger.LogError("Exception: {Message}", exp.Message);
        }

        public async Task RecieveMessage(Message message)
        {
            _logger.LogInformation($"Recieved message from {message.From.GetUserName()} in chat '{message.Chat.Title}', chatType={message.Chat.Type}");
            if (message.Chat.Type == Core.Model.ChatType.Private.ToLower())
                await ProcessBotMessage(message);
            else
                await ProcessChatMessage(message);
        }

        public async Task RecieveCallbackData(Chat chat, User from, string data)
        {
            var userRepo = await UserService.Get(from.Id);
            if (userRepo == null)
            {
                await SendTextMessage(chat.Id, "Unknown user");
                return;
            }

            if (data == CallbackActions.IWalk)
            {
                var result = await Ban($"{userRepo.Id}");
                if (chat.Type == Core.Model.ChatType.Private.ToLower())
                    await SendTextMessage(chat.Id, result.Content.ToString());
            }

            if (data == CallbackActions.NoWalk)
            {
                var result = await NoWalk(from);
                await SendTextMessage(chat.Id, result.Content.ToString());
            }

        }

        public async Task ProcessBotMessage(Message chatMessage)
        {
            if (_isFirstRun && await EnsureDbHasOwner(chatMessage))
                return;

            string commandText = (chatMessage?.Text ?? chatMessage.Caption).Trim()?.ToLower();
            if (string.IsNullOrEmpty(commandText))
                return;

            var chat = await EnsureChatSaved(chatMessage.Chat, chatMessage.From);

            var user = chatMessage.From;
            _logger.LogInformation($"Recieved message from '{user.GetUserName()}', id={user.Id}");

            await AddMessage(chatMessage, chat);
            
            var command = Commands.GetCommand(commandText);
            if (command != null && command.IsValid)
            {
                await ProcessCommandMessage(chatMessage.Chat.Id, command, user);
            }
            else
            {
                var result = await _stateMachineContainer.TryProcessStage(user.Id, chatMessage);
                await ReplyCommand(chatMessage.Chat.Id, result);
            }
        }
        
        private async Task<bool> EnsureDbHasOwner(Message chatMessage)
        {
            _isFirstRun = false;

            var userList = await UserService.GetList();
            if (!userList.Any())
            {
                var sender = chatMessage.From;
                if (sender != null)
                {
                    await UserService.Add(new Core.Model.User
                    {
                        Id = sender.Id,
                        Name = sender.Username,
                        Type = UserType.Admin,
                        IsAdmin = true,
                    });

                    return true;
                }
            }

            return false;
        }

        private async Task ProcessCommandMessage(long chatId, Command command, User user)
        {
            CommandResult commandResult;
            var userRepo = await UserService.Get(user.Id);
            if (userRepo == null)
            {
                commandResult = ExecuteCommandUnknownUser(command);
            }
            else if (userRepo.Type == UserType.Member)
            {
                commandResult = await ExecuteCommandMember(command, user);
            }
            else if (userRepo.Type == UserType.Admin || userRepo.Type == UserType.Coordinator)
            {
                commandResult = await ExecuteCommandAdmin(command, user);
            }
            else
            {
                return;
            }

            await ReplyCommand(chatId, commandResult);
        }

        private CommandResult ExecuteCommandUnknownUser(Command command)
        {
            return command == Commands.Start ?
                   new CommandResult("Who are you? Let's goodbye!", CommandResultType.TextMessage) :
                   new CommandResult(Messages.IncorrectCommand);
        }

        private async Task<CommandResult> ExecuteCommandMember(Command command, User from)
        {
            if (command == Commands.Unknown && await PasswordService.IsMemberPass(command.Text))
                command = Commands.Return;

            return command == Commands.Help ? await Start(from) :
                   command == Commands.Start ? await Start(from) :
                   command == Commands.IWalk ? await Ban($"{from.Id}") :
                   command == Commands.Return ? await Unban(from) :
                   command == Commands.NoWalk ? await NoWalk(from) :
                   new CommandResult(Messages.IncorrectCommand);
        }

        private async Task<CommandResult> ExecuteCommandAdmin(Command command, User user)
        {
            var isAdmin = await UserService.IsAdmin(user.Id);

            if (command == Commands.Unknown && await PasswordService.IsAdminPass(command.Text))
                command = Commands.GrantAdmin;

            return command.IsAdmin && isAdmin ?
                   await ExecuteCommandAdminGrant(command, user) :
                   await ExecuteCommandAdminNonGrant(command, user);
        }

        private async Task<CommandResult> ExecuteCommandAdminGrant(Command command, User from)
        {
            return command == Commands.Help ? await Start(from) :
                   command == Commands.Start ? await Start(from) :
                   command == Commands.Post ? await Post(from) :
                   command == Commands.Poll ? await Poll(from) :
                   command == Commands.Clear ? await ClearInteractive(from) :
                   command == Commands.GrantAdmin ? await GrantAdminPermissions(from) :
                   command == Commands.ChatList ? await ChatList() :
                   command == Commands.UserList ? await UserList() :
                   command == Commands.IWalk ? await Ban($"{from.Id}") :
                   command == Commands.Ban ? await Ban(command.Content) :
                   command == Commands.BanAll ? await BanAll() :
                   command == Commands.NoWalk ? await NoWalk(from) :
                   command == Commands.DeleteChat ? await DeleteChat(command.Content) :
                   command == Commands.DeleteUser ? await DeleteUser(command.Content) :
                   command == Commands.InActive ? await InActiveUsers() :
                   command == Commands.ChangePassword ? await ChangePasswordInteractive(from) :
                   command == Commands.ChangeUserType ? await ChangeUserType(from, command.Text) :
                   command == Commands.ChangeChatType ? await ChangeChatTypeInteractive(from) :
                   command == Commands.UnpinMessage ? await UnpinMessageInteractive(from) :
                   command == Commands.Backup ? await Backup() :
                   command == Commands.Restore ? await Restore() :
                   new CommandResult(Messages.IncorrectCommand, CommandResultType.TextMessage);
        }

        private async Task<CommandResult> ExecuteCommandAdminNonGrant(Command command, User from)
        {
            return command == Commands.Help ? await Start(from) :
                   command == Commands.Start ? await Start(from) :
                   command == Commands.Post ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.Poll ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.Clear ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.ChatList ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.UserList ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.IWalk ? await Ban($"{from.Id}") :
                   command == Commands.Ban ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.BanAll ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.NoWalk ? await NoWalk(from) :
                   command == Commands.DeleteChat ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.DeleteUser ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.ChangePassword ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.ChangeUserType ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.ChangeChatType ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.UnpinMessage ? new CommandResult(Messages.NoPermissions, CommandResultType.TextMessage) :
                   command == Commands.GrantAdmin ? await GrantAdminPermissions(from) :
                   new CommandResult(Messages.IncorrectCommand);
        }

        public async Task ProcessChatMessage(Message chatMessage)
        {
            Chat chat = chatMessage.Chat;

            await EnsureChatSaved(chat);

            if (chatMessage.GroupChatCreated == true && chatMessage.SupergroupChatCreated == true)
            {
                await AddGroup(chat);
                return;
            }

            if (chatMessage.MigrateToChatId.HasValue)
            {
                if (chatMessage.ForwardFromMessageId.HasValue)
                {
                    await UpdateChatId(chat.Id, chatMessage.ForwardFromMessageId.Value);
                }
                return;
            }

            if (chatMessage.MigrateFromChatId.HasValue)
            {
                return;
            }

            if (chatMessage.PinnedMessage != null)
            {
                return;
            }

            var chatRepo = await ChatService.Get(chat.Id);
            var sender = chatMessage.From;

            if (chatMessage.LeftChatMember != null)
            {
                var botMember = await Api.GetMeAsync();
                if (chatMessage.LeftChatMember.Id == botMember.Id)
                    await DeleteChat(chatMessage.Chat.Title);
                return;
            }

            if (chatMessage.NewChatMembers != null)
            {
                var users = await UserService.GetList();
                var newMembers = chatMessage.NewChatMembers.Where(c => !users.Any(u => u.Id == c.Id)).ToList();
                foreach (var chatMember in newMembers)
                {
                    Message msg = await SendWelcomeGroupMessage(chatMessage.Chat.Id, chatMember.GetUserName(), chatRepo.ChatType);
                    await EnsureUserSaved(chatMember, chatRepo.ChatType);
                }
                return;
            }

            await EnsureUserSaved(sender, chatRepo.ChatType);

            string userName = sender.GetUserName();
            _logger.LogInformation($"Recieved message from {userName} in chat id={chat?.Id}, title={chat?.Title}, type={chat?.Type}");

            if (string.IsNullOrEmpty(chatMessage.Text))
                return;

            string message = chatMessage.Text.Trim().ToLower();

            var command = Commands.GetCommand(message);
            if (command == null || !command.IsValid)
                return;

            if (message == Commands.Help.Name)
                await SendWelcomeGroupMessage(chatMessage.Chat.Id, userName, chatRepo.ChatType, chatMessage.MessageId);
            else if (message == Commands.Helb.Name)
                await Api.SendPhotoAsync(chatMessage.Chat.Id, "https://i.ytimg.com/vi/gpEtNGeM3zE/maxresdefault.jpg");
            else if (message == Commands.IWalk.Name)
                await Ban($"{sender.Id}");
            else if (message == Commands.NoWalk.Name)
            {
                var commandResult = await NoWalk(sender);
                if (commandResult.Type == CommandResultType.TextMessage)
                    await SendTextMessage(chat.Id, commandResult.Content.ToString());
            }

            return;
        }

        public async Task ProcessStage(IStateMachine stateMachine, long chatId, long userId, Message message)
        {
            var commandResult = await stateMachine.ExecuteStage(userId, message);
            await ReplyCommand(chatId, commandResult);
        }

        public async Task ReplyCommand(long chatId, CommandResult commandResult)
        {
            var recievers = commandResult.Recievers ?? new List<long>() { chatId };

            foreach (var reciever in recievers)
            {
                Message result = commandResult.Type switch
                {
                    CommandResultType.TextMessage => await SendTextMessage(
                            reciever,
                            commandResult.Content.ToString(),
                            commandResult.ReplyMarkup),
                    CommandResultType.Links => await SendLinksList(reciever,
                            commandResult.Properties["Links"] as IEnumerable<Link>),
                    CommandResultType.Pool => await SendPollMessage(reciever,
                            commandResult.Content.ToString(),
                            commandResult.Properties["Cases"] as List<string>,
                            commandResult.Properties["IsMultiAnswers"] as bool?,
                            commandResult.Properties["IsAnonymous"] as bool?,
                            commandResult.ReplyMarkup),
                    CommandResultType.Message => await SendMessageObject(reciever,
                            commandResult),
                    CommandResultType.Delete => await DeleteMessages(reciever,
                            commandResult.Content.ToString()),
                    CommandResultType.ChangePassword => await UpdatePassword(reciever,
                            commandResult.Properties["ChatType"].ToString(),
                            commandResult.Properties["NewPassword"].ToString()),
                    CommandResultType.ChangeChatType => await UpdateChatType(reciever,
                            (long)commandResult.Properties["ChatId"],
                            commandResult.Properties["ChatType"].ToString()),
                    CommandResultType.Unpin => await UnpinMessage(reciever,
                            (long)commandResult.Properties["ChatId"],
                            (long)commandResult.Properties["MessageId"]),
                    _ => default
                };

                if (result == null)
                    continue;

                if (commandResult.Properties.ContainsKey("IsPin"))
                {
                    var isPin = commandResult.Properties["IsPin"] as bool?;
                    if (isPin.HasValue && isPin.Value)
                        await Api.PinChatMessageAsync(reciever, result.MessageId);
                }

                await AddMessage(result);
            }
        }

        #region Command methods

        public async Task<CommandResult> Start(User user)
        {
            bool isAdmin = await UserService.IsAdmin(user.Id);
            return new CommandResult(MenuService.GetMenu(user.Username, isAdmin), CommandResultType.TextMessage,
                new InlineKeyboardMarkup(WelcomeService.GetWelcomeButtons(Core.Model.ChatType.Private)));
        }

        public async Task<CommandResult> GrantAdminPermissions(User user)
        {
            bool isAdmin = await UserService.IsAdmin(user.Id);
            if (!isAdmin)
                await UserService.PromoteUserAdmin(user.Id);

            return await Unban(user, false);
        }

        public async Task<CommandResult> Unban(User user, bool onlyPublicChates = true)
        {
            var result = new CommandResult("", CommandResultType.Links);

            var chatList = await ChatService.GetList();
            List<Core.Model.Chat> chatsToDelete = new List<Core.Model.Chat>();
            var links = new List<Link>();
            foreach (var chat in chatList)
            {
                try
                {
                    if (chat.ChatType == Core.Model.ChatType.Private)
                        continue;

                    if (onlyPublicChates && chat.ChatType == Core.Model.ChatType.Admin)
                        continue;

                    var chatMember = await Api.GetChatMemberAsync(chat.Id, user.Id);
                    if (chatMember == null || chatMember.Status == ChatMemberStatus.Kicked || chatMember.Status == ChatMemberStatus.Left)
                    {
                        if (chatMember.Status == ChatMemberStatus.Kicked)
                            await Api.UnbanChatMemberAsync(chat.Id, user.Id);
                        var inviteLink = await Api.ExportChatInviteLinkAsync(chat.Id);
                        links.Add(new Link
                        {
                            Url = inviteLink,
                            Description = $"Chat <b>{chat.Title}</b>"
                        });
                        _logger.LogInformation($"Link to '{chat.Title}' formed for {user.GetUserName()}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Chat '{chat.Title}' is unavailable and will be deleted");
                    chatsToDelete.Add(chat);
                }
            }

            result.Properties["Links"] = links;

            foreach (var chat in chatsToDelete)
                await ChatService.Delete(chat.Id);

            return result;
        }

        public async Task<CommandResult> ChatList()
        {
            var chatList = await ChatService.GetList();

            string answer = "no any chats";

            var chats = chatList.Where(c => c.ChatType != Core.Model.ChatType.Private).ToList();
            if (chats.Any())
            {
                StringBuilder sb = new StringBuilder("<code>");
                foreach (var chat in chats)
                {
                    if (chat.ChatType != Core.Model.ChatType.Private)
                        sb.AppendLine($"type: {chat.ChatType,-8}name: <b>{chat.Title}</b>");
                }
                sb.Append("</code>");
                answer = sb.ToString();
            }

            return new CommandResult(answer, CommandResultType.TextMessage);
        }

        public async Task<CommandResult> UserList()
        {
            var userList = await UserService.GetList();

            StringBuilder sb = new StringBuilder(string.Format($"<pre>{"Username",-20}{"Type",-12}{"Access",-6}\n"));
            foreach (var user in userList.OrderBy(u => u.Type))
            {
                var isAdmin = await UserService.IsAdmin(user.Id);
                var userType = user.Type;
                var accessSign = isAdmin ? "  +" : "  -";
                sb.AppendLine($"{user.Name,-20}{userType,-12}{accessSign,-6}");
            }
            sb.Append("</pre>");

            return new CommandResult(sb.ToString(), CommandResultType.TextMessage);
        }

        public async Task<CommandResult> InActiveUsers()
        {
            string message = "No inactive users";

            var dtNow = DateTimeService.GetDateTimeUTCNow();
            var userList = await UserService.GetList();
            var inActiveUsers = userList.Where(u => (u.Type == UserType.Member || u.Type == UserType.Admin) &&
                (u.LastMessageTime == null || (dtNow - u.LastMessageTime.Value).TotalDays >= _configuration.PeriodInactiveUserDays));

            if (inActiveUsers.Any())
            {
                StringBuilder sb = new StringBuilder(string.Format($"<code>{"Username",-20}{"Type",-12}{"Last msg",-16}\n"));
                foreach (var user in inActiveUsers.OrderBy(u => u.Type).OrderBy(u => u.LastMessageTime))
                {
                    var userType = user.Type;
                    var lastDateTime = DateTimeService.FormatToString(user.LastMessageTime);
                    sb.AppendLine($"{user.Name,-20}{userType,-12}{lastDateTime,-16}");
                }
                sb.Append("</code>");
                message = sb.ToString();
            }

            return new CommandResult(message, CommandResultType.TextMessage);
        }

        private async Task<CommandResult> Post(User user)
        {
            return await _stateMachineContainer.StartStateMachine<PostStateMachine>(user.Id);
        }

        private async Task<CommandResult> Poll(User user)
        {
            return await _stateMachineContainer.StartStateMachine<PollStateMachine>(user.Id);
        }

        private async Task<CommandResult> ClearInteractive(User user)
        {
            return await _stateMachineContainer.StartStateMachine<ClearStateMachine>(user.Id);
        }

        private async Task<CommandResult> ChangePasswordInteractive(User user)
        {
            return await _stateMachineContainer.StartStateMachine<ChangePasswordStateMachine>(user.Id);
        }

        public async Task<CommandResult> ChangeChatTypeInteractive(User user)
        {
            return await _stateMachineContainer.StartStateMachine<ChangeChatTypeStateMachine>(user.Id);
        }

        public async Task<CommandResult> UnpinMessageInteractive(User user)
        {
            return await _stateMachineContainer.StartStateMachine<UnpinMessageStateMachine>(user.Id);
        }

        public async Task<CommandResult> Ban(string userIdOrName)
        {
            var user = await UserService.GetByIdOrName(userIdOrName);

            if (user == null)
                return new CommandResult(Messages.UserNotFound, CommandResultType.TextMessage);

            if (user.Type == UserType.Coordinator)
                return new CommandResult(Messages.YouCantBanCoordinator, CommandResultType.TextMessage);

            if (user.Type == UserType.Bot)
                return new CommandResult(Messages.YouCantBanBot, CommandResultType.TextMessage);

            var chats = await ChatService.GetList();
            if (!chats.Any())
                return new CommandResult(Messages.NoAnyChats, CommandResultType.TextMessage);

            StringBuilder buffer = new StringBuilder();
            foreach (var chatRepo in chats)
            {
                if (chatRepo.ChatType == Core.Model.ChatType.Admin && user.Type == UserType.Member ||
                    chatRepo.ChatType == Core.Model.ChatType.Private)
                    continue;

                try
                {
                    var chat = await Api.GetChatAsync(chatRepo.Id);
                    if (chat == null)
                    {
                        _logger.LogInformation($"Chat '{chatRepo.Title}' not found");
                        continue;
                    }

                    ChatMember chatMember = await Api.GetChatMemberAsync(chat.Id, (int)user.Id);
                    _logger.LogInformation($"For chat='{chat.Title}', user={user.Name}, status={chatMember.Status}");
                    if (chatMember.Status != ChatMemberStatus.Kicked && chatMember.Status != ChatMemberStatus.Left)
                    {
                        await Api.BanChatMemberAsync(chat.Id, (int)user.Id);
                        _logger.LogInformation($"User '{user.Name}' kicked from chat '{chatRepo.Title}'");
                        buffer.AppendLine($"User <b>{user.Name}</b> kicked from chat <b>{chatRepo.Title}</b>");
                        await Task.Delay(200);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return new CommandResult(buffer.ToString(), CommandResultType.TextMessage);
        }

        public async Task<CommandResult> NoWalk(User user)
        {
            var userName = user.GetUserName();
            var userRepo = await UserService.Get(user.Id);
            if (userRepo == null)
                return new CommandResult($"User {userName} not found", CommandResultType.TextMessage);

            if (userRepo.Type == UserType.Coordinator)
                return new CommandResult($"Forbidden for Coordinator");

            if (!userRepo.NoWalk.HasValue || !userRepo.NoWalk.Value)
            {
                userRepo.NoWalk = true;
                await UserService.Update(userRepo);
                return new CommandResult($"You're not walking, <b>{userName}</b>", CommandResultType.TextMessage);
            }
            return new CommandResult("", CommandResultType.None);
        }

        public async Task<CommandResult> BanAll(bool onlyWalking = false)
        {
            var users = await UserService.GetList();

            StringBuilder sb = new StringBuilder();
            foreach (var user in users)
            {
                if (onlyWalking && user.NoWalk.HasValue && user.NoWalk.Value)
                    continue;

                var commandResult = await Ban(user.Id.ToString());
                sb.AppendLine(commandResult.Content.ToString());
            }

            return new CommandResult(sb.ToString(), CommandResultType.TextMessage);
        }

        public async Task<CommandResult> DeleteChat(string chatName)
        {
            var chatRepo = await ChatService.Get(chatName);
            if (chatRepo == null)
                return new CommandResult(Messages.ChatNotFound, CommandResultType.TextMessage);

            var messages = await UserMessageService.GetList();
            var msgIdToDelete = messages.Where(m => m.ChatId == chatRepo.Id).Select(m => m.Id).ToList();
            foreach (var id in msgIdToDelete)
                await UserMessageService.Delete(id);


            var users = (await UserService.GetList()).Where(u => u.Id != Api.GetMe().Id);
            foreach (var user in users)
            {
                try
                {
                    await Api.BanChatMemberAsync(chatRepo.Id, (int)user.Id);
                    _logger.LogInformation($"User '{user.Name}' deleted from chat '{chatRepo.Title}'");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can't delete user '{user.Name}' from chat '{chatName}': {ex.Message}");
                }
            }

            await Api.LeaveChatAsync(chatRepo.Id);
            _logger.LogInformation($"Chat {chatName}, type={chatRepo.ChatType} deleted");

            await ChatService.Delete(chatRepo.Id);
            return new CommandResult("", CommandResultType.None);
        }

        public async Task<CommandResult> DeleteUser(string userNameOrId)
        {
            var user = await UserService.GetByIdOrName(userNameOrId);
            if (user == null)
                return new CommandResult(Messages.UserNotFound, CommandResultType.TextMessage);

            await UserService.Delete(user.Id);

            var messages = await UserMessageService.GetList();
            var msgIdToDelete = messages.Where(m => m.UserId == user.Id).Select(m => m.Id).ToList();
            foreach (var id in msgIdToDelete)
                await UserMessageService.Delete(id);

            _logger.LogInformation($"User {user.Name}, type={user.Type} deleted");

            return new CommandResult($"User {user.Name} deleted", CommandResultType.TextMessage);
        }

        public async Task<CommandResult> ChangeUserType(User from, string commandText)
        {
            var blocks = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => b.ToLower()).ToArray();

            var fieldNames = typeof(UserType).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(f => f.Name != nameof(UserType.Bot));

            var changedUserType = fieldNames.FirstOrDefault(ut => ut.Name.ToLower() == blocks[1]);
            if (changedUserType == null)
                return new CommandResult("Incorrect user type", CommandResultType.TextMessage);

            var userIdOrName = commandText.Replace(blocks[0], "").Replace(blocks[1], "").Trim();
            var user = await UserService.GetByIdOrName(userIdOrName);
            if (user == null)
                return new CommandResult(Messages.UserNotFound, CommandResultType.TextMessage);

            var sender = await UserService.Get(from.Id);
            if (sender.Type == UserType.Admin &&
                blocks[1].ToLower() == UserType.Coordinator.ToLower())
                return new CommandResult("Only coordinators can set Coordinator type", CommandResultType.TextMessage);

            user.Type = changedUserType.Name;
            await UserService.Update(user);

            return new CommandResult($"Changed type of user <b>{user.Name}</b> to " +
                $"<b>{changedUserType.Name}</b>", CommandResultType.TextMessage);
        }

        public async Task<CommandResult> Backup()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string directory = Path.Combine(Path.GetDirectoryName(assembly.Location), "backups");
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return new CommandResult($"Can't create directory {directory}", CommandResultType.TextMessage);
            }

            var formatter = new BinaryFormatter();
            string fileName = $"Backup.{DateTimeService.GetDateTimeUTCNow().ToString("yyyyMMdd.HHmmss")}";
            try
            {
                var backupContaner = new BackupContaner();
                backupContaner.Users = (await UserService.GetList()).ToList();
                backupContaner.Chats = (await ChatService.GetList()).ToList();
                backupContaner.UserMessages = (await UserMessageService.GetList()).ToList();

                using var fs = new FileStream(Path.Combine(directory, fileName), FileMode.Create);
                formatter.Serialize(fs, backupContaner);
                _logger.LogInformation($"Backup '{fileName}' successfully created in '{directory}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new CommandResult($"Can't make backup '{fileName}' in '{directory}'", CommandResultType.TextMessage);
            }
            return new CommandResult($"Backup created <b>{fileName}</b>", CommandResultType.TextMessage);
        }

        public async Task<CommandResult> Restore()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string directory = Path.Combine(Path.GetDirectoryName(assembly.Location), "backups");
            string[] backupFilenames = null;
            try
            {
                if (!Directory.Exists(directory))
                {
                    _logger.LogWarning($"Directory {directory} doesn't exist");
                    return new CommandResult(null);
                }
                backupFilenames = Directory.GetFiles(directory, "Backup.*");
                if (!backupFilenames.Any())
                {
                    _logger.LogWarning($"Backups not found in directory {directory}");
                    return new CommandResult(null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex.ToString());
                return new CommandResult($"Something got wrong :(", CommandResultType.TextMessage);
            }

            Array.Sort(backupFilenames);

            string fileName = backupFilenames.Last();
            var shortName = Path.GetFileName(fileName);
            var formatter = new BinaryFormatter();
            try
            {
                _logger.LogInformation($"Starting restoring backup from '{fileName}'");
                using var fs = new FileStream(fileName, FileMode.Open);
                var contaner = (BackupContaner)formatter.Deserialize(fs);
                await UserMessageService.Clear();
                _logger.LogInformation($"Backup UserMessages cleared");
                await UserService.Clear();
                _logger.LogInformation($"Backup Users cleared");
                await ChatService.Clear();
                _logger.LogInformation($"Backup Chats cleared");
                await ChatService.Add(contaner.Chats);
                _logger.LogInformation($"Backup Chats restored");
                await UserService.Add(contaner.Users);
                _logger.LogInformation($"Backup Users restored");
                await UserMessageService.Add(contaner.UserMessages);
                _logger.LogInformation($"Backup UserMessages restored");
                _logger.LogInformation($"Backup '{shortName}' successfull restored");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new CommandResult($"Can't restore backup '{shortName}'", CommandResultType.TextMessage);
            }
            return new CommandResult($"Backup restored <b>{shortName}</b>", CommandResultType.TextMessage);
        }


        #endregion

        private async Task<Message> SendTextMessage(long chatId, string content, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var message = await Api.SendMessageAsync(chatId: chatId, text: content,
                parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendImageMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await Api.GetChatAsync(chatId);
            var message = await Api.SendPhotoAsync(chatId: chat.Id, photo: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendVideoMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await Api.GetChatAsync(chatId);
            var message = await Api.SendVideoAsync(chatId: chat.Id, video: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendDocumentMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await Api.GetChatAsync(chatId);
            var message = await Api.SendDocumentAsync(chatId: chat.Id, document: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendPollMessage(long chatId, string content, IEnumerable<string> cases, bool? isMultiAnswers, bool? isAnonymous, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var chat = await Api.GetChatAsync(chatId);
            var message = await Api.SendPollAsync(
                chatId: chat.Id,
                question: content,
                options: cases,
                replyMarkup: markUp,
                isAnonymous: isAnonymous,
                allowsMultipleAnswers: isMultiAnswers);

            return message;
        }

        private async Task<Message> SendLinksList(long chatId, IEnumerable<Link> links)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var link in links)
            {
                sb.Append($"{link.Description}{Environment.NewLine}{link.Url}{Environment.NewLine}{Environment.NewLine}");
            }
            return await SendTextMessage(chatId, sb.ToString());
        }

        private Task<Message> SendMessageObject(long chatId, CommandResult commandResult)
        {
            if (!(commandResult.Content is Message message))
                return null;

            var text = new HtmlTextFormatGenerator().GenerateHtmlText(message);

            if (message.Photo != null)
            {
                return SendImageMessage(chatId, message.Photo.First().FileId, text, message.ReplyMarkup);
            }
            else if (message.Text != null)
            {
                return SendTextMessage(chatId, text, message.ReplyMarkup);
            }
            else if (message.Video != null)
            {
                return SendVideoMessage(chatId, message.Video.FileId, text, message.ReplyMarkup);
            }
            else if (message.Document != null)
            {
                return SendDocumentMessage(chatId, message.Document.FileId, text, message.ReplyMarkup);
            }

            return null;
        }

        private async Task<Message> SendWelcomeGroupMessage(long chatId, string userName, string chatType, int messageId = 0)
        {
            return await Api.SendMessageAsync(
                    chatId: chatId,
                    text: WelcomeService.GetWelcomeMessage(userName),
                    parseMode: ParseMode.HTML,
                    replyToMessageId: messageId,
                    replyMarkup: new InlineKeyboardMarkup(WelcomeService.GetWelcomeButtons(chatType))
            );
        }

        private async Task<Message> DeleteMessages(long chatId, string message)
        {
            var chat = await ChatService.Get(chatId);

            var messages = await UserMessageService.GetList();
            var messagesToDelete = messages.Where(m => m.ChatId == chatId).ToList();

            foreach (var msg in messagesToDelete)
            {
                try
                {
                    await UserMessageService.Delete(msg.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can't delete message '{msg.Text}' from chat '{chat.Title}': {ex.Message}");
                }
            }

            return await SendTextMessage(chatId, message);
        }

        private async Task<Message> UpdatePassword(long chatId, string chatType, string newPassword)
        {
            var password = await PasswordService.GetByType(chatType);
            password.Value = newPassword;
            await PasswordService.Update(password);

            return await SendTextMessage(chatId, "Password updated");
        }

        private async Task<Message> UpdateChatType(long chatId, long chatToChangeId, string chatType)
        {
            var chatToChange = await ChatService.Get(chatToChangeId);
            chatToChange.ChatType = chatType;
            await ChatService.Update(chatToChange);

            return await SendTextMessage(chatId, $"Chat type changed to {chatType}");
        }

        private async Task<Message> UnpinMessage(long chatId, long unpinChatId, long unpinMessageId)
        {
            var chatToChange = await ChatService.Get(unpinChatId);
            Message message;
            try
            {
                var messagesList = await UserMessageService.GetList();

                var pinnedMessages = messagesList.Where(m => m.ChatId == unpinChatId &&
                    m.Pinned != null && m.Pinned.HasValue && m.Pinned.Value).
                    OrderBy(m => m.When).ToList();
                var msg = pinnedMessages.First(c => c.Id == unpinMessageId);

                await UserMessageService.UnPin(msg.Id);
                await Api.UnPinChatMessageAsync(unpinChatId);
                pinnedMessages.Remove(msg);

                foreach (var pinnedMsg in pinnedMessages)
                {
                    await Api.PinChatMessageAsync(unpinChatId, pinnedMsg.TelegramId);
                }

                message = await Api.SendMessageAsync(
                    new SendMessageArgs(chatId, "Messages unpinned")
                    {
                        ParseMode = ParseMode.HTML
                    });
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Cannot unpin messages from chat '{chatToChange.Title}': {ex.Message}");
                message = await SendTextMessage(chatId, $"Something got wrong");
            }

            return message;
        }

        private async Task AddGroup(Chat chat)
        {
            var chatRepo = new Core.Model.Chat
            {
                Id = chat.Id,
                Title = chat.Title,
                ChatType = Core.Model.ChatType.Public
            };
            await ChatService.Add(chatRepo);
        }

        private async Task AddMessage(Message message, Core.Model.Chat chat = null)
        {
            if (chat == null)
                chat = await ChatService.Get(message.Chat.Id);

            string txtMessage = !string.IsNullOrEmpty(message.Caption) ? message.Caption : message.Text;

            var userMessage = new UserMessage
            {
                Id = DateTimeService.GetDateTimeUTCNow().Ticks,
                TelegramId = message.MessageId,
                ChatId = message.Chat.Id,
                UserId = message.From.Id,
                Text = txtMessage,
                ChatType = chat.ChatType,
                When = DateTimeService.GetDateTimeUTCNow()
            };
            await UserMessageService.Add(userMessage);
        }

        private async Task<Core.Model.Chat> EnsureChatSaved(Chat chat, User user = null)
        {
            var chatRepo = await ChatService.Get(chat.Id);
            if (chatRepo == null)
            {
                chatRepo = new Core.Model.Chat
                {
                    Id = chat.Id,
                    Title = chat.Title,
                    ChatType = chat.Type == ChatType.Private.ToLower() ? Core.Model.ChatType.Private : Core.Model.ChatType.Public
                };

                if (chatRepo.ChatType == Core.Model.ChatType.Private && user != null)
                    chatRepo.Title = user.Id.ToString();

                await ChatService.Add(chatRepo);
            }
            return chatRepo;
        }

        private async Task EnsureUserSaved(User user, string chatType)
        {
            var userRepo = await UserService.Get(user.Id);
            if (userRepo == null)
            {
                userRepo = new Core.Model.User
                {
                    Id = user.Id,
                    Name = user.GetUserName(),
                    Type = chatType == Core.Model.ChatType.Admin ? UserType.Admin : UserType.Member
                };
                userRepo.IsAdmin = userRepo.Type == UserType.Admin;
                await UserService.Add(userRepo);
            }

            userRepo.LastMessageTime = DateTimeService.GetDateTimeUTCNow();
            userRepo.Name = user.GetUserName();
            await UserService.Update(userRepo);
        }

        private async Task UpdateChatId(long oldId, long newId)
        {
            var chatToModify = await ChatService.Get(oldId);
            chatToModify.Id = newId;
            await ChatService.Update(oldId, chatToModify);
        }
    }
}
