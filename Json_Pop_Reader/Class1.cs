using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace pop_system
{
    
    class Json_reader
    {
        public enum government_name { Patronage = 144, Democracy = 96, Dictatorship = 112, Cooperative = 80, Corporate = 64, Feudal = 128, Confederacy = 48, Communism = 32, Anarchy = 16, Theocracy = 160, Prison_Colony = 150, unknown=-1}
        public enum security {  unknown = -1, none = 0, low = 16, medium = 32, high = 48}

        public struct pop_system_template
        {
            public int id;
            public int edsm_id;
            public string name;
            public float[] coordinates;
            public long population;
            public int government_type;
            public int allegiance_id;
            public int state_id;
            public int security_id;
            public int primary_economy_id;
            public long last_scan_date;
            public bool done;
        }
        public pop_system_template[] read(string file)
        {
            string[] file_contents = downloader();
            pop_system_template[] rtn = new pop_system_template[file_contents.Length];
            int spot = 0;
            foreach (string x in file_contents)
            {
                dynamic stuff = JObject.Parse(x);
                rtn[spot].id = stuff.id;
                switch(rtn[spot].id)
                {
                    case 7154923:
                        rtn[spot].edsm_id = 4671;
                        break;
                    case 8355343:
                        rtn[spot].edsm_id = 3852646;
                        break;
                    case 8716382:
                        rtn[spot].edsm_id = 4974219;
                        break;
                    default:
                        rtn[spot].edsm_id = stuff.edsm_id;
                        break;
                }
                rtn[spot].name = stuff.name;
                rtn[spot].coordinates = new float[3];
                rtn[spot].coordinates[0] = stuff.x;
                rtn[spot].coordinates[1] = stuff.y;
                rtn[spot].coordinates[2] = stuff.z;
                if (stuff.population != null)
                    rtn[spot].population = stuff.population;
                else
                    rtn[spot].population = -1;
                if (stuff.government_id != null)
                    rtn[spot].government_type = stuff.government_id;
                else
                    rtn[spot].government_type = -1;
                if (stuff.allegiance_id != null)
                    rtn[spot].allegiance_id = stuff.allegiance_id;
                else
                    rtn[spot].allegiance_id = -1;
                if (stuff.state_id != null)
                    rtn[spot].state_id = stuff.state_id;
                else
                    rtn[spot].state_id = -1;
                if (stuff.security_id != null)
                    rtn[spot].security_id = stuff.security_id;
                else
                    rtn[spot].security_id = -1;
                if (stuff.primary_economy_id != null)
                    rtn[spot].primary_economy_id = stuff.primary_economy_id;
                else
                    rtn[spot].primary_economy_id = -1;
                rtn[spot++].last_scan_date = stuff.updated_at;
            }
            return rtn;
        }
        public string[] downloader()
        {
           string[] ret = new string[0];
            string temp = "";
            using (WebClient client = new WebClient())
                temp = client.DownloadString("https://eddb.io/archive/v5/systems_populated.json");
            temp = temp.Substring(1, temp.Length - 2);
            temp = temp.Replace("]},{", "]}" + Environment.NewLine + "{");
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"temp.json"), temp);
            ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            return ret;
        }
    }
}
