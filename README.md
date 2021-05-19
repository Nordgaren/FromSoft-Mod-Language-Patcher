# FromSoft Mod Language Patcher by Nordgaren
This is a tool that patches the languages for FromSoft mods. 
Moves all new entries from English FMG files to other languages.
Will also overwrite all changed entries with a reference file, provided by you (vanilla version of the BND you changed)
If don't provide a reference file, the program will overwrite all nulls and tell you how many changed (so any nulls that's ere changed to use in the mod WILL get changed)
It uses SoulsFormats by JK Anderson  

## Current know compatability: 
* Dark Souls: Prepare To Die Edition
* Dark Souls: Remastered
* Dark Souls III

If you would like to help me test any other FromSoft game that isn't on the list, please contact me through GitHub  
https://github.com/Nordgaren


### Instructions

1) Drag EXE into English folder of the game you are trying to patch.

(optional)If you'd like to update existing entries, make a folder named ref inside the foler with the EXE and add the VANILLA English folder of your game to the ref folder (I.E. ref/ENGLISH for DS1 and ref/engus for DS3)  

Run the EXE. 

If you run without the reference, any entries that were null and changed will be updated, but any entries that were previously used (I.E existing items) will not. This is NOT the preffered way of running it, as it COULD break your new language files. Not entirely sure.

I would recommend restoring backups if you are unsure of changes, or if you previously ran the tool without a reference file.  

I also recommend you don't delete the backup files!  

### Thank You

NamelessHoodie for literally teaching me the basics of how to use SoulsFormats  
TKGP for making SoulsFormats and suggesting some great optimizations
thefifthmatt for suggesting Dictionaries
geeeeeorge for helping me figure out FMG IDs
Meowmaritus :fatcat:  
Also Dropoff probably suggestions  

### Patch Notes  
## V 2.2
* Now asks if you want to skip overwrites if you have no reference folder
## V 2.1
* Should now be compatible with custom file names for mods
* Ignores duplicate entries in source FMGs
## V 2.0
* Added comparison feature to overwrite changed entries by mod author
## V 1.2
* Code optimizations
* Runs much faster, now
## V 1.1
* added restore backups feature
