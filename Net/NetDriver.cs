using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poly.Net
{
    public class NetDriver : ANetBase
    {
        public NetDriver(int hostId, NetSettings netSettings, ITransport transport) : base(hostId, netSettings, transport)
        {
        }
    }
}
