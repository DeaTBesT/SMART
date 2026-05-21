using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bootstrap
{
    public interface IInitializable
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
