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
        public enum government_type {unknown = -1, None, Anarchy, Communism, Confederacy, Cooperative, Corporate, Democracy, Dictatorship, Feudal, Patronage, Prison, Prison_colony, Theocracy }
        public enum security_type { unknown = -1, Anarchy, low, medium, high }
        public enum pad_size { unknown = -1, none, Medium, Large}
        public enum station_type { unknown = -1, Civilian_Outpost, Commercial_Outpost, Coriolis_Starport, Industrial_Outpost, Military_Outpost, Mining_Outpost, Ocellus_Starport, Orbis_Starport, Scientific_Outpost, Unknown_Outpost, Unknown_Starport, Planetary_Outpost, Planetary_Port, Unknown_Planetary, Planetary_Settlement, Planetary_Engineer_Base, Megaship, Asteroid_Base }
        public enum allegiance_type { unknown = -1, None, Independent, Federation, Empire, Alliance, Pilots_Federation}
        public enum economy_type { unknown = -1, Agriculture, Colony, Extraction, High_Tech, Industrial, Military, Refinery, Service, Terraforming, Tourism }
        public struct station_template
        {
            public int id;
            public long marketid;
            public station_type type;
            public string name;
            public Nullable<long> arrival_distance;
            public allegiance_type allegiance;
            public government_type government;
            public Nullable<economy_type> pri_economy;
            public Nullable<economy_type> sec_economy;
            public Nullable<bool> market;
            public Nullable<bool> shipyard;
            public Nullable<bool> outfitting;
            public Nullable<DateTime> last_update;
            public Nullable<int> pad_size;
            public Nullable<bool> blackmarket;
            public Nullable<bool> refuel;
            public Nullable<bool> repair;
            public Nullable<bool> rearm;
            public Nullable<bool> contacts;
            public Nullable<bool> cartographics;
            public Nullable<bool> missions;
            public Nullable<bool> crew;
            public Nullable<bool> tuning;
            public Nullable<bool> Interstellar_contact;
            public Nullable<bool> Search_and_Rescue;
            public Nullable<DateTime> shipyard_update;
            public Nullable<DateTime> outfitting_update;
            public Nullable<DateTime> market_update;
            public Nullable<int> body_id;
        }
        public struct pop_system_template
        {
            public int id;//edsm ID
            public Nullable<long> id64;
            public string name;
            public struct coord_st
            {
                public float x;
                public float y;
                public float z;
            }
            public coord_st coord;
            public allegiance_type allegiance;
            public government_type government;
            public economy_type economy;
            public security_type security;
            public int population;
            public struct faction_template
            {
                public int id;
                public string name;
            }
            public faction_template controlling_faction;
            public List<station_template> stations;
            public DateTime last_scan_date;
            public bool done;
        }
        public pop_system_template[] read()
        {
            Console.WriteLine("***********************************************************");
            Console.WriteLine("                    Loading... Please Wait");
            Console.WriteLine("***********************************************************");
            Console.WriteLine("Step 1 of 2: Loading Sectors");
            string[] file_contents = downloader("https://www.edsm.net/dump/systemsPopulated.json");
            pop_system_template[] rtn = new pop_system_template[file_contents.Length];
            int spot = 0;
            string temp = "";
            foreach (string x in file_contents)
            {
                if (x.Contains("\"stations\":[{"))
                    temp = "";
                dynamic stuff = JObject.Parse(x);
                rtn[spot].id = stuff.id;
                rtn[spot].id64 = stuff.id64;
                rtn[spot].name = stuff.name;
                rtn[spot].coord.x = stuff.coords.x;
                rtn[spot].coord.y = stuff.coords.y;
                rtn[spot].coord.z = stuff.coords.z;
                rtn[spot].allegiance = (allegiance_type)(stuff.allegiance == null ? -1 : (stuff.allegiance == "Pilots Federation"? allegiance_type.Pilots_Federation: stuff.allegiance));
                rtn[spot].government = (government_type)(stuff.government == null ? -1 : (stuff.government == "Prison colony"? government_type.Prison_colony : stuff.government));
                rtn[spot].economy = (economy_type)(stuff.economy == null? -1: (stuff.economy == "High Tech"? economy_type.High_Tech: stuff.economy));
                rtn[spot].security = (security_type)(stuff.security == null? -1: stuff.security);
                rtn[spot].controlling_faction.id = stuff.controllingFaction.id;
                rtn[spot].controlling_faction.name = stuff.controllingFaction.name;
                rtn[spot].stations = new List<station_template>();
                if (x.Contains("\"stations\":[{"))
                {
                    bool flag = false;
                    int i = 0;
                    station_template bldr = new station_template();
                    while (!flag)
                    {
                        try
                        {
                            bldr = new station_template();
                            bldr.id = stuff.stations[i].id;
                            bldr.marketid = stuff.stations[i].marketId;
                            bldr.type = recast(stuff.stations[i].type);
                            bldr.name = stuff.stations[i].name;
                            bldr.arrival_distance = stuff.stations[i].distanceToArrival;
                            bldr.allegiance = (allegiance_type)stuff.stations[i].allegiance;
                            bldr.government = (government_type)stuff.stations[i].government;
                            if (stuff.stations[i].economy == null)
                                bldr.pri_economy = null;
                            else
                                bldr.pri_economy = (economy_type)stuff.stations[i].economy;
                            if (stuff.stations[i].secondEconomy == null)
                                bldr.sec_economy = null;
                            else
                                bldr.sec_economy = (economy_type)stuff.stations[i].secondEconomy;
                            bldr.market = stuff.stations[i].haveMarket;
                            bldr.shipyard = stuff.stations[i].haveShipyard;
                            bldr.outfitting = stuff.stations[i].haveOutfitting;
                            if (stuff.stations[i].updateTime.information != null)
                            {
                                temp = stuff.stations[i].updateTime.information;
                                bldr.last_update = DateTime.Parse(temp);
                            }
                            else
                                bldr.last_update = null;
                            if (stuff.stations[i].updateTime.market != null)
                            {
                                temp = stuff.stations[i].updateTime.market;
                                bldr.market_update = DateTime.Parse(temp);
                            }
                            else
                                bldr.market_update = null;
                            if (stuff.stations[i].updateTime.shipyard != null)
                            {
                                temp = stuff.stations[i].updateTime.shipyard;
                                bldr.shipyard_update = DateTime.Parse(temp);
                            }
                            else
                                bldr.shipyard_update = null;
                            if (stuff.stations[i].updateTime.outfitting != null)
                            {
                                temp = stuff.stations[i].updateTime.outfitting;
                                bldr.outfitting_update = DateTime.Parse(temp);
                            }
                            else
                                bldr.outfitting_update = null;
                            rtn[spot].stations.Add(bldr);
                        }
                        catch(Exception e)
                        {
                            if (e.Message == "Index was out of range. Must be non - negative and less than the size of the collection.\r\nParameter name: index")
                                flag = true;
                            else
                                throw e;
                        }
                }
                }
                temp = stuff.date;
                rtn[spot].last_scan_date = DateTime.Parse(temp);
            }
            return rtn;
            //Console.WriteLine("Step 2 of 2: Loading Stations");
            //file_contents = downloader("https://eddb.io/archive/v5/stations.json");
            //foreach(string x in file_contents)
            //{
            //    dynamic stuff = JObject.Parse(x);
            //    station_template st = new station_template();
            //    st.id = stuff.id;
            //    st.name = stuff.name;
            //    st.last_update = stuff.updated_at;
            //    switch(stuff.max_landing_pad_size)
            //    {
            //        case "L":
            //            st.pad_size = 2;
            //            break;
            //        case "M":
            //            st.pad_size = 1;
            //            break;
            //        case "None":
            //            st.pad_size = 0;
            //            break;
            //        default:
            //            st.pad_size = -1;
            //            break;
            //    }
            //    st.type_id = stuff.type_id;
            //    st.blackmarket = stuff.has_blackmarket;
            //    st.market = stuff.has_market;
            //    st.refuel = stuff.has_refuel;
            //    st.repair = stuff.has_repair;
            //    st.rearm = stuff.has_rearm;
            //    st.outfitting = stuff.has_outfitting;
            //    st.shipyard = stuff.has_shipyard;
            //    st.docking = stuff.has_docking;
            //    st.commodities = stuff.has_commodities;
            //    st.shipyard_update = stuff.shipyard_updated_at;
            //    st.outfitting_update = stuff.outfitting_updated_at;
            //    st.market_update = stuff.market_updated_at;
            //    st.settlement_size = stuff.settlement_size_id;
            //    st.settlement_security = stuff.settlement_security_id;
            //    st.body_id = stuff.body_id;
            //    int system_id = stuff.system_id;
            //    for (int i = 0; i != rtn.Length; i++)
            //        if (rtn[i].id == system_id)
            //            rtn[i].stations.Add(st);
            //}
            //Console.Clear();
            //return rtn;
        }
        station_type recast(object obj)
        {
            if (!obj.ToString().Contains(" "))
                return (station_type)obj;
            switch(obj.ToString())
            {
                case "Civilian Outpost":
                    return station_type.Civilian_Outpost;
                case "Commercial Outpost":
                    return station_type.Commercial_Outpost;
                case "Coriolis Starport":
                    return station_type.Coriolis_Starport;
                case "Industrial Outpost":
                    return station_type.Industrial_Outpost;
                case "Military Outpost":
                    return station_type.Military_Outpost;
                case "Mining Outpost":
                    return station_type.Mining_Outpost;
                case "Ocellus Starport":
                    return station_type.Ocellus_Starport;
                case "Orbis Starport":
                    return station_type.Orbis_Starport;
                case "Scientific Outpost":
                    return station_type.Scientific_Outpost;
                case "Unknown Outpost":
                    return station_type.Unknown_Outpost;
                case "Unknown Starport":
                    return station_type.Unknown_Starport;
                case "Planetary Outpost":
                    return station_type.Planetary_Outpost;
                case "Planetary Port":
                    return station_type.Planetary_Port;
                case "Unknown Planetary":
                    return station_type.Unknown_Planetary;
                case "Planetary Settlement":
                    return station_type.Planetary_Settlement;
                case "Planetary Engineer Base":
                    return station_type.Planetary_Engineer_Base;
                case "Asteroid Base":
                    return station_type.Asteroid_Base;
                default:
                    return (station_type)obj;//will throw an error
            }
        }
        public string[] downloader(string addr)
        {
            string temp = "";
            using (WebClient client = new WebClient())
                temp = client.DownloadString(addr);
            temp = temp.Substring(6, temp.Length - 8);
            temp = temp.Replace("},\n    {\"id\"", "}" + Environment.NewLine + "{\"id\"");
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),"temp.json"), temp);
            string[] ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            return ret;
        }
    }
}