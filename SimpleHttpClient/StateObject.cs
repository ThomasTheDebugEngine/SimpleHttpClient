using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpClient
{
    class StateObject : IStateObject
    {
        public const int BufferSize = 3072;
        public byte[] buffer { get; set; } = new byte[BufferSize];
        public StringBuilder sbuild { get; set; } = new StringBuilder();
        public Socket ClientSocket { get; set; } = null;
    }

    interface IStateObject //TODO this needs to be configured to work with dependency injection without thrwoing exceptions
    {
        const int BufferSize = 3072;
        public byte[] buffer { get; set; }
        public StringBuilder sbuild { get; set; }
        public Socket ClientSocket { get; set; }
    }
}
