using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    public static class Converter
    {
        public static bool DebugStrToBool(string str)
        {
            Boolean result;
            if (str == "1")
                result = true;
            else
                result = false;

            return result;
        }
    }
}
