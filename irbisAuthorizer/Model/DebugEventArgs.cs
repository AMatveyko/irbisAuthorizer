using System;
using System.Collections.Generic;
using System.Text;

namespace irbisAuthorizer.Model
{
    public class DebugEventArgs : EventArgs
    {
        private List<String> _debugMessages;

        public List<String> GetMessages { get => _debugMessages; }

        public DebugEventArgs(List<String> debugMessages)
        {
            _debugMessages = debugMessages;
        }
        public DebugEventArgs(String debugMessages)
        {
            _debugMessages = new List<string>() { debugMessages };
        }
    }
}
