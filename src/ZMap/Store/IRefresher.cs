using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ZMap.Store;

public interface IRefresher
{
    Task Refresh(IEnumerable<IConfiguration> configurations);
}