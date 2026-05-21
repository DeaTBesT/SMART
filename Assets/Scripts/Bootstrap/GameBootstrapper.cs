using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bootstrap
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] _initializables;

        private async void Start()
        {
            await BootstrapAsync(this.GetCancellationTokenOnDestroy());
        }

        public async UniTask BootstrapAsync(CancellationToken cancellationToken)
        {
            foreach (var initializable in _initializables.OfType<IInitializable>())
            {
                await initializable.InitializeAsync(cancellationToken);
            }

            Debug.Log("Bootstrap complete");
        }
    }
}
