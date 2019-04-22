using irbisAuthorizer.Model;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace irbisAuthorizer
{
    internal class Connection : IConnection
    {

        private EventHandler _debugger;
        public void AddDebugDelegate(EventHandler debugger)
        {
            _debugger += debugger;
        }

        private Socket _socket;
        private List<String> _answerLines;
        public List<String> AnswerLines { get => _answerLines; }

        private Parameters _parameters;

        private String _error = "-> ModuleName: Connection,";
        private bool _debug = false;

        public string GetError()
        {
            return _error;
        }


        public Connection(Parameters parameters)
        {
            _parameters = parameters;
        }

        public bool SendPacket(String packetRaw)
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
                _debugger?.Invoke(null, new DebugEventArgs(debugMessages));
            }
            List<byte> data = new List<byte>();

            if (!Connect()) return false;
            int receiveBuffer = 512;
            int receive = receiveBuffer;
            _socket.Send(finalPacket);
            while (receive > 0)
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
                _debugger?.Invoke(null, new DebugEventArgs(debugMessages));
            }

            SplitAnswer(answer);
            String errorCode = GetErrorCode();
            if ((errorCode != "CLIENT_ALREADY_EXISTS") && (errorCode != "ZERO") && (errorCode != "RECORD ADDED"))
            {
                _error += $" SendPacket(): answer = \"{errorCode}\"";
                return false;
            }
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
            catch (Exception e)
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

        private void SplitAnswer(String answer)
        {
            _answerLines = new List<string>(answer.Split('\n'));
            for (int i = 0; i < _answerLines.Count; i++)
            {
                _answerLines[i] = _answerLines[i].Substring(0, _answerLines[i].Length - 1);
            }
        }

        private String GetErrorCode()
        {
            return IrbisErrors.GetErrorValue(_answerLines[10]);
        }
    }
}
