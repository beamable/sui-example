using MoeBeam.Game.Scripts.Enemies;
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

        private void Start()
        {
            if(bulletPrefab != null)
                poolManager.Register(bulletPrefab, 20, true);

            if(batEnemy != null)
                poolManager.Register(batEnemy, 10, false);
        }
    }
}