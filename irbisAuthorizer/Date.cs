using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    internal static class Date
    {
        internal static void GetDateTime(out String date, out String time)
        {
            String dateTime = DateTime.Now.ToString("yyyyMMdd|HHmmss");
            String[] tmpArr = dateTime.Split('|');
            date = tmpArr[0];
            time = tmpArr[1];
        }
    }
}
