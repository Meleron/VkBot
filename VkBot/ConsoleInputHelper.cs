using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using VkNet.Model;

namespace VkBot
{
    static class ConsoleInputHelper
    {
        private static readonly VkService vkService = VkService.GetInstance();
        public static string EnterPassword(bool printSymbols = false)
        {
            string pass = "";
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    if (printSymbols)
                        Console.Write('*');
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        if (printSymbols == true)
                            Console.Write("\b \b");

                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            return pass;
        }

        public static int EnterIntInRange(int min, int max)
        {
            int toReturn;
            while (!int.TryParse(Console.ReadLine(), out toReturn) || !(toReturn >= min && toReturn <= max))
            {
                Console.Write("Wrong input, repeat one more time: ");
            }
            return toReturn;
        }

        public static long EnterLongInRange(long min, long max)
        {
            long toReturn;
            while (!long.TryParse(Console.ReadLine(), out toReturn) || !(toReturn >= min && toReturn <= max))
            {
                Console.Write("Wrong input, repeat one more time: ");
            }
            return toReturn;
        }

        public static string EnterStringInRange(int minLength, int maxLength)
        {
            string toReturn = "";
            while (!(toReturn.Length >= minLength && toReturn.Length <= maxLength)/* && toReturn.Any<char>(c => char.IsDigit(c))*/)
            {
                Console.Write($"Enter user's id ({minLength} <= length <= {maxLength}): ");
                toReturn = Console.ReadLine();
            }
            return toReturn;
        }

        public static string EnterMessage()
        {
            string message = "";
            while (message.Length < 1 || message.Length > 1000)
            {
                Console.WriteLine("\nEnter message (1-1000 symbols). Enter \\" + "n" + " for new line:\n");
                message = Console.ReadLine();
            }
            return message.Replace("\\" + "n", Environment.NewLine);
        }

        public static long EnterUserId()
        {
            Console.Clear();
            //Console.Write("Enter destination user's id: ");
            string user = EnterStringInRange(1, 20);
            long userId = 0;
            if (user.Any<char>(c => char.IsDigit(c)))
                long.TryParse(user, out userId);
            else
                try { userId = (long)vkService.GetUserIdByScreenName(user); }
                catch (Exception ex)
                {
                    Console.WriteLine("Error getting user's id");
                    ConsoleOutputHelper.PrintException(ex);
                    ConsoleOutputHelper.PressKeyToContinueMsg();
                    return 0;
                }
            //userId = EnterIntInRange(1, int.MaxValue);
            User userToSend = VkService.GetApi().Users.Get(new long[] { userId }).FirstOrDefault();
            if (userToSend == null)
            {
                Console.WriteLine("User not found");
                return 0;
            }
            else
            {
                Console.WriteLine($"You are going to send message to \"{userToSend.FirstName} {userToSend.LastName}\". Are you sure?");
                Console.WriteLine("1 - Yes\n2 - No");
                if (EnterIntInRange(1, 2) == 2)
                    return 0;
            }
            return userId;
        }

        private static string EnterTimeString()
        {
            string time = "...";
            while (time.Length != 6 || !time.Any(c => char.IsDigit(c)))
            {
                Console.WriteLine("Enter time (format: mmhhss (example: 083412), all chars are digits)\n");
                time = Console.ReadLine();
                try
                {
                    ConvertStringToTime(time);
                }
                catch (Exception)
                {
                    time = "...";
                    continue;
                }
            }
            Console.Clear();
            return time;
        }
        private static string EnterDateString()
        {
            string date = "...";
            while (date.Length != 8 || !date.Any(c => char.IsDigit(c)))
            {
                if (date.Equals(""))
                    return DateTime.UtcNow.ToString("ddMMyyyy");
                Console.WriteLine("Enter date (format: ddmmyyyy (example: 15042020), all chars are digits)\n");
                date = Console.ReadLine();
                try
                {
                    ConvertStringToDate(date);
                }
                catch (Exception)
                {
                    date = "...";
                    continue;
                }
            }
            Console.Clear();
            return date;
        }

        private static DateTime ConvertStringToDate(string date)
        {
            return DateTime.ParseExact(date, "ddMMyyyy", CultureInfo.InvariantCulture);
        }

        private static DateTime ConvertStringToTime(string time)
        {
            return DateTime.ParseExact(time, "Hmmss", null, System.Globalization.DateTimeStyles.None); ;
        }

        private static DateTime GetDayTime(string date, string time)
        {
            return DateTime.ParseExact(date + time, "ddMMyyyyHmmss", CultureInfo.CurrentCulture);
        }

        private static DateTime EnterValidDate()
        {
            Console.Clear();
            DateTime currentDate = DateTime.Now;
            DateTime dateToSend = currentDate.AddDays(-1);
            while (dateToSend < currentDate)
            {
                dateToSend = GetDayTime(EnterDateString(), EnterTimeString());
                currentDate = DateTime.Now;
                if (dateToSend < currentDate)
                    Colorful.Console.WriteLine("You are not DeLorean driver (you can't send message to the past)", Color.Red);

            }
            return dateToSend;
        }

        public static int GetDelayValue()
        {
            Console.Clear();
            Console.WriteLine("Time settings");
            Console.WriteLine("1 - Send message right now");
            Console.WriteLine("2 - Send message with delay");
            switch (ConsoleInputHelper.EnterIntInRange(1, 2))
            {
                case 2:
                    {
                        Console.Clear();
                        Console.WriteLine("How do you want to enter delay?");
                        Console.WriteLine("1 - Enter amount of seconds");
                        Console.WriteLine("2 - Enter date and time");
                        switch (EnterIntInRange(1, 2))
                        {
                            case 2:
                                {
                                    DateTime dateToSend = EnterValidDate();
                                    return (int)Math.Ceiling((dateToSend - DateTime.Now).TotalSeconds);
                                }
                            default:
                                {
                                    Console.Write("\nEnter delay in seconds (1 - 604800): ");
                                    return ConsoleInputHelper.EnterIntInRange(1, 604800);
                                }
                        }
                    }
                default: { return 0; }
            }
        }
    }
}
