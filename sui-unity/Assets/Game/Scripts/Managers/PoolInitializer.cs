using Cysharp.Threading.Tasks;
using Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Enemies.Spawner;
using MoeBeam.Game.Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Managers
{
    public class PoolInitializer : MonoBehaviour
    {
        [SerializeField] private GenericPoolManager poolManager;
    
        [Header("Prefabs")]
        [SerializeField] private Bullet bulletPrefab;
        [FormerlySerializedAs("redBlobEnemy")] [SerializeField] private BatEnemy batEnemy;
        [SerializeField] private OrcEnemy orcEnemy;
        [SerializeField] private SniperEnemy sniperEnemy;
        [SerializeField] private SniperBullet sniperBullet;

        private async void Start()
        {
            if(bulletPrefab != null)
                poolManager.Register(bulletPrefab, 20, true);

            if (batEnemy != null)
                poolManager.Register(batEnemy, 10, false);

            await UniTask.Yield(PlayerLoopTiming.LastTimeUpdate);
            
            if(orcEnemy != null)
                poolManager.Register(orcEnemy, 10, false);
            
            if(sniperEnemy != null)
                poolManager.Register(sniperEnemy, 10, false);
            
            await UniTask.Yield(PlayerLoopTiming.LastTimeUpdate);
            
            if(sniperBullet != null)
                poolManager.Register(sniperBullet, 10, false);
        }
    }
}