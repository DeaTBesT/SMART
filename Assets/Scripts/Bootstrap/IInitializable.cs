using System.Threading;
using Cysharp.Threading.Tasks;

public interface IInitializable
{
    UniTask InitializeAsync(CancellationToken cancellationToken);
}
