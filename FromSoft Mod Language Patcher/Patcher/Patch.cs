using SoulsFormats;
using System;
using System.IO;

namespace FromSoft_Mod_Language_Patcher.Patcher
{
    public class Patch
    {

        public static void BND3Patch(string sourceLangDir, string sourceLang, string[] destLangPath, string[] destFilePath)
        {

            //Debug Stuff
            //foreach (var lang in destFilePath)
            //{
            //    Console.WriteLine(Path.GetFileName(lang));
            //}

            Console.WriteLine("Patching...");

            foreach (var lang in destLangPath)
            {
                if (Program.restoreBackups)
                {
                    //Restore Backups
                    RestoreBackups(lang, destFilePath);
                }
                else
                {
                    Console.WriteLine("Backing up " + new DirectoryInfo(lang).Name);
                    //Make Backups
                    MakeBackups(lang, destFilePath);
                }


                Console.WriteLine("Patching " + new DirectoryInfo(lang).Name);

                foreach (var file in destFilePath)//Patch each file in files Array
                {

                    //Set source BND files
                    string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                    BND3 sourceBND = BND3.Read(sourceLangFiles);
                    //Destination for BND files
                    string destLang = lang + "\\" + Path.GetFileName(file);
                    BND3 destBND = BND3.Read(destLang);

                    //Debug Stuff
                    //Console.WriteLine(sourceBND.Files.Count);
                    //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                    //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count);

                    //Patch BND file
                    LangPatcherBND3(sourceBND, destBND, destLang);

                    //Experimental file updater for missing FMG files
                    //if (sourceBND.Files.Count != destBND.Files.Count)
                    //{

                    //    FilePatcherBND3(sourceBND, destBND, destLang);

                    //    destBND = BND3.Read(destLang);

                    //    int i = 0;

                    //    Console.WriteLine(sourceBND.Files.Count);
                    //    Console.WriteLine(destBND.Files.Count);

                    //    foreach (var item in sourceBND.Files)
                    //    {
                    //        Console.WriteLine(item);
                    //        Console.WriteLine(destBND.Files[i]);
                    //        Console.ReadLine();

                    //        if (i < destBND.Files.Count - 1)
                    //            i++;
                    //    }

                    //}

                }

            }

            Console.WriteLine("Patching completed!");

        }

        public static void BND4Patch(string sourceLangDir, string sourceLang, string[] destLangPath, string[] destFilePath)
        {

            //Debug Stuff
            //foreach (var lang in destFilePath)
            //{
            //    Console.WriteLine(Path.GetFileName(lang));
            //}

            Console.WriteLine("Patching...");

            foreach (var lang in destLangPath)
            {
                if (Program.restoreBackups)
                {
                    //Restore Backups
                    RestoreBackups(lang, destFilePath);
                }
                else
                {
                    Console.WriteLine("Backing up " + new DirectoryInfo(lang).Name);
                    //Make Backups
                    MakeBackups(lang, destFilePath);
                }


                Console.WriteLine("Patching " + new DirectoryInfo(lang).Name);

                foreach (var file in destFilePath)//Patch each file in files Array
                {

                    //Set source BND files
                    string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                    BND4 sourceBND = BND4.Read(sourceLangFiles);
                    //Destination for BND files
                    string destLang = lang + "\\" + Path.GetFileName(file);
                    BND4 destBND = BND4.Read(destLang);

                    //Debug Stuff
                    //Console.WriteLine(sourceBND.Files.Count);
                    //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                    //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count);

                    //Patch BND file
                    LangPatcherBND4(sourceBND, destBND, destLang);

                    //Experimental file updater for missing FMG files
                    //if (sourceBND.Files.Count != destBND.Files.Count)
                    //{

                    //    FilePatcherBND4(sourceBND, destBND, destLang);

                    //    destBND = BND4.Read(destLang);

                    //    int i = 0;

                    //    Console.WriteLine(sourceBND.Files.Count);
                    //    Console.WriteLine(destBND.Files.Count);

                    //    foreach (var item in sourceBND.Files)
                    //    {
                    //        Console.WriteLine(item);
                    //        Console.WriteLine(destBND.Files[i]);
                    //        Console.ReadLine();

                    //        if (i < destBND.Files.Count - 1)
                    //            i++;
                    //    }

                    //}

                }

            }

            Console.WriteLine("Patching completed!");

        }



        public static void LangPatcherBND3(BND3 sourceBND, BND3 destBND, string destLang)
        {
            int iFile = 0;
            int totalAdded = 0;
            int entriesOverridden = 0;

            foreach (var file in sourceBND.Files)
            {

                if ((Path.GetFileName(file.Name) == (Path.GetFileName(destBND.Files[iFile].Name)))) //If the file names match, update. If not, skip until they do match
                {
                    FMG sourceFMG = FMG.Read(file.Bytes);
                    FMG destFMG = FMG.Read((destBND.Files[iFile]).Bytes);
                    //Debug.WriteLine(destBND.Files[iFile].Name);
                    //Console.WriteLine(file.Name);

                    int i = 0;

                    foreach (var item in sourceFMG.Entries)
                    {
                        //Console.WriteLine(item.ID);

                        if ((item.ID > destFMG.Entries[i].ID))//Catch the count up if the entries in the destination langauge is out of place (Extra entries that shouldn't be there)
                        {
                            while ((item.ID > destFMG.Entries[i].ID) && (i < destFMG.Entries.Count - 1))
                            {
                                if (i < destFMG.Entries.Count - 1)
                                    i++;
                            }
                        }

                        if (item.ID == destFMG.Entries[i].ID) //Overwrite all Null whitespace or empty entries in destination
                        {
                            //Console.WriteLine(item.ID + " = true"); //debug
                            if (string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                            {
                                destFMG.Entries[i].Text = item.Text; //Keep track of entries that actually changed
                                if (!string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                                    entriesOverridden++;
                                //Debug.WriteLine("writing " + destFMG.Entries[i].Text + " to " + destFMG.Entries[i].ID + " in " + Path.GetFileName(sourceBND.Files[iFile].Name));
                            }
                            if (i < destFMG.Entries.Count - 1)
                                i++;
                        }
                        else //Add new items
                        {
                            destFMG.Entries.Add(item);
                            totalAdded++;
                            //Debug.WriteLine("added: " + item.ID);
                            //Console.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                            //Debug.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                        }
                    }

                    destBND.Files[iFile].Bytes = destFMG.Write();
                    destBND.Write(destLang);

                    if (iFile < destBND.Files.Count - 1)
                        iFile++;
                }

            }

            Console.WriteLine("Patched: " + new DirectoryInfo(Path.GetDirectoryName(destLang)).Name + " " + Path.GetFileName(destLang) + ": " + totalAdded + " entries added and " + entriesOverridden + " entries overwritten");

        }

        public static void LangPatcherBND4(BND4 sourceBND, BND4 destBND, string destLang)
        {
            int iFile = 0;
            int totalAdded = 0;
            int entriesOverridden = 0;

            foreach (var file in sourceBND.Files)
            {

                if ((Path.GetFileName(file.Name) == (Path.GetFileName(destBND.Files[iFile].Name)))) //If the file names match, update. If not, skip until they do match
                {
                    FMG sourceFMG = FMG.Read(file.Bytes);
                    FMG destFMG = FMG.Read((destBND.Files[iFile]).Bytes);
                    //Debug.WriteLine(destBND.Files[iFile].Name);
                    //Console.WriteLine(file.Name);

                    int i = 0;

                    foreach (var item in sourceFMG.Entries)
                    {
                        //Console.WriteLine(item.ID);

                        if ((item.ID > destFMG.Entries[i].ID))//Catch the count up if the entries in the destination langauge is out of place (Extra entries that shouldn't be there)
                        {
                            while ((item.ID > destFMG.Entries[i].ID) && (i < destFMG.Entries.Count - 1))
                            {
                                if (i < destFMG.Entries.Count - 1)
                                    i++;
                            }
                        }

                        if (item.ID == destFMG.Entries[i].ID) //Overwrite all Null whitespace or empty entries in destination
                        {
                            //Console.WriteLine(item.ID + " = true"); //debug
                            if (string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                            {
                                destFMG.Entries[i].Text = item.Text; //Keep track of entries that actually changed
                                if (!string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                                    entriesOverridden++;
                                //Debug.WriteLine("writing " + destFMG.Entries[i].Text + " to " + destFMG.Entries[i].ID + " in " + Path.GetFileName(sourceBND.Files[iFile].Name));
                            }
                            if (i < destFMG.Entries.Count - 1)
                                i++;
                        }
                        else //Add new items
                        {
                            destFMG.Entries.Add(item);
                            totalAdded++;
                            //Debug.WriteLine("added: " + item.ID);
                            //Console.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                            //Debug.WriteLine(item.ID + " = " + (item.ID == destFMG.Entries[i].ID));
                        }
                    }

                    destBND.Files[iFile].Bytes = destFMG.Write();
                    destBND.Write(destLang);

                    if (iFile < destBND.Files.Count - 1)
                        iFile++;
                }

            }

            Console.WriteLine("Patched: " + new DirectoryInfo(Path.GetDirectoryName(destLang)).Name + " " + Path.GetFileName(destLang) + ": " + totalAdded + " entries added and " + entriesOverridden + " entries overwritten");

        }

        public static void MakeBackups(string lang, string[] destFilePath)
        {
            //Make Backups
            foreach (var thing in destFilePath)
            {
                if (!File.Exists(lang + "\\" + Path.GetFileName(thing) + ".bak"))
                {
                    if (!File.Exists(lang + "\\" + Path.GetFileName(thing) + ".bak"))
                        File.Copy(lang + "\\" + Path.GetFileName(thing), lang + "\\" + Path.GetFileName(thing) + ".bak");
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
                if (File.Exists(lang + "\\" + Path.GetFileName(thing) + ".bak"))
                {
                    File.Copy(lang + "\\" + Path.GetFileName(thing) + ".bak", lang + "\\" + Path.GetFileName(thing), true);
                    backupsRestored++;
                }
            }

            if (backupsRestored > 0)
            {
                Console.WriteLine(backupsRestored + " Backups restored");

            }
            else
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
