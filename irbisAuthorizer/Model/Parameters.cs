using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace irbisAuthorizer.Model
{
    public class Parameters
    {
        private readonly IPAddress _host;
        private readonly int _port;
        private readonly String _userName;
        private readonly String _password;
        private readonly String _dbName;
        private readonly String _nameFieldNumber;
        private readonly String _pwdFieldNumber;
        private readonly String _debug;

        public Parameters(IPAddress host, int port, String userName, String password, String dbName, String nameFieldNumber, String pwdFieldNumber, String debug)
        {
            _host = host;
            _port = port;
            _userName = userName;
            _password = password;
            _dbName = dbName;
            _nameFieldNumber = nameFieldNumber;
            _pwdFieldNumber = pwdFieldNumber;
            _debug = debug;
        }

        public IPAddress GetHost() => _host;
        public int GetPort() => _port;
        public String GetUserName() => _userName;
        public String GetPassword() => _password;
        public String GetDbName() => _dbName;
        public String GetNameFieldNumber() => _nameFieldNumber;
        public String GetPwdFieldNumber() => _pwdFieldNumber;
        public String GetDebug() => _debug;
    }
}
