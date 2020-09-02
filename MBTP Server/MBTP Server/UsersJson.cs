using System;
using System.Collections.Generic;
using System.Text;

namespace MBTP_Server
{
    class UsersJson
    {
        public UsersJson2[] users;
    }

    class UsersJson2
    {
        public string username;
        public string password;
        public string rootpath;
        public string fullaccess;
    }
}
