using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer
{
    public interface IDebug
    {
        string GetError();
        void AddDebugDelegate(EventHandler debugger);
    }
}
