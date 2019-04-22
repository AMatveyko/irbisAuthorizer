using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    internal interface IConnection : IDebug
    {
        List<String> AnswerLines { get; }
        bool SendPacket(String packetRaw);
    }
}
