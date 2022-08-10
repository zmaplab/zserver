using System.Threading.Tasks;
using Orleans;

namespace ZServer.Interfaces.RESTFUL
{
    public interface IRestful : IGrainWithStringKey, IRemindable
    {
        Task<bool> DeleteAsync(string workspace, string layer, object id);
    }
}