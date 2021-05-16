using SoulsFormats;
using System;
using System.IO;
using System.Linq;

namespace FromSoft_Mod_Language_Patcher
{
    class Program
    {
        static void Main(string[] args)
        {

            //Get source Directory and Language
            string sourceLangDir = Directory.GetCurrentDirectory();
            string sourceLang = new DirectoryInfo(sourceLangDir).Name;

            //Get Destination languages and filepath
            var destLangPath = Directory.GetDirectories(Path.GetDirectoryName(sourceLangDir), "*.*", SearchOption.TopDirectoryOnly).Where(d => !d.EndsWith(sourceLang)).ToArray();
            var destFilePath = Directory.GetFiles(destLangPath[0]).Where(name => !name.EndsWith(".bak")).ToArray();

            StartUp(sourceLang);

            if (Confirm("Would you like to patch the other language files?"))
            {
                //Check which method needs to be called
                if (BND3.Is(Directory.GetCurrentDirectory() + "\\" + Path.GetFileName(destFilePath[0])))
                    Patcher.Patch.BND3Patch(sourceLangDir, sourceLang, destLangPath, destFilePath);
                else
                    Patcher.Patch.BND4Patch(sourceLangDir, sourceLang, destLangPath, destFilePath);
            }
            else
            {
                Console.WriteLine("Closing without patching...");
                Console.WriteLine("Press ENTER...");
            }

            Console.ReadLine();
        }

        public static void StartUp(string sourceLang)
        {
            Console.WriteLine("Welcome to FromSoft Mod Language Patcher V1.0 by Nordgaren");
            Console.WriteLine("Please contact me on GitHub with any bugs!");
            Console.WriteLine("https://github.com/Nordgaren/");
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Your language files WILL be backed up");
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
