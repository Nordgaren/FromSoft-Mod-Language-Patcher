using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FromSoft_Mod_Language_Patcher
{
    class Patcher
    {
        private static bool NoRef { get; set; }

        public static void Patch(string sourceLangDir, string sourceLang)
        {
            //Get Destination Languages and ref directory
            var destLangPath = Directory.GetDirectories(Path.GetDirectoryName(sourceLangDir), "*.*", SearchOption.TopDirectoryOnly).Where(d => !d.EndsWith(sourceLang)).ToArray();
            string refLangDir = Directory.GetCurrentDirectory() + $@"\ref\{ sourceLang }";
            #region Debug Stuff
            //foreach (var lang in destFilePath)
            //{
            //    ConsoleLog(Path.GetFileName(lang));
            //}
            #endregion

            File.Delete($@"{ sourceLangDir }\LangPatchLog.txt");

            //Check if reference directory exists and set bool NoRef
            CheckRefDir(refLangDir);

            ConsoleLog("Patching...");
            foreach (var lang in destLangPath) //For each language
            {
                //Get Destination Files
                var destFilePath = Directory.GetFiles(lang).Where(name => !name.EndsWith(".bak")).ToArray();
                //Check if user wanted to restore backups
                if (Program.RestoreBackups) //Attempt to restore backups
                {
                    RestoreBackups(lang, destFilePath);
                }
                else //Make backups
                {
                    MakeBackups(lang, destFilePath);
                }
                //Write the language being patched and patch BND
                ConsoleLog($"Patching { new DirectoryInfo(lang).Name }");
                PatchBND(sourceLangDir, refLangDir, destFilePath);
                ConsoleLog($"Patching { new DirectoryInfo(lang).Name } Complete");
            }
            //Let user know there are no more files to patch
            ConsoleLog("Patching completed!");
        }

        private static void CheckRefDir(string refLangDir)
        {
            //Check if reference exists
            if (Directory.Exists(refLangDir))
            {
                ConsoleLog("Reference File Detected");
                NoRef = false;
            }
            else
            {
                ConsoleLog("No Reference File Detected");
                NoRef = true;
                NoRef = !Program.Confirm("Would you like to skip overwrites?");
            }
        }

        private static void PatchBND(string sourceLangDir, string refLangDir, string[] destFilePath)
        {
            foreach (var file in destFilePath)//Patch each file in files Array
            {
                if (BND3.IsRead(file, out BND3 destBND3))
                {
                    //Set source BND files
                    string sourceLangFiles = $@"{ sourceLangDir }\{ Path.GetFileName(file) }";
                    BND3 sourceBND = BND3.Read(sourceLangFiles);
                    string refLangFiles = $@"{refLangDir}\{Path.GetFileName(file)}";
                    //Make null ref BND, and make an actual BND read if it exists 
                    BND3 refBND = null;
                    if (File.Exists(refLangFiles))
                    {
                        refBND = BND3.Read(refLangFiles);
                    }
                    #region Debug Stuff
                    //ConsoleLog(sourceBND.Files.Count);
                    //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                    //ConsoleLog(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count); 
                    #endregion
                    
                    //Patch BND file
                    PatchFMG(sourceBND, destBND3, refBND, file);

                    //Write new BND
                    destBND3.Write(file);
                }
                else if (BND4.IsRead(file, out BND4 destBND4))
                {
                    //Set source BND files
                    string sourceLangFiles = $@"{sourceLangDir}\{Path.GetFileName(file)}";
                    BND4 sourceBND = BND4.Read(sourceLangFiles);
                    string refLangFiles = $@"{refLangDir}\{Path.GetFileName(file)}";
                    //Make null ref BND, and make an actual BND read if it exists 
                    BND4 refBND = null;
                    if (File.Exists(refLangFiles))
                    {
                        refBND = BND4.Read(refLangFiles);
                    }

                    #region Debug Stuff
                    //ConsoleLog(sourceBND.Files.Count);
                    //Debug.WriteLine(Path.GetFileName(sourceBND.Files[0].Name));
                    //ConsoleLog(destFMG.Entries.Count + " & " + sourceFMG.Entries.Count); 
                    #endregion
                    //Patch BND file
                    PatchFMG(sourceBND, destBND4, refBND, file);

                    //Write new BND
                    destBND4.Write(file);
                }
            }
        }

        private static List<string> Log = new List<string>();

        private static void PatchFMG(IBinder sourceBND, IBinder destBND, IBinder refBND, string destLang)
        {
            int iFile = 0; //File counter
            int iRef = 0; //Reference index counter
            int entriesAdded = 0; //Total added per file
            int entriesOverwritten = 0; //Total overwritten per file

            Log.Add($"{ new DirectoryInfo(Path.GetDirectoryName(destLang)).Name } { Path.GetFileName(destLang) } start");  

            foreach (var file in sourceBND.Files) //For each FMG in the source BND file
            {
                if (file.ID > destBND.Files[iFile].ID) //Compatability for using program from non-English folders. Does nothing in the English folder.
                {
                    while ((file.ID > destBND.Files[iFile].ID) && (iFile < destBND.Files.Count - 1))
                    {
                        //ConsoleLog("Skipped " + destBND.Files[iFile].ID); //Debug
                        if (iFile < destBND.Files.Count - 1)
                            iFile++;
                    }
                }

                if (file.ID == destBND.Files[iFile].ID) //If the file names match, update. If not, skip until they do match
                {
                    //Add the source and destination FMG
                    Log.Add($"Destination: { Path.GetFileName(destBND.Files[iFile].Name) } / Source: { Path.GetFileName(file.Name) }");
                    //ConsoleLog(file.ID + " = true"); // Debug
                    FMG sourceFMG = FMG.Read(file.Bytes);
                    FMG destFMG = FMG.Read((destBND.Files[iFile]).Bytes);

                    //Make a refFMG and refDict if refBND isn't null
                    Dictionary<int, string> refDict = MakeRef(refBND, iRef);

                    //Make dictionaries out of the FMG files
                    Dictionary<int, string> sourceDict = sourceFMG.Entries.GroupBy(x => x.ID).Select(x => x.First()).ToDictionary(x => x.ID, x => x.Text);
                    Dictionary<int, string> destDict = destFMG.Entries.GroupBy(x => x.ID).Select(x => x.First()).ToDictionary(x => x.ID, x => x.Text);

                    entriesAdded = AddNew(entriesAdded, sourceDict, destDict);

                    //Make dicitonary based on comparing sourceDict to refDict
                    if (refDict != null)
                    {
                        entriesOverwritten = UpdateText(entriesOverwritten, refDict, sourceDict, destDict);
                    }

                    //Clear and rewrite FMG file
                    RewriteFMG(destFMG, destDict);

                    //Replace old null entries if no reference.
                    if (NoRef)
                    {
                        Log.Add("Changed:");
                        entriesOverwritten = NullOverwrite(entriesOverwritten, sourceFMG, destFMG);
                    }

                    #region Debug Stuff
                    //foreach (var entry in destFMG.Entries)
                    //{
                    //    ConsoleLog(entry);
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
                //    ConsoleLog(file.ID + " = false");
                //} 
                #endregion
                iRef++;
            }
            //Print stats for entire BND file
            ConsoleLog($"Patched: { new DirectoryInfo(Path.GetDirectoryName(destLang)).Name } { Path.GetFileName(destLang) }: { entriesAdded } entries added and { entriesOverwritten } entries overwritten");

            //Append log file
            Log.Add($"{ new DirectoryInfo(Path.GetDirectoryName(destLang)).Name } { Path.GetFileName(destLang) } end");
            File.AppendAllLines($@"{ Directory.GetCurrentDirectory() }\LangPatchLog.txt", Log);
            Log.Clear();
        }

        private static Dictionary<int, string> MakeRef(IBinder refBND, int iRef)
        {
            FMG refFMG = null;
            Dictionary<int, string> refDict = null;
            if (refBND != null)
            {
                refFMG = FMG.Read((refBND.Files[iRef]).Bytes);
                refDict = refFMG.Entries.ToDictionary(t => t.ID, t => t.Text);
            }

            return refDict;
        }

        private static int AddNew(int entriesAdded, Dictionary<int, string> sourceDict, Dictionary<int, string> destDict)
        {
            //Get all of the Keys that don't match
            var newEntries = sourceDict.Keys.Except(destDict.Keys);

            if (newEntries.Count() > 0)
                Log.Add("Added");

            //Add new keys and their values to the dictionary
            foreach (var item in newEntries)
            {
                destDict.Add(item, sourceDict[item]);
                Log.Add($"{ item }: { sourceDict[item]}");
                entriesAdded++;
            }

            return entriesAdded;
        }

        private static int UpdateText(int entriesOverwritten, Dictionary<int, string> refDict, Dictionary<int, string> sourceDict, Dictionary<int, string> destDict)
        {
            var diffDict = sourceDict.Except(refDict);

            if (diffDict.Count() > 0)
                Log.Add("Changed:");

            foreach (var item in diffDict)
            {
                if (item.Value != destDict[item.Key])
                {
                    Log.Add($"{ item.Key }: { destDict[item.Key] } to { item.Value }");
                    //ConsoleLog($" before | { destDict[item.Key] } |"); // Debug see changes
                    destDict[item.Key] = item.Value;
                    entriesOverwritten++;
                    //ConsoleLog($" after  |{ destDict[item.Key] }|"); //Debug see changes
                }
            }

            return entriesOverwritten;
        }

        private static int NullOverwrite(int entriesOverwritten, FMG sourceFMG, FMG destFMG)
        {
            int i = 0;
            foreach (var item in sourceFMG.Entries) //Each entry in the current FMG file
            {
                //ConsoleLog(item.ID); //Debug
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
                    //ConsoleLog(item.ID + " = true"); //Debug
                    if (string.IsNullOrWhiteSpace(destFMG.Entries[i].Text))
                    {
                        destFMG.Entries[i].Text = item.Text;

                        if (!string.IsNullOrWhiteSpace(destFMG.Entries[i].Text)) //Keep track of entries that actually changed
                        {
                            Log.Add($"{ destFMG.Entries[i].ID }: { destFMG.Entries[i].Text }");
                            entriesOverwritten++;
                        }
                        //Debug.WriteLine("writing " + destFMG.Entries[i].Text + " to " + destFMG.Entries[i].ID + " in " + Path.GetFileName(sourceBND.Files[iFile].Name));
                    }
                    if (i < destFMG.Entries.Count - 1)
                        i++;
                }
            }

            return entriesOverwritten;
        }

        private static void RewriteFMG(FMG destFMG, Dictionary<int, string> destDict)
        {
            destFMG.Entries.Clear();
            foreach (var item in destDict)
            {
                destFMG.Entries.Add(new FMG.Entry(item.Key, item.Value));
            }
        }

        private static void MakeBackups(string lang, string[] destFilePath)
        {
            //Make backups
            ConsoleLog($"Backing up { new DirectoryInfo(lang).Name }");
            int backupsMade = 0;

            foreach (var thing in destFilePath)
            {
                //Make backups if the files aren't already backed up
                if (!File.Exists(thing + ".bak"))
                {
                    File.Copy(thing, thing + ".bak");
                    backupsMade++;
                }
            }

            if (backupsMade > 0)
                ConsoleLog($"{ backupsMade } backups made");
            else
                ConsoleLog("Backups already exist.");
        }

        private static void RestoreBackups(string lang, string[] destFilePath)
        {
            //Attempt to restore backups
            ConsoleLog($"Attempting to restore backups { new DirectoryInfo(lang).Name }");
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
                ConsoleLog($"{ backupsRestored } Backups restored");
            }
            else //If no backups restored, make backups
            {
                ConsoleLog("Backups not present");
                MakeBackups(lang, destFilePath);
            }
        }

        public static void ConsoleLog(string message)
        {
            Console.WriteLine(message);
            Log.Add(message);
        }
    }
}
