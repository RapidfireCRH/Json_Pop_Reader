# Json_Pop_Reader
Takes populated system data from EDDB and looks for old system information

Place Json_Pop_Reader.exe and Newtonsoft.Json.dll in the same folder. Launch Json_Pop_Reader.exe and it will load the latest Json into memory.

Commands are as follows:

  (e)dsm - open edsm page for target

  (r)oss - open ross page for target

  (d)one - done with target, find next target

  (f)ind - finds star with name. syntax is f [name]

  (c)losest - finds the 20 closest systems and displays their distance, and when they were last updated

  (i)mport - import done.txt to read in all done stars

  e(x)port - export done.txt file of all done sector id files

Feel free to raise a new issue for any features/ issues you might have.

More detailed explination on what is going on.

This program downloads all populated systems from EDDB.io and sorts them based on last updated date. It will return the earliest dates along with some information about the sector. At this point you can enter any of the above commands to change or open at your leisure. If you enter multiple commands, it will only execute the first one.
