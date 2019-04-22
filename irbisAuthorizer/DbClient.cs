using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using irbisAuthorizer.Model;

namespace irbisAuthorizer
{
    internal class DbClient : IDbClient
    {
        private String _error = "-> ModuleName: DbClient,";
        public String GetError() => _error;

        private EventHandler _debugger;

        public void AddDebugDelegate(EventHandler debugger)
        {
            _debugger += debugger;
        }

        private IConnection _conn;

        private static int _correct = 0;
        private static Char _arm = 'C';
        private static int _seq = 0;
        private static int Seq { get => ++_seq; }
        private static int _id = 388456;

        private string _dbLogin;
        private string _dbName;
        private string _dbPasswd;
        private string _dbFn;
        private string _dbFp;


        private bool _debug = false;

        public DbClient(Parameters parameters)
        {
            _dbLogin = parameters.GetUserName();
            _dbPasswd = parameters.GetPassword();
            _dbName = parameters.GetDbName();
            _dbFn = parameters.GetNameFieldNumber();
            _dbFp = parameters.GetPwdFieldNumber();
            _conn = new Connection(parameters);
            _conn.AddDebugDelegate(PushDebug);
            _debug = Converter.DebugStrToBool(parameters.GetDebug());
        }

        public string GetFindReaderResolution()
        {
            return _conn.AnswerLines[11];
        }

        public bool Login()
        {
            String packet = $"A\n{_arm}\nA\n{_id}\n{Seq}\n\n\n\n\n\n{_dbLogin}\n{_dbPasswd}";
            if (!_conn.SendPacket(packet)) return IsConnFalse();
            return true;
        }
        public bool FindReader(String idRdr, String pwdRdr)
        {
            String packet = $"K\n{_arm}\nK\n{_id}\n{Seq}\n\n\n\n\n\n{_dbName}\n\"A=$\"\n1000\n1\n@brief\n\n\n!if ((v{_dbFn}='{idRdr}')*(v{_dbFp}='{pwdRdr}')) then '1' else '0' fi";
            _conn.SendPacket(packet);
            return true;
        }
        public bool GetMfn(String idRdr, out String mfnRdr)
        {
            mfnRdr = String.Empty;
            String packet = $"K\n{_arm}\nK\n{_id}\n{Seq}\n\n\n\n\n\n{_dbName}\n\"A=$\"\n1000\n1\n@brief\n\n\n!if (v{_dbFn} = '{idRdr}') then '1' else '0' fi";
            if (!_conn.SendPacket(packet)) return IsConnFalse();
            if (_conn.AnswerLines[11] == "1")
            {
                Regex regex = new Regex("([0-9]{1,10})#");
                Match match = regex.Match(_conn.AnswerLines[12]);
                mfnRdr = (match.Groups)[1].Value;
                return true;
            }
            else
            {
                _error += "GetMfn(): MFN not found!";
                return false;
            }
        }
        public bool ReadRdrRecord(String mfnRdr)
        {
            String lockRdr = "0";
            String packet = $"C\n{_arm}\nC\n{_id}\n{Seq}\n\n\n\n\n\n{_dbName}\n{mfnRdr}\n{lockRdr}";
            if (!_conn.SendPacket(packet)) return IsConnFalse();
            return true;
        }
        public bool WriteRdrRecord(String mfnRdr, String ipAddr, String message, String clientIdent, String macAddr)
        {
            String date;
            String time;
            String lockRec = "0";
            String ifUpdate = "1";
            List<String> answerLines;
            Date.GetDateTime(out date, out time);
            if ((_conn == null) || (_conn.AnswerLines == null))
            {
                _error += "Answer is null";
                return false;
            }
            answerLines = _conn.AnswerLines;
            answerLines.Add($"40#^X{ipAddr}^D{date}^C{message}^V{clientIdent}^U{macAddr}^1{time}^2{time}");

            byte[] separatorCode = new byte[] { 31, 30 };
            String separatorStr = Encoding.UTF8.GetString(separatorCode);
            String packet = $"D\n{_arm}\nD\n{_id}\n{Seq}\n\n\n\n\n\n{_dbName}\n{lockRec}\n{ifUpdate}\n{mfnRdr}#0{separatorStr}";

            for (int i = 12; i < answerLines.Count; i++)
            {
                if (answerLines[i].Length > 1)
                {
                    packet += answerLines[i] + separatorStr;
                    _correct += 2;
                }
            }
            if(_debug)
                Console.WriteLine(packet);
            if (!_conn.SendPacket(packet)) return IsConnFalse();
            return true;
        }

        private void PushDebug(Object o, EventArgs e)
        {
            _debugger?.Invoke(o, e);
        }

        private bool IsConnFalse()
        {
            _error += _conn.GetError();
            return false;
        }

        //private bool Logout()
        //{
        //    String login = _parameters.GetUserName();
        //    String packet = $"B\n{_arm}\nB\n{_id}\n{Seq}\n\n\n\n\n\n{login}";
        //    return _conn.SendPacket(packet);
        //}
    }
}
