using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Altayskaya97.Core.Constant;
using Telegram.Altayskaya97.Service.Interface;
using System.Text;
using Telegram.Altayskaya97.Core.Model;
using ChatRepo = Telegram.Altayskaya97.Core.Model.Chat;
using UserRepo = Telegram.Altayskaya97.Core.Model.User;
using Microsoft.Extensions.Configuration;
using Telegram.Altayskaya97.Core.Model;
using System.Reflection;
using Telegram.Altayskaya97.Service.StateMachines;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;
using Telegram.BotAPI;
using Telegram.BotAPI.AvailableMethods;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI.AvailableMethods.FormattingOptions;
using User = Telegram.BotAPI.AvailableTypes.User;
using Chat = Telegram.BotAPI.AvailableTypes.Chat;
using Telegram.BotAPI.GettingUpdates;
using Telegram.Altayskaya97.Service.Extensions;
using Telegram.Altayskaya97.Core.Extensions;
using Telegram.Altayskaya97.Service;

namespace Telegram.Altayskaya97.Bot
{
    public class Worker : BackgroundService
    {
        private const int DEFAULT_USER_ID = 419930845;

        private readonly ILogger<Worker> _logger;

        private readonly IConfiguration _configuration;

        public ICollection<IStateMachine> StateMachines { get; set; }

        public BotClient BotInstance { get; set; }

        public int PeriodEchoSec { get; private set; }
        public int PeriodResetAccessMin { get; private set; }
        public int PeriodChatListMin { get; private set; }
        public int PeriodClearPrivateChatMin { get; private set; }
        public int PeriodClearGroupChatHours { get; private set; }
        public int PeriodInactiveUserDays { get; private set; }
        public TimeSpan WalkingTime { get; private set; }
        public List<DayOfWeek> BanDays { get; private set; } = new();

        private readonly ConcurrentDictionary<long, int> _adminResetCounters = new();
        private readonly CancellationTokenSource _cts = new();
        private volatile int _updateUserNameCounter = 0;
        private volatile bool _allKicked = false;

        #region Constant
        private const int PERIOD_ECHO_SEC_DEFAULT = 20;
        private const int PERIOD_RESET_ACCESS_MIN_DEFAULT = 60;
        private const int PERIOD_CHAT_LIST_MIN_DEFAULT = 180;
        private const int PERIOD_CLEAR_PRIVATE_CHAT_MIN_DEFAULT = 30;
        private const int PERIOD_INACTIVE_USER_DAYS = 7;
        private const string BAN_DAY_DEFAULT = "None";
        private readonly TimeSpan WALKING_TIME_DEFAULT = new TimeSpan(10, 40, 00);
        #endregion

        #region Services
        public IButtonsService WelcomeService { get; set; }
        public IMenuService MenuService { get; set; }
        public IPasswordService PasswordService { get; set; }
        public IUserService UserService { get; set; }
        public IChatService ChatService { get; set; }
        public IUserMessageService UserMessageService { get; set; }
        public IDateTimeService DateTimeService { get; set; }
        #endregion

        public Worker(ILogger<Worker> logger, IConfiguration configuration,
            IButtonsService welcomeService,
            IMenuService menuService,
            IUserService userService,
            IChatService chatService,
            IUserMessageService userMessageService,
            IPasswordService passwordService,
            IDateTimeService dateTimeService,
            bool shouldInitClient = true,
            bool shouldInitDb = true)
        {
            _logger = logger;
            _configuration = configuration;
            this.WelcomeService = welcomeService;
            this.MenuService = menuService;
            this.UserService = userService;
            this.ChatService = chatService;
            this.UserMessageService = userMessageService;
            this.PasswordService = passwordService;
            this.DateTimeService = dateTimeService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot initializing at: {Time}", DateTimeOffset.Now);

            var configSection = _configuration.GetSection("Configuration");
            InitClient(configSection);
            await InitDb();
            InitProps(configSection);

            _logger.LogInformation("Bot starting at: {Time}", DateTimeOffset.Now);
            await base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation("Bot stopping at: {Time}", DateTimeOffset.Now);
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker running at: {Time}", DateTimeOffset.Now);

            // Long Polling
            var updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
            while (!stoppingToken.IsCancellationRequested)
            {
                if (updates.Any())
                {
                    Parallel.ForEach(updates, (update) => ProcessUpdate(update));

                    updates = await _api.GetUpdatesAsync(updates[^1].UpdateId + 1, cancellationToken: stoppingToken).ConfigureAwait(false);
                }
                else
                {
                    updates = await _api.GetUpdatesAsync(cancellationToken: stoppingToken).ConfigureAwait(false);
                }
            }
        }

        private void ProcessUpdate(Update update)
        {
            using var scope = _serviceProvider.CreateScope();
            var bot = scope.ServiceProvider.GetRequiredService<HelloBot>();
            bot.OnUpdate(update);
        }

        #region Initialize
        private void InitProps(IConfigurationSection configSection)
        {
            PeriodEchoSec = configSection.GetSection("PeriodEchoSec").Value.ParseInt(PERIOD_ECHO_SEC_DEFAULT);
            PeriodResetAccessMin = configSection.GetSection("PeriodResetAccessMin").Value.ParseInt(PERIOD_RESET_ACCESS_MIN_DEFAULT);
            PeriodChatListMin = configSection.GetSection("PeriodChatListMin").Value.ParseInt(PERIOD_CHAT_LIST_MIN_DEFAULT);
            PeriodClearPrivateChatMin = configSection.GetSection("PeriodClearPrivateChatMin").Value.ParseInt(PERIOD_CLEAR_PRIVATE_CHAT_MIN_DEFAULT);
            PeriodClearGroupChatHours = configSection.GetSection("PeriodClearGroupChatHours").Value.ParseInt(PERIOD_CLEAR_PRIVATE_CHAT_MIN_DEFAULT);
            PeriodInactiveUserDays = configSection.GetSection("PeriodInactiveUserDays").Value.ParseInt(PERIOD_INACTIVE_USER_DAYS);
            WalkingTime = configSection.GetSection("WalkingTime").Value.ParseTimeSpan(WALKING_TIME_DEFAULT);
            var banDaysString = configSection.GetSection("BanDays").Value.ParseString(BAN_DAY_DEFAULT);
            var banDaysStringList = banDaysString.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach(var banDay in banDaysStringList)
            {
                if (System.Enum.TryParse<DayOfWeek>(banDay.Trim(), true, out DayOfWeek banDayOfWeek))
                    BanDays.Add(banDayOfWeek);
            }
        }

        private void InitClient(IConfigurationSection configSection)
        {
            string botName = GlobalEnvironment.BotName.StartsWith("@") ? GlobalEnvironment.BotName.Remove(0, 1) : GlobalEnvironment.BotName;
            string accessToken = configSection.GetSection(botName).Value;
            BotInstance = new BotClient(accessToken);
            StateMachines = new IStateMachine[]
            {
                new PostStateMachine(ChatService),
                new PollStateMachine(ChatService),
                new ClearStateMachine(ChatService),
                new ChangePasswordStateMachine(PasswordService),
                new ChangeChatTypeStateMachine(ChatService),
                new UnpinMessageStateMachine(ChatService, UserMessageService)
            };

            var me = BotInstance.GetMe();
            

            //BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            //BotClient.OnInlineQuery += BotClient_OnInlineQuery;
            
            //BotClient.OnMessage += Bot_OnMessage;
        }

        private async Task InitDb()
        {
            var passwords = await PasswordService.GetList();
#if DEBUG
            foreach (var pass in passwords)
                await PasswordService.Delete(pass.Id);
            passwords = await PasswordService.GetList();
#endif
            var maxId = !passwords.Any() ? 0 : passwords.Select(p => p.Id).Max();
            if (!passwords.Any(p => p.ChatType == Core.Model.ChatType.Admin))
            {
                await PasswordService.Add(new Password
                {
                    Id = ++maxId,
                    ChatType = Core.Model.ChatType.Admin,
                    Value = "/admin"
                });
            }
            if (!passwords.Any(p => p.ChatType == Core.Model.ChatType.Public))
            {
                await PasswordService.Add(new Password
                {
                    Id = ++maxId,
                    ChatType = Core.Model.ChatType.Public,
                    Value = "/public"
                });
            }

            var userList = await UserService.GetList();
            if (!userList.Any())
            {
                await UserService.Add(new UserRepo
                {
                    Id = DEFAULT_USER_ID,
                    Name = "MukaLudac",
                    Type = UserType.Admin,
                    IsAdmin = true,
                });
            }
                
            var chatList = await ChatService.GetList();
            if (!chatList.Any())
                return;

            var adminChats = chatList.Where(c => c.ChatType == Core.Model.ChatType.Admin);
            List<ChatMember> admins = new List<ChatMember>();
            foreach (var adminChat in adminChats)
            {
                try
                {
                    var adminsOfChat = await BotInstance.GetChatAdministratorsAsync(adminChat.Id);
                    admins.AddRange(adminsOfChat.Where(usr => !usr.User.IsBot));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Chat {adminChat.Title} is unavailable and will be deleted");
                    await ChatService.Delete(adminChat.Id);
                }
            }

            foreach (var admin in admins)
            {
                var userInRepo = await UserService.Get(admin.User.Id);
                if (userInRepo != null)
                {
                    _adminResetCounters.TryAdd(userInRepo.Id, 0);
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
                await UserService.Add(newUser);
                _logger.LogInformation($"User saved with id={newUser.Id}, name={newUser.Name}, type={newUser.Type}");

                _adminResetCounters.TryAdd(newUser.Id, 0);
            }

            var users = await UserService.GetList();
            var userAdmins = users.Where(u => u.Type == UserType.Admin).ToList();
            userAdmins.ForEach(u => _adminResetCounters.TryAdd(u.Id, 0));
        }
        #endregion

        #region Periodically updaters

        private async Task UpdateUsers()
        {
            await UpdateUsersAccess();
            await UpdateUserNames();
            await UpdateNoWalk();
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

        private async Task UpdateMessages()
        {
            var allMessages = await UserMessageService.GetList();

            var dtNow = DateTimeService.GetDateTimeUTCNow();
            List<UserMessage> messagesForDelete = new List<UserMessage>();
            foreach (var message in allMessages)
            {
                var msgDateTime = message.When.ToUniversalTime();
                var timePassed = dtNow - msgDateTime;
                var minutePassed = timePassed.TotalMinutes;
                if ((string.IsNullOrEmpty(message.ChatType) || message.ChatType == Core.Model.ChatType.Private) 
                    && minutePassed >= PeriodClearPrivateChatMin)
                    messagesForDelete.Add(message);
            }

            foreach (var message in messagesForDelete)
            {
                try
                {
                    await UserMessageService.Delete(message.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can't delete message from user id='{message.UserId}': {ex.Message}");
                }
            }
        }

        private async Task UpdateUserNames()
        {
            if (_updateUserNameCounter == 12 * 3600 / PeriodEchoSec)
            {
                _updateUserNameCounter = 0;

                var userList = await UserService.GetList();
                var chatList = await ChatService.GetList();
                foreach (var userRepo in userList)
                {
                    User user = null;
                    foreach (var chatRepo in chatList)
                    {
                        try
                        {
                            var chatMember = await BotInstance.GetChatMemberAsync(chatRepo.Id, userRepo.Id);
                            if (chatMember != null)
                            {
                                user = chatMember.User;
                                break;
                            }
                        }
                        catch
                        { }
                    }

                    if (user == null)
                    {
                        _logger.LogWarning($"User '{userRepo.Name}' not found!");
                        continue;
                    }

                    var userName = user.GetUserName();
                    if (userName != userRepo.Name)
                    {
                        var oldName = userRepo.Name;
                        userRepo.Name = userName;
                        await UserService.Update(userRepo);
                        _logger.LogInformation($"User name updated from '{oldName}' to '{userName}'");
                    }
                }
            }

            _updateUserNameCounter++;
        }

        private async Task UpdateNoWalk()
        {
            if (IsNextDay())
            {
                var userList = await UserService.GetList();
                foreach (var user in userList)
                {
                    if (user.NoWalk.HasValue && user.NoWalk.Value)
                    {
                        user.NoWalk = false;
                        await UserService.Update(user);
                        _logger.LogInformation($"User '{user.Name}' hasn't 'No walk' status yet");
                    }
                }
                _allKicked = false;
                return;
            }

            var now = DateTimeService.GetDateTimeNow();
            if (!BanDays.Contains(now.DayOfWeek))
                return;

            if (now.TimeOfDay > WalkingTime && !_allKicked)
            {
                await BanAll(true);
                _allKicked = true;
            }
        }
        #endregion

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
            _logger.LogInformation($"Recieved message from {message.From.GetUserName()} in chat '{message.Chat.Title}', chatType={message.Chat.Type}");
            if (message.Chat.Type == Core.Model.ChatType.Private)
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
                if (chat.Type == Core.Model.ChatType.Private)
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
            string commandText = chatMessage?.Text?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(commandText))
                return;

            var chat = await EnsureChatSaved(chatMessage.Chat, chatMessage.From);

            var user = chatMessage.From;
            _logger.LogInformation($"Recieved message from '{user.GetUserName()}', id={user.Id}");

            await AddMessage(chatMessage, chat);

            if (StateMachines != null && StateMachines.Any())
            {
                var executingSm = StateMachines.FirstOrDefault(sm => sm.IsExecuting(user.Id));
                if (executingSm != null)
                {
                    await ProcessStage(executingSm, chatMessage.Chat.Id, user.Id, chatMessage);
                    return;
                }
            }

            var command = Commands.GetCommand(commandText);
            if (command != null && command.IsValid)
            {
                await ProcessCommandMessage(chatMessage.Chat.Id, command, user);
            }
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
            User sender = chatMessage.From;

            if (chatMessage.LeftChatMember != null)
            {
                var botMember = await BotInstance.GetMeAsync();
                if (chatMessage.LeftChatMember.Id == botMember.Id)
                    await DeleteChat(chatMessage.Chat.Title);
                return;
            }

            if (chatMessage.NewChatMembers != null)
            {
                var users = await UserService.GetList();
                var newMembers = chatMessage.NewChatMembers.Where(c => !users.Any(u => u.Id == c.Id)).ToList();
                foreach(var chatMember in newMembers)
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
                await BotInstance.SendPhotoAsync(chatMessage.Chat.Id, "https://i.ytimg.com/vi/gpEtNGeM3zE/maxresdefault.jpg");
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
                    CommandResultType.Delete =>  await DeleteMessages(reciever, 
                            commandResult.Content.ToString()),
                    CommandResultType.ChangePassword => await UpdatePassword(reciever,
                            commandResult.Properties["ChatType"].ToString(),
                            commandResult.Properties["NewPassword"].ToString()),
                    CommandResultType.ChangeChatType => await UpdateChatType(reciever,
                            (long) commandResult.Properties["ChatId"],
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
                        await BotInstance.PinChatMessageAsync(reciever, result.MessageId);
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
            List<Core.Model.Chat> chatsToDelete = new List<ChatRepo>();
            var links = new List<Link>();
            foreach (var chat in chatList)
            {
                try
                {
                    if (chat.ChatType == Core.Model.ChatType.Private)
                        continue;

                    if (onlyPublicChates && chat.ChatType == Core.Model.ChatType.Admin)
                        continue;

                    var chatMember = await BotInstance.GetChatMemberAsync(chat.Id, user.Id);
                    if (chatMember == null || chatMember.Status == ChatMemberStatus.Kicked || chatMember.Status == ChatMemberStatus.Left)
                    {
                        if (chatMember.Status == ChatMemberStatus.Kicked)
                            await BotInstance.UnbanChatMemberAsync(chat.Id, user.Id);
                        var inviteLink = await BotInstance.ExportChatInviteLinkAsync(chat.Id);
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
                (u.LastMessageTime == null || (dtNow - u.LastMessageTime.Value).TotalDays >= PeriodInactiveUserDays));

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
            var postStateMachine = StateMachines.First(sm => sm.GetType() == typeof(PostStateMachine));
            return await postStateMachine.CreateUserStateFlow(user.Id);
        }

        private async Task<CommandResult> Poll(User user)
        {
            var pollStateMachine = StateMachines.First(sm => sm.GetType() == typeof(PollStateMachine));
            return await pollStateMachine.CreateUserStateFlow(user.Id);
        }

        private async Task<CommandResult> ClearInteractive(User user)
        {
            var postStateMachine = StateMachines.First(sm => sm.GetType() == typeof(ClearStateMachine));
            return await postStateMachine.CreateUserStateFlow(user.Id);
        }

        private async Task<CommandResult> ChangePasswordInteractive(User user)
        {
            var changePassStateMachine = StateMachines.First(sm => sm.GetType() == typeof(ChangePasswordStateMachine));
            return await changePassStateMachine.CreateUserStateFlow(user.Id);
        }

        public async Task<CommandResult> ChangeChatTypeInteractive(User user)
        {
            var changePassStateMachine = StateMachines.First(sm => sm.GetType() == typeof(ChangeChatTypeStateMachine));
            return await changePassStateMachine.CreateUserStateFlow(user.Id);
        }

        public async Task<CommandResult> UnpinMessageInteractive(User user)
        {
            var changePassStateMachine = StateMachines.First(sm => sm.GetType() == typeof(UnpinMessageStateMachine));
            return await changePassStateMachine.CreateUserStateFlow(user.Id);
        }

        public async Task<CommandResult> Ban(string userIdOrName)
        {
            UserRepo user = await UserService.GetByIdOrName(userIdOrName);

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
                    var chat = await BotInstance.GetChatAsync(chatRepo.Id);
                    if (chat == null)
                    {
                        _logger.LogInformation($"Chat '{chatRepo.Title}' not found");
                        continue;
                    }

                    ChatMember chatMember = await BotInstance.GetChatMemberAsync(chat.Id, (int)user.Id);
                    _logger.LogInformation($"For chat='{chat.Title}', user={user.Name}, status={chatMember.Status}");
                    if (chatMember.Status != ChatMemberStatus.Kicked && chatMember.Status != ChatMemberStatus.Left)
                    {
                        await BotInstance.BanChatMemberAsync(chat.Id, (int)user.Id);
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


            var users = (await UserService.GetList()).Where(u => u.Id != BotInstance.GetMe().Id);
            foreach (var user in users)
            {
                try
                {
                    await BotInstance.BanChatMemberAsync(chatRepo.Id, (int)user.Id);
                    _logger.LogInformation($"User '{user.Name}' deleted from chat '{chatRepo.Title}'");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Can't delete user '{user.Name}' from chat '{chatName}': {ex.Message}");
                }
            }

            await BotInstance.LeaveChatAsync(chatRepo.Id);
            _logger.LogInformation($"Chat {chatName}, type={chatRepo.ChatType} deleted");

            await ChatService.Delete(chatRepo.Id);
            return new CommandResult("", CommandResultType.None);
        }

        public async Task<CommandResult> DeleteUser(string userNameOrId)
        {
            UserRepo user = await UserService.GetByIdOrName(userNameOrId);
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
            catch(Exception ex)
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
                var contaner = (BackupContaner) formatter.Deserialize(fs);
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
            
            var message = await BotInstance.SendMessageAsync(chatId: chatId, text: content, 
                parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendImageMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await BotInstance.GetChatAsync(chatId);
            var message = await BotInstance.SendPhotoAsync(chatId: chat.Id, photo: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendVideoMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await BotInstance.GetChatAsync(chatId);
            var message = await BotInstance.SendVideoAsync(chatId: chat.Id, video: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendDocumentMessage(long chatId, string fileId, string caption, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(fileId))
                return null;

            var chat = await BotInstance.GetChatAsync(chatId);
            var message = await BotInstance.SendDocumentAsync(chatId: chat.Id, document: fileId, caption: caption, parseMode: ParseMode.HTML, replyMarkup: markUp);

            return message;
        }

        private async Task<Message> SendPollMessage(long chatId, string content, IEnumerable<string> cases, bool? isMultiAnswers, bool? isAnonymous, ReplyMarkup markUp = null)
        {
            if (string.IsNullOrEmpty(content))
                return null;

            var chat = await BotInstance.GetChatAsync(chatId);
            var message = await BotInstance.SendPollAsync(
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
            foreach(var link in links)
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
            return await BotInstance.SendMessageAsync(
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

            foreach(var msg in messagesToDelete)
            {
                try
                {
                    await UserMessageService.Delete(msg.Id);
                }
                catch(Exception ex)
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
                await BotInstance.UnPinChatMessageAsync(unpinChatId);
                pinnedMessages.Remove(msg);

                foreach (var pinnedMsg in pinnedMessages)
                {
                    await BotInstance.PinChatMessageAsync(unpinChatId, pinnedMsg.TelegramId);
                }

                message = await BotInstance.SendMessageAsync(
                    new SendMessageArgs(chatId, "Messages unpinned")
                    {
                        ParseMode = ParseMode.HTML
                    });
            }
            catch(Exception ex)
            {
                _logger.LogWarning($"Cannot unpin messages from chat '{chatToChange.Title}': {ex.Message}");
                message = await SendTextMessage(chatId, $"Something got wrong");
            }
            
            return message;
        }

        private async Task AddGroup(Chat chat)
        {
            var chatRepo = new ChatRepo
            {
                Id = chat.Id,
                Title = chat.Title,
                ChatType = Core.Model.ChatType.Public
            };
            await ChatService.Add(chatRepo);
        }

        private async Task AddMessage(Message message, ChatRepo chat = null)
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

        private async Task<ChatRepo> EnsureChatSaved(Chat chat, User user = null)
        {
            var chatRepo = await ChatService.Get(chat.Id);
            if (chatRepo == null)
            {
                chatRepo = new Core.Model.Chat
                {
                    Id = chat.Id,
                    Title = chat.Title,
                    ChatType = chat.Type == BotAPI.AvailableTypes.ChatType.Private ? Core.Model.ChatType.Private : Core.Model.ChatType.Public
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
                userRepo = new UserRepo
                {
                    Id = user.Id,
                    Name = user.GetUserName(),
                    Type = chatType == Core.Model.ChatType.Admin ? UserType.Admin : UserType.Member
                };
                userRepo.IsAdmin = userRepo.Type == Core.Model.UserType.Admin;
                await UserService.Add(userRepo);
                _adminResetCounters.TryAdd(userRepo.Id, 0);
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

        private bool IsNextDay()
        {
            var ts = new TimeSpan(0, 0, PeriodEchoSec);
            var now = DateTimeService.GetDateTimeUTCNow();
            var currentDay = now.Day;
            var prevDay = now.Subtract(ts).Day;
            return currentDay != prevDay;
        }
    }
}
