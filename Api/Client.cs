using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TLSchema;
using TLSharp;

namespace Telegram.Altayskaya97.Api
{
    public class Client
    {
        private static readonly string _apiIdKey = "api_id";
        private static readonly string _apiHashKey = "api_hash";
        private static readonly string _telephone = "+375336983461";
        private static string _code = "37078";
        private static readonly string _password = "37078";

        private TelegramClient _telegramClient;
        private TLUser _user;

        public Client()
        {
            _telegramClient = CreateClient();
            //var hash = await _telegramClient.SendCodeRequestAsync("<user_number>");
            //var code = "<code_from_telegram>"; // you can change code in debugger

            // var user = await _telegramClient.MakeAuthAsync("<user_number>", hash, code);

        }

        private TelegramClient CreateClient()
        {
            try
            {
                int apiId = Int32.Parse(Environment.GetEnvironmentVariable(_apiIdKey));
                string apiHash = Environment.GetEnvironmentVariable(_apiHashKey);
                var client = new TelegramClient(apiId, apiHash);
                return new TelegramClient(apiId, apiHash);
            }
            catch (MissingApiConfigurationException ex)
            {
                throw new Exception($"Please add your API settings (api_id, api_hash) to environment(More info: {MissingApiConfigurationException.InfoUrl})", ex);
            }
        }

        public async Task SendMessage(int userId, string htmlMessage)
        {
            if (!_telegramClient.IsUserAuthorized())
                await Authorize();

            if (!_telegramClient.IsUserAuthorized())
                return;

            try
            {
                await _telegramClient.SendMessageAsync(new TLInputPeerUser() { UserId = _user.Id }, htmlMessage);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            /*

            bool isAuthorized = _telegramClient.IsUserAuthorized();
            if (!isAuthorized)
                isAuthorized = await Authorize();
            if (isAuthorized)
                await _telegramClient.SendMessageAsync(new TLInputPeerUser() { UserId = userId }, htmlMessage);*/
        }

        public async Task<bool> Authorize()
        {
            if (_telegramClient == null)
                _telegramClient = CreateClient();

            await _telegramClient.ConnectAsync();

            var hash = await _telegramClient.SendCodeRequestAsync(_telephone);
            
            try
            {
                _user = await _telegramClient.MakeAuthAsync(_telephone, hash, _code);
                return true;
            }
            catch (CloudPasswordNeededException ex)
            {
                var passwordSetting = await _telegramClient.GetPasswordSetting();
                _user = await _telegramClient.MakeAuthWithPasswordAsync(passwordSetting, _password);
                return true;
            }
            catch (InvalidPhoneCodeException ex)
            {
                throw new Exception("CodeToAuthenticate is wrong, fill it with the code you just got now by SMS/Telegram", ex);
            }
        }
    }
}
