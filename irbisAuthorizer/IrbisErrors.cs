using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    internal static class IrbisErrors
    {
        private static Dictionary<string, string> _errors;

        public static String GetErrorValue(String key)
        {
            if (_errors.ContainsKey(key))
                return _errors[key];
            else
                return "NOT FOUND";
        }

        static IrbisErrors()
        {
            _errors = new Dictionary<string, string>()
            {
                { "10745" , "RECORD ADDED" },
                { "0" , "ZERO" },
                { "-1111" , "SERVER_EXECUTE_ERROR" },
                { "-2222" , "WRONG_PROTOCOL" },
                { "-3333" , "CLIENT_NOT_IN_LIST" },
                { "-3334" , "CLIENT_NOT_IN_USE" },
                { "-3335" , "CLIENT_IDENTIFIER_WRONG" },
                { "-3336" , "CLIENT_NOT_ALLOWED" },
                { "-3337" , "CLIENT_ALREADY_EXISTS" },
                { "-3340" , "ERROR?" },
                { "-4444" , "WRONG_PASSWORD" },
                { "-5555" , "FILE_NOT_EXISTS" },
                { "-6666" , "SERVER_OVERLOAD" },
                { "-7777" , "PROCESS_ERROR" },
                { "-100" , "READ_WRONG_MFN" },
                { "-600" , "REC_DELETE" },
                { "-601" , "REC_PHYS_DELETE" },
                { "-602" , "ERR_RECLOCKED" },
                { "-603" , "REC_DELETE" },
                { "-607" , "AUTOIN_ERROR" },
                { "-300" , "ERR_DBEWLOCK" },
                { "-400" , "ERR_FILEMASTER" },
                { "-401" , "ERR_FILEINVERT" },
                { "-402" , "ERR_WRITE" },
                { "-403" , "ERR_ACTUAL" },
                { "-202" , "TERM_NOT_EXISTS" },
                { "-203" , "TERM_LAST_IN_LIST" },
                { "-204" , "TERM_FIRST_IN_LIST" }
            };

        }
    }
}
