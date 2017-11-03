using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace pop_system
{
    class Program
    {
        static void Main(string[] args)
        {
            Json_reader j = new Json_reader();
            Json_reader.pop_system_template[] systems = j.read(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"systems_populated.json"));
            while (true)
            {
                int ptr = 0;
                for (int scan = 1; scan != systems.Length; scan++)
                    if (systems[ptr].last_scan_date > systems[scan].last_scan_date && systems[scan].done != true)
                        ptr = scan;
                Console.Clear();
                Console.WriteLine("Entry" + ptr + ": ID - " + systems[ptr].id + " System Name: " + systems[ptr].name);
                Console.WriteLine("Last entry: " + epochconvert((int)systems[ptr].last_scan_date));
                Console.WriteLine("Pop: " + (systems[ptr].population == -1 ? "Unknown" : systems[ptr].population.ToString()) + " System Gov: " + (Json_reader.government_name)systems[ptr].government_type + " Security: " + (Json_reader.security)systems[ptr].security_id);
                string temp = "";
                char k = ' ';
                while (k != 'd' && !systems[ptr].done)
                {
                    k = (char)Console.Read();
                    switch (k)
                    {
                        case 'r':
                            temp = "https://ross.eddb.io/system/update/" + systems[ptr].id.ToString();
                            Process.Start(new ProcessStartInfo("cmd", $"/c start {temp}") { CreateNoWindow = true });
                            break;
                        case 'e':
                            temp = "https://www.edsm.net/en/system/bodies/id/" + systems[ptr].edsm_id.ToString() + "/name";
                            Process.Start(new ProcessStartInfo("cmd", $"/c start {temp}") { CreateNoWindow = true });
                            break;
                    }
                }
                systems[ptr].done = true;
            }
        }
        static DateTime epochconvert(int epoch)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epoch);
        }
    }

}