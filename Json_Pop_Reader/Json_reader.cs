using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace pop_system
{

    class Json_reader
    {
        bool testing_toggle = false;
        public enum government_type {unknown = -1, None, Anarchy, Communism, Confederacy, Cooperative, Corporate, Democracy, Dictatorship, Feudal, Patronage, Prison, Prison_colony, Theocracy, Workshop_Engineer }
        public enum security_type { unknown = -1, Anarchy, low, medium, high }
        public enum pad_size { unknown = -1, none, Medium, Large}
        public enum station_type { unknown = -1, Coriolis_Starport,  Ocellus_Starport, Orbis_Starport, Outpost, Scientific_Outpost, Planetary_Outpost, Planetary_Port,  Megaship, Asteroid_Base }
        public enum allegiance_type { unknown = -1, None, Independent, Federation, Empire, Alliance, Pilots_Federation}
        public enum economy_type { unknown = -1, Agriculture, Colony, Damaged, Extraction, High_Tech, Industrial, Military, Prison, Refinery, Repair, Rescue, Service, Terraforming, Tourism }
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
            public Nullable<bool> Material_Trader;
            public Nullable<bool> Tech_Broker;
            public Nullable<DateTime> shipyard_update;
            public Nullable<DateTime> outfitting_update;
            public Nullable<DateTime> market_update;
            public Nullable<int> body_id;
        }
        public struct pop_system_template : IComparable<pop_system_template>
        {
            public int id;//edsm ID
            public int eddbid;
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
            public int body_count;
            public bool done;
            public int CompareTo(pop_system_template other)
            {
                return this.last_scan_date.CompareTo(other.last_scan_date);
            }
            public double distance(pop_system_template other)
            {
                return Math.Sqrt(Math.Pow(this.coord.x - other.coord.x, 2) + Math.Pow(this.coord.y - other.coord.y, 2) + Math.Pow(this.coord.z - other.coord.z, 2));
            }
        }
        /// <summary>
        /// Main load for Sectors and Stations
        /// </summary>
        /// <returns>Main data struct for application</returns>
        public pop_system_template[] read()
        {
            Console.WriteLine("***********************************************************");
            Console.WriteLine("                    Loading... Please Wait");
            Console.WriteLine("***********************************************************");
            Console.WriteLine("Step 1 of 2: Loading EDSM Info");
            string[] file_contents = downloader("https://www.edsm.net/dump/systemsPopulated.json");
            pop_system_template[] rtn = new pop_system_template[file_contents.Length];
            int spot = 0;
            string temp = "";
            foreach (string x in file_contents)
            {
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
                 
                    station_template bldr = new station_template();
                    for (int i = 0; i != stuff.stations.Count; i++)
                    {
                        bldr = new station_template();
                        bldr.id = stuff.stations[i].id;
                        if (stuff.stations[i].marketId == null)
                            bldr.marketid = -1;
                        else
                            bldr.marketid = stuff.stations[i].marketId;
                        if (stuff.stations[i].type == null)
                            bldr.type = station_type.unknown;
                        else
                            bldr.type = recaststation(stuff.stations[i].type);
                        bldr.name = stuff.stations[i].name;
                        bldr.arrival_distance = stuff.stations[i].distanceToArrival;
                        if (stuff.stations[i].allegiance == null)
                            bldr.allegiance = allegiance_type.unknown;
                        else
                            bldr.allegiance = (allegiance_type)(stuff.stations[i].allegiance == "Pilots Federation" ? allegiance_type.Pilots_Federation : stuff.stations[i].allegiance);
                        if (stuff.stations[i].government == null)
                            bldr.government = government_type.unknown;
                        else
                            bldr.government = (government_type)(stuff.stations[i].government == "Prison colony" ? government_type.Prison_colony : (stuff.stations[i].government == "Workshop (Engineer)"? government_type.Workshop_Engineer: stuff.stations[i].government));
                        if (stuff.stations[i].economy == null)
                            bldr.pri_economy = null;
                        else
                            bldr.pri_economy = (stuff.stations[i].economy == "High Tech" ? economy_type.High_Tech : (economy_type)stuff.stations[i].economy);
                        if (stuff.stations[i].secondEconomy == null)
                            bldr.sec_economy = null;
                        else
                            bldr.sec_economy = (stuff.stations[i].secondEconomy == "High Tech" ? economy_type.High_Tech : (economy_type)stuff.stations[i].secondEconomy);
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
                        if (stuff.stations[i].otherServices.Count != 0)
                            bldr.blackmarket = bldr.rearm = bldr.refuel = bldr.repair = bldr.contacts = bldr.cartographics = bldr.missions = bldr.crew = bldr.tuning = bldr.Interstellar_contact = bldr.Search_and_Rescue = bldr.Material_Trader = bldr.Tech_Broker = false;
                        for(int j = 0; j!=stuff.stations[i].otherServices.Count;j++)
                        {
                            string tmp = stuff.stations[i].otherServices[j];
                            switch (tmp)
                            {
                                case "Black Market":
                                    bldr.blackmarket = true;
                                    continue;
                                case "Restock":
                                    bldr.rearm = true;
                                    continue;
                                case "Refuel":
                                    bldr.refuel = true;
                                    continue;
                                case "Repair":
                                    bldr.repair = true;
                                    continue;
                                case "Contacts":
                                    bldr.contacts = true;
                                    continue;
                                case "Universal Cartographics":
                                    bldr.cartographics = true;
                                    continue;
                                case "Missions":
                                    bldr.missions = true;
                                    continue;
                                case "Crew Lounge":
                                    bldr.crew = true;
                                    continue;
                                case "Tuning":
                                    bldr.tuning = true;
                                    continue;
                                case "Interstellar Factors Contact":
                                    bldr.Interstellar_contact = true;
                                    continue;
                                case "Search and Rescue":
                                    bldr.Search_and_Rescue = true;
                                    continue;
                                case "Material Trader":
                                    bldr.Material_Trader = true;
                                    continue;
                                case "Technology Broker":
                                    bldr.Tech_Broker = true;
                                    continue;
                                default:
                                    throw new NotImplementedException("Unknown service: " + tmp);

                            }
                        }
                        rtn[spot].stations.Add(bldr);
                    }
                }
                temp = stuff.date;
                rtn[spot++].last_scan_date = DateTime.Parse(temp);
            }
            Console.WriteLine("Step 2 of 2: Loading EDDB Info");
            file_contents = EDDBdownload("https://eddb.io/archive/v5/systems_populated.json");
            foreach (string x in file_contents)
            {
                dynamic stuff = JObject.Parse(x);
                for (int i = 0; i != rtn.Length; i++)
                {
                    if (rtn[i].eddbid != 0)
                        continue;
                    if (stuff.edsm_id == rtn[i].id)
                    {
                        rtn[i].eddbid = stuff.id;
                        break;
                    }
                }
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
        station_type recaststation(object obj)
        {
            switch(obj.ToString())
            {
                case "Coriolis Starport":
                    return station_type.Coriolis_Starport;
                case "Ocellus Starport":
                    return station_type.Ocellus_Starport;
                case "Orbis Starport":
                    return station_type.Orbis_Starport;
                case "Scientific Outpost":
                    return station_type.Scientific_Outpost;
                case "Planetary Outpost":
                    return station_type.Planetary_Outpost;
                case "Planetary Port":
                    return station_type.Planetary_Port;
                case "Asteroid base":
                    return station_type.Asteroid_Base;
                case "Outpost":
                    return station_type.Outpost;
                case "Mega ship":
                    return station_type.Megaship;
                default:
                    throw new NotImplementedException("Uknown starport type: " + obj);
            }
        }
        public string[] EDDBdownload(string addr)
        {
            string temp = "";
            using (WebClient client = new WebClient())
                temp = client.DownloadString(addr);
            temp = temp.Substring(1, temp.Length - 2);//Remove [ and ]
            temp = temp.Replace("},{\"id\"", "}" + Environment.NewLine + "{\"id\"");//move each entry into a new line

            //Write text to read (faster then going line per line)
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"), temp);
            string[] ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
            return ret;
        }
        public string[] downloader(string addr)
        {
            if (testing_toggle)
            {
                if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json")) || File.GetCreationTime(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json")) < DateTime.Now.AddDays(-1))
                {
                    File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
                    string temp = "";
                    using (WebClient client = new WebClient())
                        temp = client.DownloadString(addr);
                    temp = temp.Substring(6, temp.Length - 8);
                    temp = temp.Replace("},\n    {\"id\"", "}" + Environment.NewLine + "{\"id\"");
                    File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"), temp);
                }
                string[] ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
                return ret;
            }
            else
            {
                string temp = "";
                using (WebClient client = new WebClient())
                    temp = client.DownloadString(addr);
                temp = temp.Substring(6, temp.Length - 8);
                temp = temp.Replace("},\n    {\"id\"", "}" + Environment.NewLine + "{\"id\"");
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"), temp);
                string[] ret = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
                File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.json"));
                return ret;
            }
        }
        public int edsmbodies(string name)
        {
            try
            {
                string temp = "";
                using (WebClient client = new WebClient())

                    temp = client.DownloadString("https://www.edsm.net/api-system-v1/bodies?systemName=" + name);
                dynamic stuff = JObject.Parse(temp);
                if (stuff.bodies.Count > 0)//Wanted to say unknown for 0 bodies, since each star has atleast one body (host star)
                    return stuff.bodies.Count;
            }

            catch (Exception e)
            {
                Console.WriteLine("Unable to get body count from EDSM. " + e.Message + Environment.NewLine + e.StackTrace);
                Thread.Sleep(2000);
            }
            return -1;
        }
    }
}