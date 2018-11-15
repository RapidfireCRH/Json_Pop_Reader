using pop_system;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Json_Pop_Reader
{
    class database
    {
        Json_reader.pop_system_template[] db = new Json_reader.pop_system_template[0];
        private void load()
        {
            Json_reader j = new Json_reader();
            db = j.read();
            db
        }
        public Json_reader.pop_system_template findnext()
        {
            if (db.Length == 0)
                load();
        }
    }
}
