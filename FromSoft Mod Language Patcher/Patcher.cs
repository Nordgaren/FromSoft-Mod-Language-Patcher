using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FromSoft_Mod_Language_Patcher
{
    class Patcher
    {
        static bool noRef = false;

        public static void Patch(string sourceLangDir, string sourceLang)
        {
            //Get Destination Languages and ref directory
            var destLangPath = Directory.GetDirectories(Path.GetDirectoryName(sourceLangDir), "*.*", SearchOption.TopDirectoryOnly).Where(d => !d.EndsWith(sourceLang)).ToArray();
            string refLangDir = Directory.GetCurrentDirectory() + @"\ref\" + sourceLang;

            #region Debug Stuff
            //foreach (var lang in destFilePath)
            //{
            //    Console.WriteLine(Path.GetFileName(lang));
            //}
            #endregion
            //Check if refernce exists
            if (Directory.Exists(refLangDir))
            {
                Console.WriteLine("Reference File Detected");
                noRef = false;
            }
            else
            {
                Console.WriteLine("No Reference File Detected");
                noRef = true;
            }

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
                    if (BND3.IsRead(file, out BND3 destBND3))
                    {
                        //Set source BND files
                        string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                        BND3 sourceBND = BND3.Read(sourceLangFiles);
                        string refLangFiles = refLangDir + "\\" + Path.GetFileName(file);
                        //Make null ref BND, and make an actual BND read if it exists 
                        BND3 refBND = null;
                        if (File.Exists(refLangFiles))
                        {
                            refBND = BND3.Read(refLangFiles);
                        }

                        #region Debug Stuff
                        //Console.WriteLine(sourceBND.Files.Count);
                        //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                        //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count); 
                        #endregion

                        //Patch BND file
                        LangPatcher(sourceBND, destBND3, refBND, file);

                        //Write new BND
                        destBND3.Write(file);
                    }
                    else if (BND4.IsRead(file, out BND4 destBND4))
                    {
                        //Set source BND files
                        string sourceLangFiles = sourceLangDir + "\\" + Path.GetFileName(file);
                        BND4 sourceBND = BND4.Read(sourceLangFiles);
                        string refLangFiles = refLangDir + "\\" + Path.GetFileName(file);
                        //Make null ref BND, and make an actual BND read if it exists 
                        BND4 refBND = null;
                        if (File.Exists(refLangFiles))
                        {
                            refBND = BND4.Read(refLangFiles);
                        }

                        #region Debug Stuff
                        //Console.WriteLine(sourceBND.Files.Count);
                        //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                        //Console.WriteLine(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count); 
                        #endregion

                        //Patch BND file
                        LangPatcher(sourceBND, destBND4, refBND, file);

                        //Write new BND
                        destBND4.Write(file);
                    }
                }
            }
            //Let user know there are no more files to patch
            Console.WriteLine("Patching completed!");
        }

        public static void LangPatcher(IBinder sourceBND,IBinder destBND, IBinder refBND, string destLang)
        {
            int iFile = 0; //File counter
            int iRef = 0; //Reference index counter
            int totalAdded = 0; //Total added per file
            int entriesOverwritten = 0; //Total overwritten per file

            foreach (var file in sourceBND.Files) //For each FMG in the source BND file
            {
                if (file.ID > destBND.Files[iFile].ID)
                {
                    while ((file.ID > destBND.Files[iFile].ID) && (iFile < destBND.Files.Count - 1))
                    {
                        //Console.WriteLine("Skipped " + destBND.Files[iFile].ID); //Debug
                        if (iFile < destBND.Files.Count - 1)
                            iFile++;
                    }
                }

                if (file.ID == destBND.Files[iFile].ID) //If the file names match, update. If not, skip until they do match
                {
                    //Console.WriteLine(file.ID + " = true"); // Debug
                    FMG sourceFMG = FMG.Read(file.Bytes);
                    FMG destFMG = FMG.Read((destBND.Files[iFile]).Bytes);

                    //Make a refFMG and refDict if refBND isn't null
                    FMG refFMG = null;
                    Dictionary<int, string> refDict = null;
                    if (refBND != null)
                    {
                        refFMG = FMG.Read((refBND.Files[iRef]).Bytes);
                        refDict = refFMG.Entries.ToDictionary(t => t.ID, t => t.Text);
                    }

                    //Make dictionaries out of the FMG files
                    Dictionary<int, string> sourceDict = sourceFMG.Entries.GroupBy(x => x.ID).Select(x => x.First()).ToDictionary(x => x.ID, x => x.Text);
                    Dictionary<int, string> destDict = destFMG.Entries.GroupBy(x => x.ID).Select(x => x.First()).ToDictionary(x => x.ID, x => x.Text);

                    //Get all of the Keys that don't match
                    var newEntries = sourceDict.Keys.Except(destDict.Keys);

                    //Add new keys and their values to the dictionary
                    foreach (var item in newEntries)
                    {
                        destDict.Add(item, sourceDict[item]);
                        totalAdded++;
                    }
                    
                    //Make dicitonary based on comparing sourceDict to refDict
                    if (refDict != null)
                    {
                        Dictionary<int, string> diffDict = sourceDict.Except(refDict).ToDictionary(t => t.Key, t => t.Value);

                        foreach (var item in diffDict)
                        {
                            if (item.Value != destDict[item.Key])
                            {
                                   destDict.Remove(item.Key);
                                   destDict.Add(item.Key, item.Value);
                                   entriesOverwritten++;
                            }
                        }
                    }

                    //Clear and rewrite FMG file
                    destFMG.Entries.Clear();
                    foreach (var item in destDict)
                    {
                        destFMG.Entries.Add(new FMG.Entry(item.Key, item.Value));
                    }

                    //Replace old null entries if no reference.
                    if (noRef)
                    {
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
                        }
                    }
                    #region Debug Stuff
                    //foreach (var entry in destFMG.Entries)
                    //{
                    //    Console.WriteLine(entry);
                    //} 
                    #endregion

                    //Write the new files
                    destBND.Files[iFile].Bytes = destFMG.Write();
                    //Add to counter if it's not already maxed
                    if (iFile < destBND.Files.Count - 1)
                        iFile++;
                }
                #region Debug Stuff
                //else //Debug
                //{
                //    Console.WriteLine(file.ID + " = false");
                //} 
                #endregion
                iRef++;
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
    }
}
