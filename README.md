# Json_Pop_Reader
Takes populated system data from EDDM and looks for old system information

Place Json_Pop_Reader.exe and Newtonsoft.Json.dll in the same folder. Launch Json_Pop_Reader.exe and it will load the latest Json into memory. BE CAREFUL - THIS PROGRAM WILL USE 4+GB ON STARTUP. THIS WILL ONLY WORK ON 64bit Systems.

Commands are as follows:

  (e)dsm - open edsm page for target

  (r)oss - open ross page for target

  (d)one - done with target, find next target

  (f)ind - finds star with name. syntax is f [name]

  (c)losest - finds the 20 closest systems and displays their distance, and when they were last updated

  (i)mport - import done.txt to read in all done stars

  e(x)port - export done.txt file of all done sector id files
  
  (b)odies - reload body count. Be careful with this command, as you may reach your api rate limit with EDSM.
  
  (h)elp - shows help dialog (showing this information)
  
  (l)ist stations - Station list toggle. Stations follow the following format:
          
          DMFRAT mCIUc TEC RES MAT  1/1/1970 | Station_name

          Station Key:
            ? means unknown on EDSM.
            M - Has Market
            F - Has Refuel
            R - Has Repair
            A - Has Rearm
            T - Has Tuning (Refit)

            m - Has Missions
            C - Has Contacts
            I - Has Intersteller Contacts
            U - Has Universal Catrographics
            c - Has Crew

            TEC - Has Technology Broker
            RES - Has Search and Rescue
            MAT - Has Material Trader
            
          Date - Date from Marketupdate > outfittingupdate > shipyardupdate > last_update in that order

          Name - Station name
          
          Each symbol is a - when not available and a ? when unknown.

Feel free to raise a new issue for any features/ issues you might have.

More detailed explination on what is going on.

This program downloads all populated systems from EDSM and sorts them based on last updated date. It will return the earliest dates along with some information about the sector. At this point you can enter any of the above commands to change or open at your leisure. If you enter multiple commands, it will only execute the first one.
