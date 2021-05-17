using SoulsFormats;
using System;
using System.IO;
using System.Linq;

namespace FromSoft_Mod_Language_Patcher
{
    class Patcher
    {
        public static void Patch(string sourceLangDir, string sourceLang)
        {


            //Get Destination Languages
            var destLangPath = Directory.GetDirectories(Path.GetDirectoryName(sourceLangDir), "*.*", SearchOption.TopDirectoryOnly).Where(d => !d.EndsWith(sourceLang)).ToArray();

            //Debug Stuff
            //foreach (var lang in destFilePath)
            //{
            //    Console.WriteLine(Path.GetFileName(lang));
            //}

            Console.WriteLine("Patching...");

            foreach (var lang in destLangPath) //For each language
            {
                //Get Destination Files
                var destFilePath = Directory.GetFiles(lang).Where(name => !name.EndsWith(".bak")).ToArray();
                //Check if user wanted to restore backups
                if (Program.restoreBackups) //Attempt to restore backups
                {
                    RestoreBackups(lang, destFilePath);
                }
                else //Make backups
                {
                    Console.WriteLine("Backing up " + new DirectoryInfo(lang).Name);
                    MakeBackups(lang, destFilePath);
                }

                Console.WriteLine("Patching " + new DirectoryInfo(lang).Name);

                foreach (var file in destFilePath)//Patch each file in files Array
                {

                    if (BND3.Is(file))
                    {
                        //Set source BND files
                        string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                        BND3 sourceBND = BND3.Read(sourceLangFiles);
                        //Destination for BND files
                        BND3 destBND = BND3.Read(file);

                        //Debug Stuff
                        //Console.WriteLine(sourceBND.Files.Count);
                        //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                        //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count);

                        //Patch BND file
                        LangPatcher(sourceBND, destBND, file);

                        //Write new BND
                        destBND.Write(file);
                    }
                    else
                    {
                        //Set source BND files
                        string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                        BND4 sourceBND = BND4.Read(sourceLangFiles);
                        //Destination for BND files
                        BND4 destBND = BND4.Read(file);

                        //Debug Stuff
                        //Console.WriteLine(sourceBND.Files.Count);
                        //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                        //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count);

                        //Patch BND file
                        LangPatcher(sourceBND, destBND, file);

                        //Write new BND
                        destBND.Write(file);
                    }


                    /* Experimental file updater for missing FMG files
                    if (sourceBND.Files.Count != destBND.Files.Count)
                    {

                        FilePatcherBND4(sourceBND, destBND, destLang);

                        destBND = BND4.Read(destLang);

                        int i = 0;

                        Console.WriteLine(sourceBND.Files.Count);
                        Console.WriteLine(destBND.Files.Count);

                        foreach (var item in sourceBND.Files)
                        {
                            Console.WriteLine(item);
                            Console.WriteLine(destBND.Files[i]);
                            Console.ReadLine();

                            if (i < destBND.Files.Count - 1)
                                i++;
                        }

                    }
                    */
                }
            }
            //Let user know there are no more files to patch
            Console.WriteLine("Patching completed!");
        }

        public static void LangPatcher(IBinder sourceBND,IBinder destBND, string destLang)
        {
            int iFile = 0; //File counter
            int totalAdded = 0; //Total added per file
            int entriesOverwritten = 0; //Total overwritten per file

            foreach (var file in sourceBND.Files) //For each FMG in the source BND file
            {
                if ((Path.GetFileName(file.Name) == (Path.GetFileName(destBND.Files[iFile].Name)))) //If the file names match, update. If not, skip until they do match
                {
                    FMG sourceFMG = FMG.Read(file.Bytes);
                    FMG destFMG = FMG.Read((destBND.Files[iFile]).Bytes);
                    //Debug.WriteLine(destBND.Files[iFile].Name); //Debug
                    //Console.WriteLine(file.Name); //Debug
                    int i = 0;

                    foreach (var item in sourceFMG.Entries) //Each entry in the current FMG file
                    {
                        //Console.WriteLine(item.ID); //Debug
                        //Batch up dest entries if there are extras
                        if ((item.ID > destFMG.Entries[i].ID)) //Catch the count up if the entries in the destination langauge is out of place (Extra entries that shouldn't be there)
                        {
                            while ((item.ID > destFMG.Entries[i].ID) && (i < destFMG.Entries.Count - 1))
                            {
                                if (i < destFMG.Entries.Count - 1)
                                    i++;
                            }
                        }
                        //If item IDs match, check if the destination is null, then overwrite 
                        if (item.ID == destFMG.Entries[i].ID) //Overwrite all Null whitespace or empty entries in destination
                        {
                            //Console.WriteLine(item.ID + " = true"); //Debug
                            if (string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                            {
                                destFMG.Entries[i].Text = item.Text;
                                if (!string.IsNullOrWhiteSpace(destFMG.Entries[i].Text)) //Keep track of entries that actually changed
                                    entriesOverwritten++;
                                //Debug.WriteLine("writing " + destFMG.Entries[i].Text + " to " + destFMG.Entries[i].ID + " in " + Path.GetFileName(sourceBND.Files[iFile].Name));
                            }
                            if (i < destFMG.Entries.Count - 1)
                                i++;
                        }
                        else //Add new items
                        {
                            destFMG.Entries.Add(item);
                            totalAdded++;
                            //Debug Stuff
                            //Debug.WriteLine("added: " + item.ID);
                            //Console.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                            //Debug.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                        }
                    }
                    //Write the new files
                    destBND.Files[iFile].Bytes = destFMG.Write();
                    //Add to counter if it's not already maxed
                    if (iFile < destBND.Files.Count - 1)
                        iFile++;
                }
            }
            //Print stats for entire BND file
            Console.WriteLine("Patched: " + new DirectoryInfo(Path.GetDirectoryName(destLang)).Name + " " + Path.GetFileName(destLang) + ": "
                + totalAdded + " entries added and " + entriesOverwritten + " entries overwritten");
        }

        public static void MakeBackups(string lang, string[] destFilePath)
        {
            //Make Backups
            foreach (var thing in destFilePath)
            {
                //Make backups if the files aren't already backed up
                if (!File.Exists(thing + ".bak"))
                {
                    if (!File.Exists(thing + ".bak"))
                        File.Copy(thing, thing + ".bak");
                }
            }
        }

        public static void RestoreBackups(string lang, string[] destFilePath)
        {
            //Make Backups
            Console.WriteLine("Attempting to restore backups " + new DirectoryInfo(lang).Name);
            int backupsRestored = 0;

            foreach (var thing in destFilePath)
            {
                //If the backups exist, restore them
                if (File.Exists(thing + ".bak"))
                {
                    File.Copy(thing + ".bak", thing, true);
                    backupsRestored++;
                }
            }

            if (backupsRestored > 0) //Print how many backups were restored
            {
                Console.WriteLine(backupsRestored + " Backups restored");
            }
            else //If no backups restored, make backups
            {
                Console.WriteLine("Backups not present");
                Console.WriteLine("Backing up " + new DirectoryInfo(lang).Name);
                MakeBackups(lang, destFilePath);
            }
        }

        /* Emperimental and broken file distributor
        public static void FilePatcherBND4(BND4 sourceBND, BND4 destBND, string destLang)
        {
            int iFile = 0;

            foreach (var item in sourceBND.Files)
            {
                if (item != destBND.Files[iFile])
                {
                    //Console.WriteLine(Path.GetFileName(item.Name));
                    //Console.WriteLine(Path.GetDirectoryName(destBND.Files[1].Name));
                    //Console.WriteLine(Path.GetDirectoryName(destBND.Files[1].Name) + "\\" + Path.GetFileName(item.Name));
                    var newitem = item.Bytes;
                    destBND.Files.Add(newitem);
                }
                else
                {
                    if (iFile < destBND.Files.Count - 1)
                        iFile++;
                }
            }

            foreach (var file in destBND.Files)
            {
                destBND.Files[iFile].Bytes = destBND.Write();
                destBND.Write(destLang);
            }

        }
        */
                }
            }
