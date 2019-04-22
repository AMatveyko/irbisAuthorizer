using irbisAuthorizer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    public class Radius : IRadius
    {
        private EventHandler _debugger;

        public void AddDebugDelegate(EventHandler _delegate)
        {
            _debugger += _delegate;
        }

        private IDbClient _db;

        private string _error = "-> ModuleName: Radius,";

        public Radius(Parameters parameters)
        {
            _db = new DbClient(parameters);
            _db.AddDebugDelegate( PushDebug );
        }

        private bool WriteAccountInfo(String idRdr, String ipAddr, String macAddr, String clientIdent, String message)
        {
            String mfnRdr;
            if (!_db.Login()) return IfError();
            if (!_db.GetMfn(idRdr, out mfnRdr)) return false;
            if (!_db.ReadRdrRecord(mfnRdr)) return false;
            if (!_db.WriteRdrRecord(mfnRdr, ipAddr, message, clientIdent, macAddr)) return false;
            return true;
        }

        public bool WriteAccountLoginInfo(String idRdr, String ipAddr, String macAddr, String clientIdent)
        {
            String message = $"Wi_Fi(Вход)";
            return WriteAccountInfo(idRdr, ipAddr, macAddr, clientIdent, message);
        }
        public bool WriteAccountLogoutInfo(String idRdr, String ipAddr, String sessionTime, String inputByte, String outputByte, String macAddr, String clientIdent)
        {
            String message = $"Wi_Fi(Выход){sessionTime}с";
            return WriteAccountInfo(idRdr, ipAddr, macAddr, clientIdent, message);
        }
        public String CheckLogPass(String idRdr, String pwdRdr)
        {
            String acceptAnswer = "Accept";
            String rejectAnswer = "Reject";
            if (!_db.Login()) return String.Format($"{rejectAnswer}: {_db.GetError()}");
            if (!_db.FindReader(idRdr, pwdRdr)) return String.Format($"{rejectAnswer}: {GetError()}");

            String resolution = _db.GetFindReaderResolution();

            if (resolution == "1")
                return acceptAnswer;
            else
            {
                return rejectAnswer;
            }
        }

        private bool IfError()
        {
            _error += _db.GetError();
            return false;
        }

        public string GetError()
        {
            return _error;
        }

        private void PushDebug(Object o, EventArgs e)
        {
            _debugger?.Invoke(o, e);
        }
    }
}
