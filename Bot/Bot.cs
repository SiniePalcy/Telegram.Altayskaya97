using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Altayskaya97.Bot.Helpers;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using Chat = Telegram.Bot.Types.Chat;
using ChatType = Telegram.Bot.Types.Enums.ChatType;
using User = Telegram.Bot.Types.User;
using ChatRepo = Telegram.Altayskaya97.Core.Model.Chat;
using UserRepo = Telegram.Altayskaya97.Core.Model.User;
using Microsoft.Extensions.Configuration;
using Telegram.Altayskaya97.Bot.Model;
using Telegram.Bot.Exceptions;
using System.Reflection;

namespace Telegram.Altayskaya97.Bot
{
    public class Bot : BackgroundService
    {
        private readonly ILogger<Bot> _logger;
        //private readonly IConfiguration _configuration;
        public ITelegramBotClient BotClient { get; set; }

        public int PeriodEchoSec { get; private set; }
        public int PeriodResetAccessMin { get; private set; }
        public int PeriodChatListMin { get; private set; }
        public int PeriodClearPrivateChatMin { get; private set; }
        public int PeriodInactiveUserDays { get; private set; }

        private ConcurrentDictionary<long, int> _adminResetCounters = new ConcurrentDictionary<long, int>();
        private volatile int _chatListCounter = 0;

        #region Constant
        private const string INCORRECT_COMMAND = "Incorrect command";
        private const string NO_PERMISSIONS = "No permissions. Please, input secret command";
        private const int PERIOD_ECHO_SEC_DEFAULT = 20;
        private const int PERIOD_RESET_ACCESS_MIN_DEFAULT = 60;
        private const int PERIOD_CHAT_LIST_MIN_DEFAULT = 180;
        private const int PERIOD_CLEAR_PRIVATE_CHAT_MIN_DEFAULT = 30;
        private const int PERIOD_INACTIVE_USER_DAYS = 7;
        #endregion

        #region Services
        public IWelcomeService WelcomeService { get; set; }
        public IMenuService MenuService { get; set; }
        public IUserService UserService { get; set; }
        public IChatService ChatService { get; set; }
        public IUserMessageService UserMessageService { get; set; }
        public IDateTimeService DateTimeService { get; set; }
        #endregion

        public Bot(ILogger<Bot> logger, IConfiguration configuration,
            IWelcomeService welcomeService,
            IMenuService menuService,
            IUserService userService,
            IChatService chatService,
            IUserMessageService userMessageService,
            IDateTimeService dateTimeService,
            bool shouldInitClient = true,
            bool shouldInitDb = true)
        {
            _logger = logger;

            this.WelcomeService = welcomeService;
            this.MenuService = menuService;
            this.UserService = userService;
            this.ChatService = chatService;
            this.UserMessageService = userMessageService;
            this.DateTimeService = dateTimeService;

            var configSection = configuration.GetSection("Configuration");

            if (shouldInitClient)
                InitClient(configSection);

            if (shouldInitDb)
                InitDb();

            InitProps(configSection);
        }

        #region Initialize
        private void InitProps(IConfigurationSection configSection)
        {
            PeriodEchoSec = ParseInt(configSection.GetSection("PeriodEchoSec").Value, PERIOD_ECHO_SEC_DEFAULT);
            PeriodResetAccessMin = ParseInt(configSection.GetSection("PeriodResetAccessMin").Value, PERIOD_RESET_ACCESS_MIN_DEFAULT);
            PeriodChatListMin = ParseInt(configSection.GetSection("PeriodChatListMin").Value, PERIOD_CHAT_LIST_MIN_DEFAULT);
            PeriodClearPrivateChatMin = ParseInt(configSection.GetSection("PeriodClearPrivateChatMin").Value, PERIOD_CLEAR_PRIVATE_CHAT_MIN_DEFAULT);
            PeriodInactiveUserDays = ParseInt(configSection.GetSection("PeriodInactiveUserDays").Value, PERIOD_INACTIVE_USER_DAYS);
        }

        private void InitClient(IConfigurationSection configSection)
        {
            string botName = GlobalEnvironment.BotName.StartsWith("@") ? GlobalEnvironment.BotName.Remove(0, 1) : GlobalEnvironment.BotName;
            string accessToken = configSection.GetSection(botName).Value;
            BotClient = new TelegramBotClient(accessToken);

            BotClient.OnMessage += Bot_OnMessage;
            BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            BotClient.OnInlineQuery += BotClient_OnInlineQuery;
            BotClient.StartReceiving();
        }

        private async void InitDb()
        {
            var chatList = await ChatService.GetChatList();
            if (!chatList.Any())
                return;

            var adminChats = chatList.Where(c => c.ChatType == Core.Model.ChatType.Admin);
            List<ChatMember> admins = new List<ChatMember>();
            foreach (var adminChat in adminChats)
            {
                try
                {
                    var adminsOfChat = await BotClient.GetChatAdministratorsAsync(adminChat.Id);
                    admins.AddRange(adminsOfChat.Where(usr => !usr.User.IsBot));
                }
                catch(ApiRequestException)
                {
                    _logger.LogInformation($"Chat {adminChat.Title} is unavailable and will be deleted");
                    await ChatService.DeleteChat(adminChat.Id);
                }
            }

            foreach (var admin in admins)
            {
                var userInRepo = await UserService.GetUser(admin.User.Id);
                if (userInRepo != null)
                {
                    _logger.LogInformation($"User with id={userInRepo.Id}, name={userInRepo.Name} is already exist");
                    continue;
                }

                string userName = admin.User.GetUserName();
                var newUser = new UserRepo
                {
                    Id = admin.User.Id,
                    Name = userName,
                    IsAdmin = true,
                    Type = admin.User.IsBot ? UserType.Bot : UserType.Admin
                };
                await UserService.AddUser(newUser);
                _logger.LogInformation($"User saved with id={newUser.Id}, name={newUser.Name}, type={newUser.Type}");
            }

            var users = await UserService.GetUserList();
            foreach(var user in users)
            {
                _adminResetCounters.TryAdd(user.Id, 0);
            }
        }
        #endregion

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeService.GetDateTimeUTCNow();
                await UpdateUsersAccess();
                await UpdateBotMessages();
                _logger.LogInformation($"[Echo] Bot v{Assembly.GetExecutingAssembly().GetName().Version} running at: {now}");
                await Task.Delay(PeriodEchoSec * 1000, stoppingToken);
            }
        }

        private async Task UpdateUsersAccess()
        {
            foreach (var userId in _adminResetCounters.Keys)
            {
                bool got = _adminResetCounters.TryGetValue(userId, out int counterValue);
                if (got && counterValue == PeriodResetAccessMin * 60 / PeriodEchoSec)
                {
                    _adminResetCounters.TryUpdate(userId, 0, counterValue);
                    await UserService.RestrictUser(userId);
                }
                _adminResetCounters.TryUpdate(userId, counterValue + 1, counterValue);
            }
        }

        private async Task UpdateBotMessages()
        {
            var allMessages = await UserMessageService.GetUserMessageList();

            var dtNow = DateTimeService.GetDateTimeUTCNow();
            List<UserMessage> messagesForDelete = new List<UserMessage>();
            foreach (var message in allMessages)
            {
                var msgDateTime = message.When.ToUniversalTime();
                if ((dtNow - msgDateTime).TotalMinutes >= PeriodClearPrivateChatMin)
                    messagesForDelete.Add(message);
            }

            foreach (var message in messagesForDelete)
            {
                await BotClient.DeleteMessageAsync(message.ChatId, (int)message.Id);
                await UserMessageService.DeleteUserMessage(message.Id);
            }
        }

        private async Task UpdateChatList()
        {
            if (_chatListCounter == PeriodChatListMin * 60 / PeriodEchoSec)
            {
                _chatListCounter = 0;

                var chatList = await ChatService.GetChatList();
                foreach (var chatRepo in chatList)
                {
                    var chat = await BotClient.GetChatAsync(chatRepo.Id);
                    
                    try
                    {
                        int chatMembers = await BotClient.GetChatMembersCountAsync(chat.Id);
                        if (chat == null || chatMembers <= 1)
                        {
                            await ChatService.DeleteChat(chatRepo.Id);
                        }
                    }
                    catch (Exception)
                    {
                        await ChatService.DeleteChat(chat.Id);
                    }
                }
            }

            _chatListCounter++;
        }

        #region Event handlers
        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            //don't change this method for saving test cover
            await RecieveMessage(e.Message);
        }

        private async void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            await BotClient.SendTextMessageAsync(
                chatId: e.InlineQuery.From.Id,
                text: e.InlineQuery.Query);
        }

        private async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            //don't change this method for saving test cover
            await RecieveCallbackData(e.CallbackQuery.Message.Chat, e.CallbackQuery.From, e.CallbackQuery.Data);
        }
        #endregion

        public async Task RecieveMessage(Message message)
        {
            if (message.Chat.Type == ChatType.Private)
                await ProcessBotMessage(message);
            else
                await ProcessChatMessage(message);
        }

        public async Task RecieveCallbackData(Chat chat, User from, string data)
        {
            var userRepo = await UserService.GetUser(from.Id);
            if (userRepo == null)
            {
                await SendTextMessage(chat.Id, "Unknown user");
                return;
            }

            if (data == CallbackActions.IWalk)
            {
                await Ban(Commands.GetCommand($"/ban {userRepo.Id}"));
            }
        }

        public async Task ProcessBotMessage(Message chatMessage)
        {
            string commandText = chatMessage?.Text?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(commandText))
                return;

            await EnsureChatSaved(chatMessage.Chat, chatMessage.From);

            var user = chatMessage.From;
            _logger.LogInformation($"Recieved message from '{user.GetUserName()}', id={user.Id}");

            if (chatMessage.Chat.Type == ChatType.Private)
                await AddMessage(chatMessage);

            var command = Commands.GetCommand(commandText);
            if (command == null || !command.IsValid)
                return;

            CommandResult commandResult;
            var userRepo = await UserService.GetUser(user.Id);
            var isAdmin = await UserService.IsAdmin(user.Id);
            if (userRepo == null)
            {
                commandResult = command.Name == Commands.Start.Name ? 
                    new CommandResult("Who are you? Let's goodbye!", CommandResultType.Message) :
                    new CommandResult(INCORRECT_COMMAND);
            }
            else if (userRepo.Type == UserType.Member)
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) :
                                command.Name == Commands.IWalk.Name ? await Ban(Commands.GetCommand($"/ban {userRepo.Id}")) :
                                command.Name == Commands.Return.Name ? await Unban(user) :
                                    new CommandResult(INCORRECT_COMMAND);
            }
            else if (command.IsAdmin && isAdmin) //commands for admin with permissions
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) :
                                command.Name == Commands.GrantAdmin.Name ? await GrantAdminPermissions(user) :
                                command.Name == Commands.ChatList.Name ? await ChatList() :
                                command.Name == Commands.UserList.Name ? await UserList() :
                                command.Name == Commands.IWalk.Name ? await Ban(Commands.GetCommand($"/ban {userRepo.Id}")) :
                                command.Name == Commands.Ban.Name ? await Ban(command) :
                                command.Name == Commands.BanAll.Name ? await BanAll() :
                                command.Name == Commands.InActive.Name ? await InActiveUsers() :
                                    new CommandResult(INCORRECT_COMMAND, CommandResultType.Message);
            }
            else  //commands for admin without permissions
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) :
                                command.Name == Commands.ChatList.Name ? new CommandResult(NO_PERMISSIONS, CommandResultType.Message) :
                                command.Name == Commands.UserList.Name ? new CommandResult(NO_PERMISSIONS, CommandResultType.Message) :
                                command.Name == Commands.IWalk.Name ? await Ban(Commands.GetCommand($"/ban {userRepo.Id}")) :
                                command.Name == Commands.Ban.Name ? new CommandResult(NO_PERMISSIONS, CommandResultType.Message) :
                                command.Name == Commands.BanAll.Name ? new CommandResult(NO_PERMISSIONS, CommandResultType.Message) :
                                command.Name == Commands.GrantAdmin.Name ? await GrantAdminPermissions(user) :
                                    new CommandResult(INCORRECT_COMMAND);
            }

            var recievers = commandResult.Recievers ?? new List<long>() { chatMessage.Chat.Id };

            foreach (var reciever in recievers)
            {
                if (commandResult.Type == CommandResultType.Message)
                    await SendTextMessage(reciever, commandResult.Content, commandResult.ReplyMarkup);
                else if (commandResult.Type == CommandResultType.Links)
                {
                    foreach (var link in commandResult.Links)
                        await SendTextMessage(reciever, $"{link.Description}{Environment.NewLine}{link.Url}");
                }
            }
        }

        public async Task ProcessChatMessage(Message chatMessage)
        {
            Chat chat = chatMessage.Chat;

            await EnsureChatSaved(chat);

            if (chatMessage.Type == MessageType.GroupCreated || chatMessage.Type == MessageType.SupergroupCreated)
            {
                await AddGroup(chat);
                return;
            }

            var chatRepo = await ChatService.GetChat(chat.Id);
            User sender = chatMessage.From;

            if (chatMessage.Type == MessageType.ChatMembersAdded)
            {
                var users = await UserService.GetUserList();
                var newMembers = chatMessage.NewChatMembers.Where(c => !users.Any(u => u.Id == c.Id)).ToList();
                newMembers.ForEach(async chatMember => await SendWelcomeGroupMessage(chatMessage.Chat, chatMember.GetUserName(), chatRepo.ChatType));
                return;
            }

            await EnsureUserSaved(sender, chatRepo.ChatType);

            if (string.IsNullOrEmpty(chatMessage.Text))
                return;

            string message = chatMessage.Text.Trim().ToLower();

            var command = Commands.GetCommand(message);
            if (command == null || !command.IsValid)
                return;

            string userName = sender.GetUserName();

            _logger.LogInformation($"Recieved message from {userName} in chat id={chat?.Id}, title={chat?.Title}, type={chat?.Type}");


            if (message == Commands.Help.Name)
                await SendWelcomeGroupMessage(chatMessage.Chat, userName, chatRepo.ChatType, chatMessage.MessageId);
            else if (message == Commands.Helb.Name)
                await BotClient.SendPhotoAsync(chatMessage.Chat, "https://i.ytimg.com/vi/gpEtNGeM3zE/maxresdefault.jpg");

            return;
        }

        public async Task<CommandResult> Start(User user)
        {
            bool isAdmin = await UserService.IsAdmin(user.Id);
            return new CommandResult(MenuService.GetMenu(user.Username, isAdmin), CommandResultType.Message, 
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

            var chatList = await ChatService.GetChatList();
            List<Core.Model.Chat> chatsToDelete = new List<ChatRepo>();
            foreach (var chat in chatList)
            {
                try
                {
                    if (chat.ChatType == Core.Model.ChatType.Private)
                        continue;

                    if (onlyPublicChates && chat.ChatType == Core.Model.ChatType.Admin)
                        continue;

                    var chatMember = await BotClient.GetChatMemberAsync(chat.Id, user.Id);
                    if (chatMember == null || chatMember.Status == ChatMemberStatus.Kicked || chatMember.Status == ChatMemberStatus.Left)
                    {
                        if (chatMember.Status == ChatMemberStatus.Kicked)
                            await BotClient.UnbanChatMemberAsync(chat.Id, user.Id);
                        var inviteLink = await BotClient.ExportChatInviteLinkAsync(chat.Id);
                        result.Links.Add(new Link
                        {
                            Url = inviteLink,
                            Description = $"Chat <b>{chat.Title}</b>"
                        });
                        _logger.LogInformation($"Link to '{chat.Title}' formed for {user.GetUserName()}");
                    }
                }
                catch (ApiRequestException)
                {
                    _logger.LogInformation($"Chat '{chat.Title}' is unavailable and will be deleted");
                    chatsToDelete.Add(chat);
                }
            }

            foreach (var chat in chatsToDelete)
                await ChatService.DeleteChat(chat.Id);

            return result;
        }

        public async Task<CommandResult> ChatList()
        {
            var chatList = await ChatService.GetChatList();

            StringBuilder sb = new StringBuilder("<code>");
            foreach (var chat in chatList.Where(c => c.ChatType != Core.Model.ChatType.Private))
            {
                if (chat.ChatType != Core.Model.ChatType.Private)
                    sb.AppendLine($"id: {chat.Id,-20}title: <b>{chat.Title}</b>");
            }
            sb.Append("</code>");

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        public async Task<CommandResult> UserList()
        {
            var userList = await UserService.GetUserList();

            StringBuilder sb = new StringBuilder(string.Format($"<code>{"Username",-20}{"Type",-12}{"Access",-6}\n"));
            foreach (var user in userList.OrderBy(u => u.Type))
            {
                var isAdmin = await UserService.IsAdmin(user.Id);
                var userType = user.Type;
                var accessSign = isAdmin ? "  +" : "  -";
                sb.AppendLine($"{user.Name,-20}{userType,-12}{accessSign,-6}");
            }
            sb.Append("</code>");

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        public async Task<CommandResult> InActiveUsers()
        {
            string message = "No inactive users";

            var dtNow = DateTimeService.GetDateTimeUTCNow();
            var userList = await UserService.GetUserList();
            var inActiveUsers = userList.Where(u => (u.Type == UserType.Member || u.Type == UserType.Admin) && 
                (u.LastMessageTime == null || (dtNow - u.LastMessageTime.Value).TotalDays >= PeriodInactiveUserDays));

            if (inActiveUsers.Any())
            {
                StringBuilder sb = new StringBuilder(string.Format($"<code>{"Username",-20}{"Type",-12}{"Last msg",-16}\n"));
                foreach (var user in userList.OrderBy(u => u.Type).OrderBy(u => u.LastMessageTime))
                {
                    var userType = user.Type;
                    var lastDateTime =  DateTimeService.FormatToString(user.LastMessageTime.Value);
                    sb.AppendLine($"{user.Name,-20}{userType,-12}{lastDateTime,-16}");
                }
                sb.Append("</code>");
                message = sb.ToString();
            }

            return new CommandResult(message, CommandResultType.Message);
        }

        private async Task<CommandResult> Post(Command command)
        {
            ICollection<ChatRepo> chatsToPost = null;

            var firstWord = command.GetFirstWord().ToLower();
            if (firstWord == "all")
            {
                chatsToPost = await ChatService.GetChatList();
            }
            else
            {
                var chatId = command.GetFirstNumber();
                if (chatId != null)
                {
                    var chat = await ChatService.GetChat(chatId.Value);
                    if (chat != null)
                    {
                        chatsToPost = new List<ChatRepo>() { chat };
                    }
                }
            }

            if (chatsToPost == null || !chatsToPost.Any())
                return new CommandResult(Messages.CheckCommand, CommandResultType.Message);

            string contentToPost = command.Text.Replace(command.Name, "").Replace(firstWord, "").Trim();
            
            var commandResult = new CommandResult(contentToPost, CommandResultType.Message);
            commandResult.Recievers = chatsToPost.Select(c => c.Id).ToList();

            return commandResult;
        }

        public async Task<CommandResult> Ban(Command command)
        {
            var commandContent = command.Text.Replace(command.Name,"").Trim().ToLower();
            if (string.IsNullOrEmpty(commandContent))
                return new CommandResult(Messages.CheckCommand, CommandResultType.Message);

            long userId;
            Core.Model.User user;
            if (long.TryParse(commandContent, out userId))
                user = await UserService.GetUser(userId);
            else 
                user = await UserService.GetUser(commandContent);

            if (user == null)
                return new CommandResult(Messages.UserNotFound, CommandResultType.Message);

            if (user.Type == UserType.Coordinator)
                return new CommandResult(Messages.YouCantBanCoordinator, CommandResultType.Message);

            if (user.Type == UserType.Bot)
                return new CommandResult(Messages.YouCantBanBot, CommandResultType.Message);

            var chats = await ChatService.GetChatList();
            if (!chats.Any())
                return new CommandResult(Messages.NoAnyChats, CommandResultType.Message);

            StringBuilder buffer = new StringBuilder();
            foreach (var chatRepo in chats)
            {
                var chat = await BotClient.GetChatAsync(chatRepo.Id);
                if (chat == null)
                {
                    _logger.LogInformation($"Chat '{chatRepo.Title}' not found");
                    continue;
                }

                try
                {
                    if (chatRepo.ChatType == Core.Model.ChatType.Admin && user.Type == UserType.Member ||
                        chatRepo.ChatType == Core.Model.ChatType.Private)
                        continue;

                    ChatMember chatMember = await BotClient.GetChatMemberAsync(chat.Id, (int)user.Id);
                    _logger.LogInformation($"For chat='{chat.Title}', user={user.Name}, status={chatMember.Status}");
                    if (chatMember.Status != ChatMemberStatus.Kicked && chatMember.Status != ChatMemberStatus.Left)
                    {
                        await BotClient.KickChatMemberAsync(chat.Id, (int)user.Id);
                        _logger.LogInformation($"User '{user.Name}' kicked from chat '{chatRepo.Title}'");
                        buffer.AppendLine($"User <b>{user.Name}</b> deleted from chat <b>{chatRepo.Title}</b>");
                    }
                }
                catch (ApiRequestException ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return new CommandResult(buffer.ToString(), CommandResultType.Message);
        }

        public async Task<CommandResult> BanAll()
        {
            var users = await UserService.GetUserList();
            var chats = await ChatService.GetChatList();

            StringBuilder sb = new StringBuilder();
            foreach (var user in users)
            {
                if (user.Type == UserType.Coordinator || user.Type == UserType.Bot)
                    continue;

                foreach (var chatRepo in chats.Where(c => c.ChatType != Core.Model.ChatType.Private))
                {
                    var chat = await BotClient.GetChatAsync(chatRepo.Id);
                    if (chat == null)
                        continue;

                    var chatMember = await BotClient.GetChatMemberAsync(chat.Id, (int)user.Id);
                    if (chatMember == null || chatMember.User.IsBot)
                        continue;

                    try
                    {
                        await BotClient.KickChatMemberAsync(chat.Id, (int)user.Id);
                    }
                    catch (ApiRequestException ex)
                    {
                        _logger.LogInformation(ex.Message);
                    }

                    sb.AppendLine($"User <b>{user.Name}</b> has been deleted from chat <b>{chatRepo.Title}</b>");
                }
            }

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        private async Task<Message> SendTextMessage(long chatId, string content, IReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var chat = await BotClient.GetChatAsync(chatId);
            var message = await BotClient.SendTextMessageAsync(chatId: chat.Id, text: content, parseMode: ParseMode.Html, replyMarkup: markUp);
            
            if (message != null && chat.Type == ChatType.Private)
                await AddMessage(message);
            
            return message;
        }

        private async Task<Message> SendWelcomeGroupMessage(Chat chat, string userName, string chatType, int messageId = 0)
        {
            return await BotClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: WelcomeService.GetWelcomeMessage(userName),
                    parseMode: ParseMode.Html,
                    replyToMessageId: messageId,
                    replyMarkup: new InlineKeyboardMarkup(WelcomeService.GetWelcomeButtons(chatType))
            );
        }

        private async Task AddGroup(Chat chat)
        {
            var chatRepo = new ChatRepo
            {
                Id = chat.Id,
                Title = chat.Title,
                ChatType = Core.Model.ChatType.Public
            };
            await ChatService.AddChat(chatRepo);
        }

        private async Task AddMessage(Message message)
        {
            var userMessage = new UserMessage
            {
                Id = message.MessageId,
                ChatId = message.Chat.Id,
                UserId = message.From.Id,
                Text = message.Text,
                When = DateTimeService.GetDateTimeUTCNow()
            };
            await UserMessageService.AddUserMessage(userMessage);
        }

        private async Task EnsureChatSaved(Chat chat, User user = null)
        {
            var chatRepo = await ChatService.GetChat(chat.Id);
            if (chatRepo == null)
            {
                var dbChat = new Core.Model.Chat
                {
                    Id = chat.Id,
                    Title = chat.Title,
                    ChatType = chat.Type == ChatType.Private ? Core.Model.ChatType.Private : Core.Model.ChatType.Public
                };

                if (dbChat.ChatType == Core.Model.ChatType.Private && user != null)
                    dbChat.Title = user.Id.ToString();

                await ChatService.AddChat(dbChat);
            }
        }

        private async Task EnsureUserSaved(User user, string chatType)
        {
            var userRepo = await UserService.GetUser(user.Id);
            if (userRepo == null)
            {
                userRepo = new UserRepo
                {
                    Id = user.Id,
                    Name = user.GetUserName(),
                    Type = chatType == Core.Model.ChatType.Admin ? UserType.Admin : UserType.Member
                };
                userRepo.IsAdmin = userRepo.Type == Core.Model.UserType.Admin;
                await UserService.AddUser(userRepo);
                _adminResetCounters.TryAdd(userRepo.Id, 0);
            }

            userRepo.LastMessageTime = DateTimeService.GetDateTimeUTCNow();
            await UserService.UpdateUser(userRepo);
            
        }

        private int ParseInt(string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source) || !int.TryParse(source, out int result))
                result = defaultValue;

            return result;
        }
    }
}
