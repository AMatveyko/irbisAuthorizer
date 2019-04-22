using System;
using irbisAuthorizer;
using irbisAuthorizer.Model;

namespace Client
{
    class Program
    {
        private static String accessExample = "dotnet name.dll access readerId readerWiFiPwd";
        private static String writeLoginExample = "dotnet name.dll writelogin 007000-91 1.1.1.1 FF:FF:FF:FF:FF:FF clientIdent";
        private static String writeLogoutExample = "dotnet name.dll writelogout 007000-91 1.1.1.1 60 2048 1024 FF:FF:FF:FF:FF:FF TestScriptOOP";

        private static Parameters _parameters;

        private static IRadius _rd;

        private static Boolean _debug = true; 

        static void Main(string[] args)
        {

            if( !GetParameters() ) return;

            _rd = new Radius(_parameters);
            _rd.AddDebugDelegate( DebugMessage );

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "access":
                        {
                            if (args.Length == 3)
                                Access(args);
                            else
                            {
                                Console.WriteLine("Error args!!! Example:");
                                Console.WriteLine(accessExample);
                            }
                            break;
                        }
                    case "writelogin":
                        {
                            if (args.Length == 5)
                                WriteLogin(args);
                            else
                            {
                                Console.WriteLine("Error args!!! Example:");
                                Console.WriteLine(writeLoginExample);
                            }
                            break;
                        }
                    case "writelogout":
                        {
                            if (args.Length == 8)
                                WriteLogout(args);
                            else
                            {
                                Console.WriteLine("Error args!!! Example:");
                                Console.WriteLine(writeLogoutExample);
                            }
                            break;
                        }
                    default:
                        Help();
                        break;
                }
            }
            else
            {
                Help();
            }
            if (_debug)
                Console.ReadLine();
        }
        private static void Access(String[] args)
        {
            String rdrId = args[1];
            String rdrPwd = args[2];
            Console.Write(_rd.CheckLogPass(rdrId, rdrPwd));
        }
        private static void WriteLogin(String[] args)
        {
            String rdrId = args[1];
            String ipAddr = args[2];
            String macAddr = args[3];
            String clientIdent = args[4];
            if (!_rd.WriteAccountLoginInfo(rdrId, ipAddr, macAddr, clientIdent))
                Console.Write(_rd.GetError());
        }
        private static void WriteLogout(String[] args)
        {
            String rdrId = args[1];
            String ipAddr = args[2];
            String sessionTime = args[3];
            String inputBytes = args[4];
            String outputBytes = args[5];
            String macAddr = args[6];
            String clientIdent = args[7];
            if (!_rd.WriteAccountLogoutInfo(rdrId, ipAddr, sessionTime, inputBytes, outputBytes, macAddr, clientIdent))
                Console.Write(_rd.GetError());
        }
        private static void Help()
        {
            Console.WriteLine("Examples:");
            Console.WriteLine(accessExample);
            Console.WriteLine(writeLoginExample);
            Console.WriteLine(writeLogoutExample);
        }
        private static void DebugMessage(Object o, EventArgs e)
        {
            DebugEventArgs debugEventArgs = (DebugEventArgs)e;
            foreach (var str in debugEventArgs.GetMessages)
                Console.WriteLine(str);
        }
        private static bool GetParameters()
        {
            if (!AppConfig.GetParameters(out _parameters))
            {
                Console.WriteLine($" GetParameters {AppConfig.GetError()}");
                return false;
            }
            _debug = Converter.DebugStrToBool(_parameters.GetDebug());
            return true;
        }
    }

}
