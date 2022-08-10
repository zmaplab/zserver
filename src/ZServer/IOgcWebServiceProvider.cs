using System.Collections.Generic;

namespace ZServer
{
    public interface IOgcWebServiceProvider
    {
        HashSet<ServiceType> Services { get; }
    }
}