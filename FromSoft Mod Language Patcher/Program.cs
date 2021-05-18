using System;
using System.IO;
using System.Reflection;

namespace FromSoft_Mod_Language_Patcher
{
    class Program
    {

        public static bool restoreBackups = false;

        static void Main(string[] args)
        {
            //Get source Directory and Language
            string sourceLangDir = Directory.GetCurrentDirectory();
            string sourceLang = new DirectoryInfo(sourceLangDir).Name;
            StartUp(sourceLang);

            //Ask if the user would like to actually patch
            if (Confirm("Would you like to patch the other language files?"))
            {
                //Ask if user would like to restore backups
                if (Confirm("Would you like to restore backups, first?"))
                {
                    restoreBackups = true;
                    Patcher.Patch(sourceLangDir, sourceLang);
                }
                else
                {
                    restoreBackups = false;
                    Patcher.Patch(sourceLangDir, sourceLang);
                }
            }
            else
            {
                Console.WriteLine("Closing without patching...");
                Console.WriteLine("Press ENTER...");
            }
            Console.WriteLine("Press ENTER...");
            Console.ReadLine();
        }

        public static void StartUp(string sourceLang)
        {
            //Get version number
            Assembly assembly = Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            //Intro text
            Console.WriteLine("Welcome to FromSoft Mod Language Patcher v" + version + " by Nordgaren");
            Console.WriteLine("Please contact me on GitHub with any bugs!");
            Console.WriteLine("https://github.com/Nordgaren/");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("If this your first time using this program, your language files WILL be backed up");
            Console.WriteLine("Detected source Language: " + sourceLang);
        }

        public static bool Confirm(string title)
        {
            ConsoleKey response;
            do
            {
                Console.Write($"{ title } [y/n] ");
                response = Console.ReadKey(false).Key;
                if (response != ConsoleKey.Enter)
                {
                    Console.WriteLine();
                }
            } while (response != ConsoleKey.Y && response != ConsoleKey.N);

            return (response == ConsoleKey.Y);
        }

    }
}
