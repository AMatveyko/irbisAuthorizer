using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    internal interface IDbClient : IDebug
    {
        bool Login();
        bool GetMfn(String idRdr, out String mfnRdr);
        bool FindReader(String idRdr, String pwdRdr);
        bool ReadRdrRecord(String mfnRdr);
        bool WriteRdrRecord(String mfnRdr, String ipAddr, String message, String clientIdent, String macAddr);
        string GetFindReaderResolution();
    }
}
