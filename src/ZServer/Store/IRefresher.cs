using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ZServer.Store;

public interface IRefresher
{
    Task Refresh(IConfiguration configuration);
}