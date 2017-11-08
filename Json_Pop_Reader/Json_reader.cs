﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;

namespace pop_system
{

    class Json_reader
    {
        public enum government_name { Patronage = 144, Democracy = 96, Dictatorship = 112, Cooperative = 80, Corporate = 64, Feudal = 128, Confederacy = 48, Communism = 32, Anarchy = 16, Theocracy = 160, Prison_Colony = 150, unknown = -1 }
        public enum security { unknown = -1, none = 0, low = 16, medium = 32, high = 48 }
        public enum pad_size { unknown = -1, none = 0, Medium = 1, Large = 2}
        public enum station_type { unknown = -1, Civilian_Outpost = 1, Commercial_Outpost = 2, Coriolis_Starport = 3, Industrial_Outpost = 4, Military_Outpost = 5, Mining_Outpost = 6, Ocellus_Starport = 7, Orbis_Starport = 8, Scientific_Outpost = 9, Unknown_Outpost = 11, Unknown_Starport = 12, Planetary_Outpost = 13, Planetary_Port = 14, Unknown_Planetary = 15, Planetary_Settlement = 16, Planetary_Engineer_Base = 17, Megaship = 19, Asteroid_Base = 20 }

        public struct station_template
        {
            public int id;
            public string name;
            public Nullable<long> last_update;
            public Nullable<int> pad_size;
            public Nullable<int> type_id;
            public Nullable<bool> blackmarket;
            public Nullable<bool> market;
            public Nullable<bool> refuel;
            public Nullable<bool> repair;
            public Nullable<bool> rearm;
            public Nullable<bool> outfitting;
            public Nullable<bool> shipyard;
            public Nullable<bool> docking;
            public Nullable<bool> commodities;
            public Nullable<long> shipyard_update;
            public Nullable<long> outfitting_update;
            public Nullable<long> market_update;
            public Nullable<int> settlement_size;
            public Nullable<int> settlement_security;
            public Nullable<int> body_id;
        }
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
            public List<station_template> stations;
        }
        public pop_system_template[] read()
        {
            string[] file_contents = downloader("https://eddb.io/archive/v5/systems_populated.json");
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
                rtn[spot].stations = new List<station_template>();
                rtn[spot++].last_scan_date = stuff.updated_at;
            }
            file_contents = downloader("https://eddb.io/archive/v5/stations.json");
            foreach(string x in file_contents)
            {
                dynamic stuff = JObject.Parse(x);
                station_template st = new station_template();
                st.id = stuff.id;
                st.name = stuff.name;
                st.last_update = stuff.updated_at;
                switch(stuff.max_landing_pad_size)
                {
                    case "L":
                        st.pad_size = 2;
                        break;
                    case "M":
                        st.pad_size = 1;
                        break;
                    case "None":
                        st.pad_size = 0;
                        break;
                    default:
                        st.pad_size = -1;
                        break;
                }
                st.type_id = stuff.type_id;
                st.blackmarket = stuff.has_blackmarket;
                st.market = stuff.has_market;
                st.refuel = stuff.has_refuel;
                st.repair = stuff.has_repair;
                st.rearm = stuff.has_rearm;
                st.outfitting = stuff.has_outfitting;
                st.shipyard = stuff.has_shipyard;
                st.docking = stuff.has_docking;
                st.commodities = stuff.has_commodities;
                st.shipyard_update = stuff.shipyard_updated_at;
                st.outfitting_update = stuff.outfitting_updated_at;
                st.market_update = stuff.market_updated_at;
                st.settlement_size = stuff.settlement_size_id;
                st.settlement_security = stuff.settlement_security_id;
                int system_id = stuff.system_id;
                for (int i = 0; i != rtn.Length; i++)
                    if (rtn[i].id == system_id)
                        rtn[i].stations.Add(st);
            }
            return rtn;
        }
        public string[] downloader(string addr)
        {
            string temp = "";
            using (WebClient client = new WebClient())
                temp = client.DownloadString(addr);
            temp = temp.Substring(1, temp.Length - 2);
            temp = temp.Replace("},{\"id\"", "}" + Environment.NewLine + "{\"id\"");
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"temp.json"), temp);
            string[] ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            return ret;
        }
    }
}