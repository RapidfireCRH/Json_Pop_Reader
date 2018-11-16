//This file is part of Json_Pop_Reader.

//    Json_Pop_Reader is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    Json_Pop_Reader is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with Json_Pop_Reader.  If not, see<https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace pop_system
{
    class Program
    {
        /// <summary>
        /// Distance defination for closest query
        /// </summary>
        struct distance_template : IComparable<distance_template>
        {
            public int place;
            public double distance;
            public int CompareTo(distance_template other)
            {
                return this.distance.CompareTo(other.distance);
            }
        }
        /// <summary>
        /// Find old systems and displays known data
        /// </summary>
        /// v1.03 - help added, catching of errors
        /// v1.02 - Export, import and station support added
        /// v1.01 - closest and find commands added
        /// v1.0 - initial release
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //anounce version
            Console.WriteLine("Json_Pop_Reader");
            Console.WriteLine("V2.0a BETA EDSM Edition");
            for (int i = 0; i != 5; i++)
                Console.WriteLine();

            //Read in systems
            Json_reader j = new Json_reader();
            Json_reader.pop_system_template[] systems = j.read();
            j = new Json_reader(); // attempt to release all items in load for memory purposes
            
            //Sort by latest update date
            Array.Sort(systems);

            //Read done.txt if it exists
            if (File.Exists("done.txt"))
            {
                string[] read = File.ReadAllLines("done.txt");
                foreach (string x in read)
                    for (int i = 0; i != systems.Length; i++)
                        if (systems[i].id.ToString() == x)
                            systems[i].done = true;
            }

            //force garbage collection to reclaim ram
            GC.Collect();
            GC.WaitForPendingFinalizers();

            
            int ptr = 0;//system pointer for current system
            bool foundonfind = false;//Find flag to keep found system in selection
            bool list_stations = false;//List stations toggle
            bool forcereset = false;//force reset for resort
            while (true)
            {
                if (!foundonfind)
                {
                    int lastptr = ptr;
                    forcereset = false;
                    ptr = 0;
                    int search = 0;
                    distance_template[] scan = new distance_template[12];
                    for (int i = 0; i != 12; i++)
                    {
                        while (systems[search++].done) ;
                        scan[i].place = search - 1;
                        scan[i].distance = systems[search - 1].distance(systems[lastptr]);
                    }
                    Array.Sort(scan);
                    ptr = scan[0].place;
                }
                else
                    foundonfind = false;
                string k = "";
                systems[ptr].body_count = j.edsmbodies(systems[ptr].name);
                while (!systems[ptr].done && !forcereset )
                {
                    Console.Clear();
                    Console.WriteLine("Entry" + ptr + " | ID - " + systems[ptr].id + " | Last Update: " + systems[ptr].last_scan_date.ToShortDateString() + " " + systems[ptr].last_scan_date.ToLongTimeString());
                    Console.WriteLine("System Name: " + systems[ptr].name);
                    Console.WriteLine("Pop: " + (systems[ptr].population == -1 ? "Unknown" : systems[ptr].population.ToString()));
                    Console.WriteLine("System Gov: " + systems[ptr].government);
                    Console.WriteLine("Security: " + systems[ptr].security);
                    Console.WriteLine("Number of Bodies:");
                    Console.WriteLine("  EDSM: " + (systems[ptr].body_count == -1 ? "Unknown" : systems[ptr].body_count.ToString()));
                    Console.WriteLine("Number of Stations: " + systems[ptr].stations.Count);
                    if (list_stations)
                        foreach (Json_reader.station_template x in systems[ptr].stations)
                            Console.WriteLine("  " + station_writeline(x));
                    Console.WriteLine();
                    Console.Write("Enter Command (h for help):");
                    k = Console.ReadLine();
                    if (k.Length == 0)
                        continue;
                    switch (k.ToLower()[0])
                    {
                        case 'r'://open ross page
                            System.Diagnostics.Process.Start("https://ross.eddb.io/system/update/" + systems[ptr].eddbid.ToString());
                            break;
                        case 'e'://open EDSM page
                            System.Diagnostics.Process.Start("https://www.edsm.net/en/system/id/" + systems[ptr].id.ToString() + "/name");
                            break;
                        case 'd'://enter sector as done
                            systems[ptr].done = true;
                            break;
                        case 'f'://find system
                            string search2 = k.Substring(2);
                            foundonfind = false;
                            for (int i = 0; i != systems.Length; i++)
                            {
                                if (systems[i].name.ToLower() == search2.ToLower())
                                {
                                    ptr = i;
                                    systems[ptr].done = false;
                                    foundonfind = true;
                                    break;
                                }
                            }
                            if (!foundonfind)
                            {
                                Console.WriteLine("");
                                Console.WriteLine(search2 + " was not found.");
                                Thread.Sleep(4000);
                            }
                            break;
                        case 'c'://find closest systems
                            List<distance_template> list = new List<distance_template>();
                            for (int scan2 = 0; scan2 != systems.Length; scan2++)
                            {
                                distance_template next = new distance_template();
                                next.place = scan2;
                                next.distance = systems[ptr].distance(systems[scan2]);
                                list.Add(next);
                            }
                            list.Sort();
                            Console.WriteLine();
                            for (int i = 1; i != 21; i++)
                                Console.WriteLine(i.ToString() + ". " + Math.Ceiling(list[i].distance) + "ly | " + systems[list[i].place].name + " - Last Updated: " + systems[list[i].place].last_scan_date.ToShortDateString() + systems[list[i].place].last_scan_date.ToLongTimeString());
                            Console.WriteLine("Press [Enter] to continue");
                            Console.Read();
                            break;
                        case 'i'://import list of done systems
                            if (!File.Exists("done.txt"))
                            {
                                Console.WriteLine("");
                                Console.WriteLine("Not able to find done.txt to import. Please make sure it is in the same directory as this program.");
                                Thread.Sleep(6000);
                                break;
                            }
                            string[] read = File.ReadAllLines("done.txt");
                            foreach (string x in read)
                                for (int i = 0; i != systems.Length; i++)
                                    if (systems[i].id.ToString() == x)
                                        systems[i].done = true;
                            Console.WriteLine();
                            Console.WriteLine("Load completed.");
                            Thread.Sleep(4000);
                            break;
                        case 'x'://export list of done systems
                            List<string> write = new List<string>();
                            foreach (Json_reader.pop_system_template x in systems)
                                if (x.done)
                                    write.Add(x.id.ToString());
                            if (File.Exists("done.txt"))
                                File.Delete("done.txt");
                            File.WriteAllLines("done.txt", write);
                            Console.WriteLine();
                            Console.WriteLine("Save completed.");
                            Thread.Sleep(4000);
                            break;
                        case 'l'://toggle listing of stations
                            list_stations = !list_stations;
                            break;
                        case 'b'://reload bodies
                            systems[ptr].body_count = j.edsmbodies(systems[ptr].name);
                            break;
                        case 's'://change sort order
                            forcereset = true;
                            if (!systems[0].changeorder.Value)
                            {
                                for (int i = 0; i != systems.Length; i++)
                                    systems[i].changeorder = true;
                                Array.Sort(systems);
                            }
                            else
                            {
                                for (int i = 0; i != systems.Length; i++)
                                    systems[i].changeorder = false;
                                Array.Sort(systems);
                            }
                            break;
                        case 'h'://Help dialog
                            Console.Clear();
                            Console.WriteLine("List of commands:");
                            Console.WriteLine("  r - Opens ROSS page for currently selected star");
                            Console.WriteLine("  e - Opens EDSM page for currently selected star");
                            Console.WriteLine("  d - Marks star as done (automatically finds next oldest star)");
                            Console.WriteLine("  f (star name) - Finds the named star");
                            Console.WriteLine("  c - Find the closest 20 systems ");
                            Console.WriteLine("  i - Import done.txt list of names");
                            Console.WriteLine("  x - Export done.txt with list of done names");
                            Console.WriteLine("  l - Toggle list of stations (Key on next page)");
                            Console.WriteLine("  b - Reload body count");
                            Console.WriteLine("  s - Toggle sort order [By Star Last update, By oldest date between star and stations]");
                            Console.WriteLine("  h - Show this help menu");
                            Console.WriteLine();
                            Console.Write("(1/2) Press any key to continue.");
                            Console.ReadKey();
                            Console.Clear();
                            Console.WriteLine("Station Key:");
                            Console.WriteLine("  ? means unknown on EDSM.");
                            Console.WriteLine("  M - Has Market");
                            Console.WriteLine("  F - Has Refuel");
                            Console.WriteLine("  R - Has Repair");
                            Console.WriteLine("  A - Has Rearm");
                            Console.WriteLine("  T - Has Tuning (Refit)");
                            Console.WriteLine("  ");
                            Console.WriteLine("  m - Has Missions");
                            Console.WriteLine("  C - Has Contacts");
                            Console.WriteLine("  I - Has Intersteller Contacts");
                            Console.WriteLine("  U - Has Universal Catrographics");
                            Console.WriteLine("  c - Has Crew");
                            Console.WriteLine("  ");
                            Console.WriteLine("  RES - Has Search and Rescue");
                            Console.WriteLine("  MAT - Has Material Trader");
                            Console.WriteLine("  TEC - Has Technology Broker");
                            Console.WriteLine();
                            Console.Write("(2/2) Press any key to continue.");
                            Console.ReadKey();
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("Bad Command :" + k[0]);
                            Thread.Sleep(2000);
                            break;
                    }
                }

            }
        }
        static DateTime epochconvert(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }
        static string station_writeline(Json_reader.station_template station)
        {
            String ret = "";
            //ret += station.docking.HasValue ? ((bool)station.docking ? "D" : "X") : "?";
            ret += station.market.HasValue ? ((bool)station.market ? "M" : "-") : "?";
            ret += station.refuel.HasValue ? ((bool)station.refuel ? "F" : "-") : "?";
            ret += station.repair.HasValue ? ((bool)station.repair ? "R" : "-") : "?";
            ret += station.rearm.HasValue ? ((bool)station.rearm ? "A" : "-") : "?";
            ret += station.tuning.HasValue ? ((bool)station.tuning ? "T" : "-") : "?";
            ret += " ";
            ret += station.missions.HasValue ? ((bool)station.missions ? "m" : "-") : "?";
            ret += station.contacts.HasValue ? ((bool)station.contacts ? "C" : "-") : "?";
            ret += station.Interstellar_contact.HasValue ? ((bool)station.Interstellar_contact ? "I" : "-") : "?";
            ret += station.cartographics.HasValue ? ((bool)station.cartographics ? "U" : "-") : "?";
            ret += station.crew.HasValue ? ((bool)station.crew ? "c" : "-") : "?";
            ret += " ";
            ret += station.Search_and_Rescue.HasValue ? ((bool)station.Search_and_Rescue ? "RES" : "---") : "-?-";
            ret += " ";
            ret += station.Material_Trader.HasValue ? ((bool)station.Material_Trader ? "MAT" : "---") : "-?-";
            ret += " ";
            ret += station.Tech_Broker.HasValue ? ((bool)station.Tech_Broker ? "TEC" : "---") : "-?-";
            ret += " " + 
                (station.market_update.HasValue ? station.market_update.Value.ToShortDateString() : 
                (station.outfitting_update.HasValue ? station.outfitting_update.Value.ToShortDateString() : 
                (station.shipyard_update.HasValue ? station.shipyard_update.Value.ToShortDateString() : 
                (station.last_update.HasValue ? station.last_update.Value.ToShortDateString() : "Unknown Date"))));
            ret += " | " + station.name;
            return ret;
        }
    }
}
