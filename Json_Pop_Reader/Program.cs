﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Collections;

namespace pop_system
{
    class Program
    {
        struct distance_template : IComparable<distance_template>
        {
            public int place;
            public float distance;
            public int CompareTo(distance_template other)
            {
                return this.distance.CompareTo(other.distance);
            }
        }
        static void Main(string[] args)
        {
            Json_reader j = new Json_reader();
            Json_reader.pop_system_template[] systems = j.read();
            while (true)
            {
                int ptr = 0;
                for (int scan = 1; scan != systems.Length; scan++)
                    if (systems[ptr].last_scan_date > systems[scan].last_scan_date && systems[scan].done != true)
                        ptr = scan;
                string k = "";
                while (!systems[ptr].done)
                {
                    Console.Clear();
                    Console.WriteLine("Entry" + ptr + ": ID - " + systems[ptr].id + " System Name: " + systems[ptr].name);
                    Console.WriteLine("Last entry: " + epochconvert((int)systems[ptr].last_scan_date));
                    Console.WriteLine("Pop: " + (systems[ptr].population == -1 ? "Unknown" : systems[ptr].population.ToString()) + " | System Gov: " + (Json_reader.government_name)systems[ptr].government_type + " | Security: " + (Json_reader.security)systems[ptr].security_id);
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
                            for(int i = 0; i!=systems.Length;i++)
                            {
                                if (systems[i].name.ToLower() == search.ToLower())
                                {
                                    ptr = i;
                                    systems[ptr].done = false;
                                    found = true;
                                    break;
                                }
                            }
                            if(!found)
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
                            for(int i = 1; i!= 21; i++)
                                Console.WriteLine(i.ToString() + ". " + Math.Ceiling(list[i].distance) + "ly | " + systems[list[i].place].name + " - Last Updated: " + epochconvert((int)systems[list[i].place].last_scan_date));
                            Console.WriteLine("Press [Enter] to continue");
                            Console.Read();
                            break;
                        case 'i'://import list of done systems
                            if(!File.Exists("done.txt"))
                            {
                                Console.WriteLine("");
                                Console.WriteLine("Not able to find done.txt to import. Please make sure it is in the same directory as this program.");
                                Thread.Sleep(6000);
                                break;
                            }
                            string[] read = File.ReadAllLines("done.txt");
                            foreach(string x in read)
                                for (int i = 0; i != systems.Length; i++)
                                    if (systems[i].id.ToString() == x)
                                        systems[i].done = true;
                            break;
                        case 'x'://export list of done systems
                            List<string> write = new List<string>();
                            foreach(Json_reader.pop_system_template x in systems)
                                if (x.done)
                                    write.Add(x.id.ToString());
                            if (File.Exists("done.txt"))
                                File.Delete("done.txt");
                            File.WriteAllLines("done.txt", write);
                            break;
                    }
                }
                
            }
        }
        static DateTime epochconvert(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }
    }

}