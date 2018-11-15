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
            public float distance;
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
            Json_reader j = new Json_reader();
            Json_reader.pop_system_template[] systems = j.read();
            if (systems.Length == 0)//error catching on json load.
                return;
            bool list_stations = false;
            while (true)
            {
                int ptr = 0;
                for (int scan = 1; scan != systems.Length; scan++)
                    if (systems[ptr].last_scan_date > systems[scan].last_scan_date && systems[scan].done != true)
                        ptr = scan;
                string k = "";
                systems[ptr].edsm_body_count = j.edsmdownloader(systems[ptr].name);
                while (!systems[ptr].done)
                {
                    Console.Clear();
                    Console.WriteLine("Entry" + ptr + " | ID - " + systems[ptr].id + " | Last Update: " + epochconvert((int)systems[ptr].last_scan_date));
                    Console.WriteLine("System Name: " + systems[ptr].name);
                    Console.WriteLine("Pop: " + (systems[ptr].population == -1 ? "Unknown" : systems[ptr].population.ToString()));
                    Console.WriteLine("System Gov: " + (Json_reader.government_name)systems[ptr].government_type);
                    Console.WriteLine("Security: " + (Json_reader.security)systems[ptr].security_id);
                    Console.WriteLine("Number of Bodies:");
                    Console.WriteLine("  EDSM: " + (systems[ptr].edsm_body_count == -1 ? "Unknown" : systems[ptr].edsm_body_count.ToString()));
                    Console.WriteLine("  EDDB: Unknown");
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
                            System.Diagnostics.Process.Start("https://ross.eddb.io/system/update/" + systems[ptr].id.ToString());
                            break;
                        case 'e'://open EDSM page
                            System.Diagnostics.Process.Start("https://www.edsm.net/en/system/id/" + systems[ptr].edsm_id.ToString() + "/name");
                            break;
                        case 'd'://enter sector as done
                            systems[ptr].done = true;
                            break;
                        case 'f'://find system
                            string search = k.Substring(2);
                            bool found = false;
                            for (int i = 0; i != systems.Length; i++)
                            {
                                if (systems[i].name.ToLower() == search.ToLower())
                                {
                                    ptr = i;
                                    systems[ptr].done = false;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                Console.WriteLine("");
                                Console.WriteLine(search + " was not found.");
                                Thread.Sleep(4000);
                            }
                            break;
                        case 'c'://find closest systems
                            List<distance_template> list = new List<distance_template>();
                            for (int scan = 0; scan != systems.Length; scan++)
                            {
                                distance_template next = new distance_template();
                                next.place = scan;
                                next.distance = (float)Math.Sqrt(Math.Pow(systems[scan].coordinates[0] - systems[ptr].coordinates[0], 2) + Math.Pow(systems[scan].coordinates[1] - systems[ptr].coordinates[1], 2) + Math.Pow(systems[scan].coordinates[2] - systems[ptr].coordinates[2], 2));
                                list.Add(next);
                            }
                            list.Sort();
                            Console.WriteLine();
                            for (int i = 1; i != 21; i++)
                                Console.WriteLine(i.ToString() + ". " + Math.Ceiling(list[i].distance) + "ly | " + systems[list[i].place].name + " - Last Updated: " + epochconvert((int)systems[list[i].place].last_scan_date));
                            Console.WriteLine("Press any key to continue");
                            Console.ReadKey();
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
                        case 'b':
                            systems[ptr].edsm_body_count = j.edsmdownloader(systems[ptr].name);
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
                            Console.WriteLine("  l - Toggle list of stations (Key below)");
                            Console.WriteLine("  b - Reload body count");
                            Console.WriteLine("  h - Show this help menu");
                            Console.WriteLine();
                            Console.WriteLine("Station Key:");
                            Console.WriteLine("  ? means unknown on EDDB/ROSS.");
                            Console.WriteLine("  D - Docking allowed; X - Docking not allowed");
                            Console.WriteLine("  M - Has Market");
                            Console.WriteLine("  F - Has Refuel");
                            Console.WriteLine("  R - Has Repair");
                            Console.WriteLine("  A - Has Rearm");
                            Console.WriteLine("  B - Has Blackmarket");
                            Console.WriteLine();
                            Console.Write("Press any key to continue.");
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
            ret += station.docking.HasValue ? ((bool)station.docking ? "D" : "X") : "?";
            ret += station.market.HasValue ? ((bool)station.market ? "M" : "-") : "?";
            ret += station.refuel.HasValue ? ((bool)station.refuel ? "F" : "-") : "?";
            ret += station.repair.HasValue ? ((bool)station.repair ? "R" : "-") : "?";
            ret += station.rearm.HasValue ? ((bool)station.rearm ? "A" : "-") : "?";
            ret += station.blackmarket.HasValue ? ((bool)station.blackmarket ? "B" : "-") : "?";
            ret += " " + 
                (station.market_update.HasValue ? epochconvert((int)station.market_update).ToShortDateString() : 
                (station.outfitting_update.HasValue ? epochconvert((int)station.outfitting_update).ToShortDateString() : 
                (station.shipyard_update.HasValue ? epochconvert((int)station.shipyard_update).ToShortDateString() : 
                (station.last_update.HasValue ? epochconvert((int)station.last_update).ToShortDateString() : "Unknown Date"))));
            ret += " | " + station.name;
            return ret;
        }
    }
}
