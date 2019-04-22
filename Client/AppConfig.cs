using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using irbisAuthorizer.Model;
using Microsoft.Extensions.Configuration;

namespace Client
{

    public static class AppConfig
    {

        private static String _configFile = "ClientConfig.json";

        private static String _error = $"-> ModulName: AppConfig,";
        public static String GetError() => _error;

        //private static String _configFile = "config.json";
        private static String _hostKey = "Host";
        private static String _port = "Port";
        private static String _userNameKey = "UserName";
        private static String _userPwdKey = "UserPwd";
        private static String _dbNameKey = "DbName";
        private static String _nameFieldKey = "NameField";
        private static String _pwdFieldKey = "PwdField";
        private static String _debugKey = "Debug";

        private static IConfigurationRoot _config;

        public static Boolean GetParameters(out Parameters parameters)
        {
            Boolean isOk = true;
            IPAddress host;
            int port;
            String userName, password, db, nameField, pwdField, debug;
            if (File.Exists(_configFile))
            {
                try
                {
                    _config = new ConfigurationBuilder().AddJsonFile(_configFile).Build();
                    isOk = CheckHost(out host, ref _hostKey) && isOk;
                    isOk = CheckPort(out port, ref _port) && isOk;
                    isOk = NotEmpty(out userName, ref _userNameKey) && isOk;
                    isOk = NotEmpty(out password, ref _userPwdKey) && isOk;
                    isOk = NotEmpty(out db, ref _dbNameKey) && isOk;
                    isOk = NotEmpty(out nameField, ref _nameFieldKey) && isOk;
                    isOk = NotEmpty(out pwdField, ref _pwdFieldKey) && isOk;
                    isOk = NotEmpty(out debug, ref _debugKey) && isOk;
                    parameters = new Parameters(host, port, userName, password, db, nameField, pwdField, debug);
                }
                catch (Exception e)
                {
                    parameters = null;
                    _error += $" AppConfig.GetParameters: {e.Message}";
                    return false;
                }
            }
            else
            {
                _error += $" AppConfig.GetParameters: File not found";
                isOk = false;
                parameters = null;
            }
            
            return isOk;
        }

        private static bool CheckPort(out int port, ref String key)
        {
            String portStr;
            if(NotEmpty(out portStr, ref key))
            {
                if(Int32.TryParse(portStr, out port))
                {
                    if((port < 1) || (port > 65534))
                    {
                        _error += $" \"{portStr}\" range error! (1-65535)";
                        return false;
                    }
                    return true;
                }
                _error += $" \"{portStr}\" not port!";
            }
            port = 0;
            return false;
        }

        private static bool CheckHost(out IPAddress host, ref String key)
        {
            String hostStr;
            if(NotEmpty(out hostStr, ref key))
            {

                if(!IPAddress.TryParse(hostStr, out host))
                {
                    IPHostEntry iPHostEntry;
                    try
                    {
                        iPHostEntry = Dns.GetHostEntry(hostStr);
                        host = iPHostEntry.AddressList[0];
                    }
                    catch(Exception e)
                    {
                        _error += $" AppConfig.ChckHost: {e.Message}";
                        return false;
                    }
                    
                }
                return true;
            }
            else
            {
                host = null;
                return false;
            }
        }

        private static bool NotEmpty(out String to, ref String key)
        {
            if(String.IsNullOrEmpty(_config[key]))
            {
                _error += $" Not found value for key: \"{key}\".";
                to = null;
                return false;
            }
            to = _config[key];
            return true;
        }
    }
}
