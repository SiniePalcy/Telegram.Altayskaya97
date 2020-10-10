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

namespace Telegram.Altayskaya97.Bot
{
    public class Bot : BackgroundService
    {
#if DEBUG
        private static readonly string _accessTokenKeyName = "altayskaya97_test_bot";
#else
        private static readonly string _accessTokenKeyName = "altayski_bot";
#endif

        private readonly ILogger<Bot> _logger;
        //private readonly IConfiguration _configuration;
        private readonly ITelegramBotClient _botClient;

        private int PeriodEchoSec { get; }
        private int PeriodResetAccessMin { get; }
        private int PeriodChatListMin { get; }
        
        private ConcurrentDictionary<long, int> _adminResetCounters = new ConcurrentDictionary<long, int>();
        private volatile int _chatListCounter = 0;

        #region Constant
        private const string INCORRECT_COMMAND = "Неверная команда";
        private const int PERIOD_ECHO_SEC_DEFAULT = 20;
        private const int PERIOD_RESET_ACCESS_MIN_DEFAULT = 60;
        private const int PERIOD_CHAT_LIST_MIN_DEFAULT = 180;
        #endregion

        #region Services
        private readonly IWelcomeService _welcomeService;
        private readonly IMenuService _menuService;
        private readonly IUserService _userService;
        private readonly IChatService _chatService;
        #endregion

        public Bot(ILogger<Bot> logger, IConfiguration configuration,
            IWelcomeService welcomeService,
            IMenuService menuService,
            IUserService userService,
            IChatService chatService)
        {
            _logger = logger;

            this._welcomeService = welcomeService;
            this._menuService = menuService;
            this._userService = userService;
            this._chatService = chatService;

            var configSection = configuration.GetSection("Configuration");
            PeriodEchoSec = ParseInt(configSection.GetSection("PeriodEchoSec").Value, PERIOD_ECHO_SEC_DEFAULT);
            PeriodResetAccessMin = ParseInt(configSection.GetSection("PeriodResetAccessMin").Value, PERIOD_RESET_ACCESS_MIN_DEFAULT);
            PeriodChatListMin = ParseInt(configSection.GetSection("PeriodChatListMin").Value, PERIOD_CHAT_LIST_MIN_DEFAULT);

            //var sec = configuration.GetSection("Tokens").GetSection(_accessTokenKeyName);
            string accessToken = Environment.GetEnvironmentVariable(_accessTokenKeyName);
            _botClient = new TelegramBotClient(accessToken);

            _botClient.OnMessage += Bot_OnMessage;
            _botClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            _botClient.OnInlineQuery += BotClient_OnInlineQuery;
            _botClient.StartReceiving();

            Init();
        }

        private async void Init()
        {
            var chatList = await _chatService.GetChatList();
            if (!chatList.Any())
                return;

            var adminChats = chatList.Where(c => c.ChatType == Core.Model.ChatType.Admin);
            List<ChatMember> admins = new List<ChatMember>();
            foreach (var adminChat in adminChats)
            {
                var adminsOfChat = await _botClient.GetChatAdministratorsAsync(adminChat.Id);
                admins.AddRange(adminsOfChat.Where(usr => !usr.User.IsBot));
            }

            foreach (var admin in admins)
            {
                _adminResetCounters[admin.User.Id] = 0;
                var userInRepo = await _userService.GetUser(admin.User.Id);
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
                };
                await _userService.AddUser(newUser);
                _logger.LogInformation($"User saved with id={newUser.Id}, name={newUser.Name}, isAdmin={newUser.IsAdmin}");
            }
        }

        private int ParseInt(string source, int defaultValue)
        {
            if (string.IsNullOrEmpty(source) || !int.TryParse(source, out int result))
                result = defaultValue;

            return result;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTimeOffset.Now;
                await UpdateUsersAccess();
                //await UpdateChatList();
                _logger.LogInformation($"[Echo] Bot running at: {now}");
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
                    await _userService.RestrictUser(userId);
                }
                _adminResetCounters.TryUpdate(userId, counterValue + 1, counterValue);
            }
        }

        private async Task UpdateChatList()
        {
            if (_chatListCounter == PeriodChatListMin * 60 / PeriodEchoSec)
                _chatListCounter = 0;

            var chatList = await _chatService.GetChatList();
            foreach (var chatRepo in chatList)
            {
                var chat = await _botClient.GetChatAsync(chatRepo.Id);

                int chatMembers = 0;
                try
                {
                    chatMembers = await _botClient.GetChatMembersCountAsync(chat.Id);
                }
                catch (Exception ex)
                {
                    await _chatService.DeleteChat(chat.Id);
                }
                if (chat == null  || chatMembers <= 1)
                {
                    await _chatService.DeleteChat(chatRepo.Id);
                }
            }
            
            _chatListCounter++;
        }

        #region Event handlers
        private async void Bot_OnMessage(object sender, MessageEventArgs e)
        {
            Message chatMessage = e.Message;

            if (chatMessage.Chat.Type == ChatType.Private)
                await ProcessBotMessage(chatMessage);
            else
                await ProcessChatMessage(chatMessage);

        }

        private async void BotClient_OnInlineQuery(object sender, InlineQueryEventArgs e)
        {
            await _botClient.SendTextMessageAsync(
                chatId: e.InlineQuery.From.Id,
                text: e.InlineQuery.Query);
        }

        private async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            await _botClient.SendTextMessageAsync(
                chatId: e.CallbackQuery.From.Id,
                text: e.CallbackQuery.Data);
        }
        #endregion

        private async Task ProcessChatMessage(Message chatMessage)
        {
            Chat chat = chatMessage.Chat;
            if (chatMessage.Type == MessageType.GroupCreated || chatMessage.Type == MessageType.SupergroupCreated)
            {
                await AddGroup(chat);
                return;
            }

            if (chat.Type != ChatType.Private)
                await EnsureChatSaved(chat);

            var chatRepo = await _chatService.GetChat(chat.Id);
            User sender = chatMessage.From;
            if (chatRepo.ChatType == Telegram.Altayskaya97.Core.Model.ChatType.Admin)
                await EnsureUserSaved(sender);

            if (chatMessage.Type == MessageType.ChatMembersAdded)
            {
                var users = await _userService.AllUsers();
                var newMembers = chatMessage.NewChatMembers.Where(c => !users.Any(u => u.Id == c.Id)).ToList();
                newMembers.ForEach(async chatMember => await SendWelcomeGroupMessage(chatMessage.Chat, chatMember.GetUserName()));
                return;
            }

            if (string.IsNullOrEmpty(chatMessage.Text))
                return;

            string message = chatMessage.Text.Trim().ToLower();
            
            string userName = sender.GetUserName();
            _logger.LogInformation($"Recieved message from {userName} in chat id={chat.Id}, title={chat.Title}, type={chat.Type}");

            if (message == Commands.Help.Name)
                await SendWelcomeGroupMessage(chatMessage.Chat, userName, chatMessage.MessageId);
            else if (message == Commands.Helb.Name)
                await _botClient.SendPhotoAsync(chatMessage.Chat, "https://i.ytimg.com/vi/gpEtNGeM3zE/maxresdefault.jpg");

            return;
        }

        private async Task AddGroup(Chat chat)
        {
            var chatRepo = new ChatRepo
            {
                Id = chat.Id,
                Title = chat.Title,
                ChatType = chat.Type == ChatType.Supergroup ? Core.Model.ChatType.Admin : Core.Model.ChatType.Public
            };
            await _chatService.AddChat(chatRepo);
        }

        private async Task EnsureChatSaved(Chat chat)
        {
            var chatRepo = await _chatService.GetChat(chat.Id);
            if (chatRepo == null)
            {
                var dbChat = new Core.Model.Chat
                {
                    Id = chat.Id,
                    Title = chat.Title,
                    ChatType = chat.Type == ChatType.Supergroup ? Core.Model.ChatType.Admin : Core.Model.ChatType.Public
                };
                await _chatService.AddChat(dbChat);
            }
        }

        private async Task EnsureUserSaved(User user)
        {
            var userRepo = await _userService.GetUser(user.Id);
            if (userRepo == null)
            {
                var dbUser = new UserRepo
                {
                    Id = user.Id,
                    Name = user.GetUserName(),
                    IsAdmin = true
                };
                await _userService.AddUser(dbUser);
            }
        }

        private async Task ProcessBotMessage(Message chatMessage)
        {
            string commandText = chatMessage?.Text?.Trim()?.ToLower();
            if (string.IsNullOrEmpty(commandText))
                return;

            var command = Commands.GetCommand(commandText);
            if (command == null || !command.IsValid)
                return;

            var user = chatMessage.From;

            CommandResult commandResult;
            var userRepo = await _userService.GetUser(user.Id);
            if (userRepo == null)
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) :
                                    new CommandResult(INCORRECT_COMMAND);
            }
            else if (command.IsAdmin && userRepo.IsAdmin)
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) : 
                                command.Name == Commands.Sobachku.Name ? await Sobachku(user) :
                                command.Name == Commands.ChatList.Name ? await ChatList() :
                                command.Name == Commands.UserList.Name ? await UserList() :
                                command.Name == Commands.Ban.Name ? await Ban(command) :
                                command.Name == Commands.BanAll.Name ? await BanAll() :
                                    new CommandResult(INCORRECT_COMMAND, CommandResultType.Message);
            }
            else
            {
                commandResult = command.Name == Commands.Start.Name ? await Start(user) :
                                command.Name == Commands.Sobachku.Name ? await Sobachku(user) :
                                    new CommandResult(INCORRECT_COMMAND);
            }

            var recievers = commandResult.Recievers ?? new List<long>() { chatMessage.Chat.Id };

            foreach (var reciever in recievers)
            {
                if (commandResult.Type == CommandResultType.Message)
                    await _botClient.SendTextMessageAsync(chatId: reciever, text: commandResult.Content, parseMode: ParseMode.Html, replyMarkup: commandResult.ReplyMarkup);
                else if (commandResult.Type == CommandResultType.Links)
                {
                    foreach (var link in commandResult.Links)
                        await _botClient.SendTextMessageAsync(reciever, $"{link.Description}{Environment.NewLine}{link.Url}", ParseMode.Html);
                }
            }
        }

        private async Task<CommandResult> Start(User user)
        {
            bool isAdmin = await _userService.IsAdmin(user.Id);
            return new CommandResult(_menuService.GetMenu(user.Username, isAdmin), CommandResultType.Message, new InlineKeyboardMarkup(_welcomeService.GetWelcomeButtons()));
        }

        private async Task<CommandResult> Sobachku(User user)
        {
            await _userService.PromoteUserAdmin(user.Id);

            bool isBlocked = await _userService.IsBlocked(user.Id);
            if (!isBlocked)
                return new CommandResult("Рад тебя видеть :)", CommandResultType.Message);

            await _userService.UnbanUser(user.Id);

            var result = new CommandResult("", CommandResultType.Links);

            var chatList = await _chatService.GetChatList();
            foreach(var chat in chatList)
            {
                var chatMember = await _botClient.GetChatMemberAsync(chat.Id, user.Id);
                 if (chatMember == null || chatMember.Status == ChatMemberStatus.Kicked || chatMember.Status == ChatMemberStatus.Left)
                {
                    await _botClient.UnbanChatMemberAsync(chat.Id, user.Id);
                    var inviteLink = await _botClient.ExportChatInviteLinkAsync(chat.Id);
                    result.Links.Add(new Link
                    {
                        Url = inviteLink,
                        Description = $"Чат <b>{chat.Title}</b>"
                    }); 
                }
            }
            
            return result;
        }

        private async Task<CommandResult> ChatList()
        {
            var chatList = await _chatService.GetChatList();

            StringBuilder sb = new StringBuilder("<code>");
            foreach (var chat in chatList)
            {
                sb.AppendLine($"id: {chat.Id,-20}title: <b>{chat.Title}</b>");
            }
            sb.Append("</code>");

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        private async Task<CommandResult> UserList()
        {
            var userList = await _userService.AllUsers();

            StringBuilder sb = new StringBuilder(string.Format($"<code>{"Username",-20}{"Admin",-6}{"Blocked",-7}\n"));
            foreach (var user in userList)
            {
                var adminSign = user.IsAdmin ? "  +" : "  -";
                var blockedSign = user.IsBlocked ? "  +" : "  -";
                sb.AppendLine($"{user.Name,-20}{adminSign,-6}{blockedSign,-7}");
            }
            sb.Append("</code>");

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        private async Task<CommandResult> Post(Command command)
        {
            ICollection<ChatRepo> chatsToPost = null;

            var firstWord = command.GetFirstWord().ToLower();
            if (firstWord == "all")
            {
                chatsToPost = await _chatService.GetChatList();
            }
            else
            {
                var chatId = command.GetFirstNumber();
                if (chatId != null)
                {
                    var chat = await _chatService.GetChat(chatId.Value);
                    if (chat != null)
                    {
                        chatsToPost = new List<ChatRepo>() { chat };
                    }
                }
            }

            if (chatsToPost == null || !chatsToPost.Any())
                return new CommandResult("Проверьте команду", CommandResultType.Message);

            string contentToPost = command.Text.Replace(command.Name, "").Replace(firstWord, "").Trim();
            
            var commandResult = new CommandResult(contentToPost, CommandResultType.Message);
            commandResult.Recievers = chatsToPost.Select(c => c.Id).ToList();

            return commandResult;
        }

        private async Task<CommandResult> Ban(Command command)
        {
            var commandContent = command.Text.Replace(command.Name,"").Trim().ToLower();
            if (string.IsNullOrEmpty(commandContent))
                return new CommandResult("Проверьте команду", CommandResultType.Message);

            var users = await _userService.AllUsers();
            var user = users.FirstOrDefault(u => commandContent.Contains(u.Name.ToLower()));
            if (user == null)
                return new CommandResult("Пользователь не найден", CommandResultType.Message);

            if (user.IsBlocked)
                return new CommandResult("Пользователь уже заблокирован", CommandResultType.Message);

            if (user.IsCoordinator)
                return new CommandResult("Координаторов не баним", CommandResultType.None);

            var chats = await _chatService.GetChatList();
            if (!chats.Any())
                return new CommandResult("Список чатов пуст", CommandResultType.Message);

            StringBuilder buffer = new StringBuilder();
            foreach (var chatRepo in chats)
            {
                var chat = await _botClient.GetChatAsync(chatRepo.Id);
                if (chat == null)
                {
                    _logger.LogInformation($"Chat {chatRepo.Title} not found");
                    continue;
                }

                try
                {
                    await _botClient.KickChatMemberAsync(chat.Id, (int)user.Id);
                    await _userService.BanUser(user.Id);
                    buffer.AppendLine($"Пользователь <b>{user.Name}</b> удален из чата <b>{chatRepo.Title}</b>");
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                {
                    _logger.LogInformation(ex.Message);
                }
            }

            return new CommandResult(buffer.ToString(), CommandResultType.Message);
        }

        private async Task<CommandResult> BanAll()
        {
            var users = await _userService.AllUsers();
            var chats = await _chatService.GetChatList();

            StringBuilder sb = new StringBuilder();
            foreach (var user in users)
            {
                if (user.IsBlocked)
                {
                    sb.AppendLine($"Пользователь <b>{user.Name}</b> уже заблокирован");
                    continue;
                }

                if (user.IsCoordinator)
                    continue;

                foreach (var chatRepo in chats)
                {
                    var chat = await _botClient.GetChatAsync(chatRepo.Id);
                    if (chat == null)
                        continue;

                    try
                    {
                        await _botClient.KickChatMemberAsync(chat.Id, (int)user.Id);
                        await _userService.BanUser(user.Id);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ex)
                    {
                        _logger.LogInformation(ex.Message);
                    }

                    sb.AppendLine($"Пользователь <b>{user.Name}</b> удален из чата <b>{chatRepo.Title}</b>");
                }
            }

            return new CommandResult(sb.ToString(), CommandResultType.Message);
        }

        private async Task<Message> SendWelcomeGroupMessage(Telegram.Bot.Types.Chat chat, string userName, int messageId = 0)
        {
            return await _botClient.SendTextMessageAsync(
                    chatId: chat.Id,
                    text: _welcomeService.GetWelcomeMessage(userName),
                    parseMode: ParseMode.Html,
                    replyToMessageId: messageId,
                    replyMarkup: new InlineKeyboardMarkup(_welcomeService.GetWelcomeButtons())
            );
        }
    }
}
