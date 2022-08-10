using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using ZServer.Interfaces.RESTFUL;

namespace ZServer.Grains.RESTFUL
{
    public class Restful : Grain, IRestful
    {
        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return Task.CompletedTask;
        }

        public Task<bool> DeleteAsync(string workspace, string layer, object id)
        {
            return Task.FromResult(false);
        }
    }
}