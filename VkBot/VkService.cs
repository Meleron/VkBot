using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Model;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkBot
{
    class VkService
    {
        static ServiceCollection services = new ServiceCollection();
        private static IVkApi api { get; set; }
        private static VkService instance = null;
        private static readonly object syncRoot = new object();
        private VkService()
        {
            services.AddAudioBypass();
            api = new VkApi(services);
        }

        public static VkService GetInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new VkService();
                }
            }
            return instance;
        }

        public static VkApi GetApi()
        {
            if (api == null)
                new VkService();
            return api as VkApi;
        }

        public LoginResult LogIn(string login, string password)
        {
            try
            {
                api.Authorize(new VkNet.Model.ApiAuthParams
                {
                    Login = login,
                    Password = password,
                    TwoFactorAuthorization = () =>
                    {
                        Console.WriteLine("Auth code: ");
                        return Console.ReadLine();
                    }
                });
                return new LoginResult { isSucceed = true, exception = null };
            }
            catch (Exception ex)
            {
                return new LoginResult { isSucceed = false, exception = ex };
            }
        }

        public long GetUserIdByScreenName(string screenName)
        {
            return api.Users.Get(new List<string> { screenName }).FirstOrDefault().Id;
        }

        public string GetToken()
        {
            return api.Token;
        }

        public void LogOut()
        {
            api.LogOut();
        }

        public Thread SendMessage(MessagesSendParams message, int delay = 0)
        {
            Thread thread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                Thread.Sleep(new TimeSpan(0, 0, delay));
                try { api.Messages.Send(message); }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending message");
                    ConsoleOutputHelper.PrintException(ex);
                    ConsoleOutputHelper.PressKeyToContinueMsg();
                    return;
                }
                Console.WriteLine($"Messaage {message.RandomId} successfully sended");
            });
            thread.Start();
            return thread;
        }

        public VkCollection<User> GetFriendsList()
        {
            return api.Friends.Get(new FriendsGetParams { UserId = api.UserId, Fields = ProfileFields.All, Order = FriendsOrder.Name });
        }
    }
}
