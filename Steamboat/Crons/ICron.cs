using System.Threading;
using System.Threading.Tasks;

namespace Steamboat.Crons
{
    internal interface ICron
    {
        public Task Update(CancellationToken cancellationToken);
    }
}