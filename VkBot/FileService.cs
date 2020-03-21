using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkBot
{
    class FileService
    {
        private static FileService instance = null;
        private static readonly object syncRoot = new object();
        private const string FILEPATH = "..\\..\\..\\..\\token.txt";

        private FileService() { }

        public static FileService GetInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new FileService();
                }
            }
            return instance;
        }

        public void SaveToFile(string input)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(FILEPATH))
                {
                    sw.Write(input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to file...");
                ConsoleOutputHelper.PrintException(ex);
            }
        }

        public string ReadFromFile()
        {
            try
            {
                using (StreamReader sr = new StreamReader(FILEPATH))
                {
                    return sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading file...");
                ConsoleOutputHelper.PrintException(ex);
                return "";
            }
        }
    }
}
