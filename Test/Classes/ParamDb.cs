using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Classes
{
    public class ParamDb
    {
        private String dbName, user, pwd;

        public ParamDb(String dbName, String user, String pwd)
        {
            this.dbName = dbName;
            this.user = user;
            this.pwd = pwd;
        }

        public String getDbname()
        { return this.dbName; }

        public String getuser()
        { return this.user; }

        public String getpwd()
        { return this.pwd; }
    }
}
