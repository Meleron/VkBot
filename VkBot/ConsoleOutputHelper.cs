using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VkNet.Model;
using VkNet.Utils;

namespace VkBot
{
    static class ConsoleOutputHelper
    {
        private static readonly VkService vkService = VkService.GetInstance();

        public static void PressKeyToContinueMsg()
        {
            Console.WriteLine("\nPress any key to continue...");
            Console.ReadKey();
        }

        public static void PrintFriendsList()
        {
            Console.Clear();
            Console.WriteLine("Getting friens list...");
            VkCollection<User> friends = vkService.GetFriendsList();
            Console.WriteLine("Friends count: " + friends.Count);
            int counter = 0;
            foreach (var friend in friends)
            {
                Console.WriteLine($"[{counter}/{friends.Count - 1}] id: |{friend.Id}| name: |{friend.FirstName}| last:|{friend.LastName}|");
                counter++;
            }
        }

        public static void PrintException(Exception ex)
        {
            Colorful.Console.WriteLine("\n\n***↓exception message below↓***", Color.Red);
            Colorful.Console.WriteLine(ex.Message, Color.Red);
            Colorful.Console.WriteLine("***end exception message***", Color.Red);
        }
    }
}
