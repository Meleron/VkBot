using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Threading;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Enums.Filters;
using VkNet.Enums.SafetyEnums;
using VkNet.Exception;
using VkNet.Model;
using VkNet.Model.Attachments;
using VkNet.Model.RequestParams;
using VkNet.Utils;

namespace VkBot
{
    class Program
    {
        private static string savedLogin = "";
        private static string savedPassword = "";
        public static Thread threadChecker;
        private static bool shutDownPc = false;
        private static readonly VkService vkService = VkService.GetInstance();
        private static readonly FileService fileSerive = FileService.GetInstance();
        public static List<Thread> activeThreads = new List<Thread>();
        public static bool IsAuthed { get; private set; }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            Console.InputEncoding = Encoding.Unicode;
            try { MenuLoop(); } catch (Exception ex) { ConsoleOutputHelper.PrintException(ex); }
        }

        public static void MenuLoop()
        {
            bool toQuit = false;
            bool toClear = true;
            while (!toQuit)
            {
                if (toClear)
                    Console.Clear();
                else
                {
                    ConsoleOutputHelper.PressKeyToContinueMsg();
                    toClear = true;
                }
                Console.WriteLine("---Vk Delayed Message Sender Menu---");
                if (IsAuthed)
                {
                    Console.WriteLine("1 - Get friends list");
                    Console.WriteLine("2 - Send message");
                    Console.WriteLine("3 - Log out");
                    Console.Write("4 - Shut down PC after sending all messages");
                    if (shutDownPc)
                        Colorful.Console.WriteLine(" (Active)", Color.Green);
                    else
                        Colorful.Console.WriteLine(" (Inactive)", Color.Red);
                    switch (ConsoleInputHelper.EnterIntInRange(1, 4))
                    {
                        case 1:
                            {
                                ConsoleOutputHelper.PrintFriendsList();
                                ConsoleOutputHelper.PressKeyToContinueMsg();
                                break;
                            }
                        case 2:
                            {
                                long userId = ConsoleInputHelper.EnterUserId();
                                if (userId == 0)
                                    continue;
                                SendMessage(userId, ConsoleInputHelper.EnterMessage());
                                break;
                            }
                        case 3:
                            {
                                vkService.LogOut();
                                IsAuthed = false;
                                break;
                            }
                        case 4:
                            {
                                if (shutDownPc)
                                    TurnOffShutDownPcOnAllThreadsFinish();
                                else
                                    TurnOnShutDownPcOnAllThreadsFinish();
                                break;
                            }
                    }
                }
                else
                {
                    Console.WriteLine("1 - Authorize");
                    Console.WriteLine("2 - Quit");
                    switch (ConsoleInputHelper.EnterIntInRange(1, 2))
                    {
                        case 1: { Console.Clear(); Authorize(); break; }
                        case 2: { Console.Clear(); toQuit = true; break; }
                    }
                }
            }
        }

        public static void SendMessage(long userId, string message, IEnumerable<MediaAttachment> attachments = null)
        {
            Colorful.Console.WriteLine("\nMessage sending procedure started...", Color.Yellow);
            if (!IsAuthed)
            {
                Colorful.Console.WriteLine("\nUser not authorized, aborting...", Color.Red);
                ConsoleOutputHelper.PressKeyToContinueMsg();
                return;
            }
            MessagesSendParams messageToSend = new MessagesSendParams
            {
                UserId = userId,
                RandomId = new Random().Next(),
                Message = message,
                Attachments = attachments
            };
            int delay = ConsoleInputHelper.GetDelayValue();
            DateTime timeToSend = DateTime.Now.AddSeconds(delay);
            Colorful.Console.WriteLine($"Message {messageToSend.RandomId} will be sent in {delay} second(s) (~{timeToSend}). Please keep the program running...", Color.Yellow);
            try
            {
                activeThreads.Add(vkService.SendMessage(messageToSend, delay));
                ConsoleOutputHelper.PressKeyToContinueMsg();
            }
            catch (VkApiException ex)
            {
                Colorful.Console.WriteLine("Error sending message", Color.Red);
                ConsoleOutputHelper.PrintException(ex);
                ReLoginAndSend(messageToSend);
            }
        }


        public static void Authorize()
        {
            Console.Write("Enter login:");
            string login = Console.ReadLine();
            savedLogin = login;
            Console.Write("\nEnter password:");
            string password = ConsoleInputHelper.EnterPassword(false);
            savedPassword = password;
            Console.WriteLine();
            Colorful.Console.Write("\nAuthorizing...", Color.Yellow);
            LoginResult loginStatus = vkService.LogIn(login, password);
            IsAuthed = loginStatus.isSucceed;
            if (!IsAuthed)
            {
                ConsoleOutputHelper.PrintException(loginStatus.exception);
                Colorful.Console.WriteLine("\nFailed...", Color.Red);
            }
            else Colorful.Console.WriteLine(" Succeed!", Color.Green);
            //fileSerive.SaveToFile(vkService.GetToken());
            ConsoleOutputHelper.PressKeyToContinueMsg();
        }

        public static void ReLoginAndSend(MessagesSendParams message)
        {
            Colorful.Console.WriteLine("Trying to relogin and send message again...", Color.Yellow);
            vkService.LogIn(savedLogin, savedPassword);
            vkService.SendMessage(message);
        }

        public static void TurnOnShutDownPcOnAllThreadsFinish()
        {
            //ShutDownPcOnAllThreadsFinish();
            shutDownPc = true;
            threadChecker = GetThreadCheckingThread();
            threadChecker.Start();
        }

        public static void TurnOffShutDownPcOnAllThreadsFinish()
        {
            shutDownPc = false;
            try { threadChecker.Abort(); }
            catch (Exception ex) { Colorful.Console.WriteLine("Error aborting thread...", Color.Red); ConsoleOutputHelper.PrintException(ex); }
        }

        //public static void ShutDownPcOnAllThreadsFinish()
        //{
        //}

        public static Thread GetThreadCheckingThread()
        {
            return new Thread(() =>
            {
                foreach (Thread thread in activeThreads)
                {
                    if (!shutDownPc)
                    {
                        //Console.WriteLine("Your pc won't shut down after sending all messages.");
                        //ConsoleOutputHelper.PressKeyToContinueMsg();
                        return;
                    }
                    thread.Join();
                    Thread.Sleep(500);
                }
                if (shutDownPc)
                {
                    Colorful.Console.WriteLine("All messages sent. Shutting down pc in 5sec...", Color.Yellow);
                    Thread.Sleep(5000);
                    //Process.Start("shutdown", "/s /t 0");
                    Environment.Exit(0);
                }
            });
        }

    }
}
