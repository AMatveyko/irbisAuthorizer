using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace irbisAuthorizer
{
    public class DbClient
    {

        public EventHandler Debugger;

        private String _error = "-> ModuleName: DbClient,";
        public String GetError() => _error;

        private Parameters _parameters;
        private Socket _socket;

        private List<String> _splitedAnswer;

        private static int _correct = 0;
        private static Char _arm = 'C';
        private static int _seq = 0;
        private static int Seq { get => ++_seq; }
        private static int _id = 388456;

        private bool _debug = false;

        public DbClient() { }


        private bool GetParameters()
        {
            if(!AppConfig.GetParameters(out _parameters))
            {
                _error += $" GetParameters {AppConfig.GetError()}";
                return false;
            }
            _debug = (_parameters.GetDebug() == "1");
            return true;
        }

        private bool Connect()
        {
            if (_parameters == null)
            {
                _error += " Connect(): parameters = null!!!";
                return false;
            }
            try
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(_parameters.GetHost(), _parameters.GetPort());
            }
            catch(Exception e)
            {
                _error += $" Connect(): {e.Message}";
                return false;
            }
            return true;
        }
        private bool Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            return true;
        }

        private bool SendPacket(String packetRaw)
        {

            byte[] packetToByte = Encoding.UTF8.GetBytes(packetRaw);
            byte[] lengthPacket = Encoding.UTF8.GetBytes($"{packetToByte.Length}\n");
            byte[] finalPacket = new byte[lengthPacket.Length + packetToByte.Length];
            Array.Copy(lengthPacket, finalPacket, lengthPacket.Length);
            Array.Copy(packetToByte, 0, finalPacket, lengthPacket.Length, packetToByte.Length);

            if (_debug)
            {
                String packet = $"{finalPacket.Length}\n{packetRaw}";
                List<String> debugMessages = new List<String>()
                {
                    "------------------------",
                    "Start Packet:",
                    $"{packet}|<-End Packet",
                    "------------------------"
                };
                Debugger?.Invoke(null, new DebugEventArgs(debugMessages));
            }
            List<byte> data = new List<byte>();

            if (!Connect()) return false;
            int receiveBuffer = 512;
            int receive = receiveBuffer;
            _socket.Send(finalPacket);
            while(receive > 0)
            {
                byte[] buffer = new byte[receiveBuffer];
                receive = _socket.Receive(buffer);
                if (receive == receiveBuffer)
                    data.AddRange(buffer);
                else
                    for (int i = 0; i <= receive; i++)
                        data.Add(buffer[i]);
            }
            byte[] dataArr = data.ToArray();
            Disconnect();

            String answer = Encoding.UTF8.GetString(dataArr);

            if (_debug)
            {
                List<String> debugMessages = new List<String>()
                {
                    "------------------------",
                    "Start Answer:",
                    $"{answer}|<-End Answer",
                    "------------------------"
                };
                Debugger?.Invoke(null, new DebugEventArgs(debugMessages));
            }

            SplitAnswer(answer);
            String errorCode = GetErrorCode();
            if ( (errorCode != "CLIENT_ALREADY_EXISTS") && (errorCode != "ZERO") && (errorCode != "RECORD ADDED") )
            {
                _error += $" SendPacket(): answer = \"{errorCode}\"";
                return false;
            }
            return true;
        }
        private void SplitAnswer(String answer)
        {
            _splitedAnswer = new List<string>( answer.Split('\n') );
            for (int i = 0; i < _splitedAnswer.Count; i++)
            {
                _splitedAnswer[i] = _splitedAnswer[i].Substring(0, _splitedAnswer[i].Length - 1);
            }
        }
        private String GetErrorCode()
        {
            return IrbisErrors.GetErrorValue(_splitedAnswer[10]);
        }

        private bool Login()
        {
            String login = _parameters.GetUserName();
            String passwd = _parameters.GetPassword();

            String packet = $"A\n{_arm}\nA\n{_id}\n{Seq}\n\n\n\n\n\n{login}\n{passwd}";
            if (!SendPacket(packet)) return false;
            return true;
        }
        private bool FindReader(String idRdr, String pwdRdr)
        {
            
            String db = _parameters.GetDbName();
            String fn = _parameters.GetNameFieldNumber();
            String fp = _parameters.GetPwdFieldNumber();
            String packet = $"K\n{_arm}\nK\n{_id}\n{Seq}\n\n\n\n\n\n{db}\n\"A=$\"\n1000\n1\n@brief\n\n\n!if ((v{fn}='{idRdr}')*(v{fp}='{pwdRdr}')) then '1' else '0' fi";
            SendPacket(packet);
            return true;
        }
        private bool GetMfn(String idRdr, out String mfnRdr)
        {
            mfnRdr = String.Empty;
            String db = _parameters.GetDbName();
            String fn = _parameters.GetNameFieldNumber();
            String packet = $"K\n{_arm}\nK\n{_id}\n{Seq}\n\n\n\n\n\n{db}\n\"A=$\"\n1000\n1\n@brief\n\n\n!if (v{fn} = '{idRdr}') then '1' else '0' fi";
            if (!SendPacket(packet)) return false;
            if (_splitedAnswer[11] == "1")
            {
                Regex regex = new Regex("([0-9]{1,10})#");
                Match match = regex.Match(_splitedAnswer[12]);
                mfnRdr = (match.Groups)[1].Value;
                return true;
            }
            else
            {
                _error += "GetMfn(): MFN not found!";
                return false;
            }
        }
        private bool ReadRdrRecord(String mfnRdr)
        {
            String lockRdr = "0";
            String db = _parameters.GetDbName();
            String packet = $"C\n{_arm}\nC\n{_id}\n{Seq}\n\n\n\n\n\n{db}\n{mfnRdr}\n{lockRdr}";
            if (!SendPacket(packet)) return false;
            return true;
        }
        private bool WriteRdrRecord(String mfnRdr, String ipAddr, String message, String clientIdent, String macAddr)
        {
            String date;
            String time;
            String db = _parameters.GetDbName();
            String lockRec = "0";
            String ifUpdate = "1";
            GetDateTime(out date, out time);
            _splitedAnswer.Add($"40#^X{ipAddr}^D{date}^C{message}^V{clientIdent}^U{macAddr}^1{time}^2{time}");

            byte[] separatorCode = new byte[] { 31, 30 };
            String separatorStr = Encoding.UTF8.GetString(separatorCode);
            String packet = $"D\n{_arm}\nD\n{_id}\n{Seq}\n\n\n\n\n\n{db}\n{lockRec}\n{ifUpdate}\n{mfnRdr}#0{separatorStr}";

            for (int i = 12; i < _splitedAnswer.Count; i++)
            {
                if (_splitedAnswer[i].Length > 1)
                {
                    packet += _splitedAnswer[i] + separatorStr;
                    _correct += 2;
                }
            }
            if(_debug)
                Console.WriteLine(packet);
            if (!SendPacket(packet)) return false;
            return true;
        }
        private void GetDateTime(out String date, out String time)
        {
            String dateTime = DateTime.Now.ToString("yyyyMMdd|HHmmss");
            String[] tmpArr = dateTime.Split('|');
            date = tmpArr[0];
            time = tmpArr[1];
        }
        private bool Logout()
        {
            String login = _parameters.GetUserName();
            String packet = $"B\n{_arm}\nB\n{_id}\n{Seq}\n\n\n\n\n\n{login}";
            return SendPacket(packet);
        }

        private bool WriteAccountInfoFromRadius(String idRdr, String ipAddr, String macAddr, String clientIdent, String message)
        {
            String mfnRdr;
            if (!GetParameters()) return false;
            if (!Login()) return false;
            if (!GetMfn(idRdr, out mfnRdr)) return false;
            if (!ReadRdrRecord(mfnRdr)) return false;
            if (!WriteRdrRecord(mfnRdr, ipAddr, message, clientIdent, macAddr)) return false;
            return true;
        }
        public bool WriteAccountLoginInfoFromRadius(String idRdr, String ipAddr, String macAddr, String clientIdent)
        {
            String message = $"Wi_Fi(Вход)";
            return WriteAccountInfoFromRadius(idRdr, ipAddr, macAddr, clientIdent, message);
        }
        public bool WriteAccountLogoutInfoFromRadius(String idRdr, String ipAddr, String sessionTime, String inputByte, String outputByte, String macAddr, String clientIdent)
        {
            String message = $"Wi_Fi(Выход){sessionTime}с";
            return WriteAccountInfoFromRadius(idRdr, ipAddr, macAddr, clientIdent, message);
        }
        public String CheckLogPassFromRadius(String idRdr, String pwdRdr)
        {
            String acceptAnswer = "Accept";
            String rejectAnswer = "Reject";
            if (!GetParameters()) return String.Format($"{rejectAnswer}: {_error}");
            if (!Login()) return String.Format($"{rejectAnswer}: {GetError()}");
            FindReader(idRdr, pwdRdr);

            String resolution = _splitedAnswer[11];

            if (resolution == "1")
                return acceptAnswer;
            else
            {
                return rejectAnswer;
            }
        }
    }
}
