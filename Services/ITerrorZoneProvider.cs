using System.Threading;
using System.Threading.Tasks;
using D2RTerrorZone.Models;

namespace D2RTerrorZone.Services
{
    public interface ITerrorZoneProvider
    {
        Task<TerrorZoneData> GetCurrentAsync(CancellationToken cancellationToken);
    }
}
