using irbisAuthorizer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    public interface IRadius : IDebug
    { 
        bool WriteAccountLoginInfo(String idRdr, String ipAddr, String macAddr, String clientIdent);
        bool WriteAccountLogoutInfo(String idRdr, String ipAddr, String sessionTime, String inputByte, String outputByte, String macAddr, String clientIdent);
        String CheckLogPass(String idRdr, String pwdRdr);
    }
}
