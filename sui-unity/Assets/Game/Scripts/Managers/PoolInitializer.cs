using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Enemies;
using MoeBeam.Game.Scripts.Items;
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
        [SerializeField] private CoinSelector coinSelector;
        
        private void Start()
        {
            poolManager.Init();
            
            Register(bulletPrefab, 20);
            Register(batEnemy, 5);
            Register(orcEnemy, 5);
            Register(sniperEnemy, 5);
            Register(sniperBullet, 10);
            Register(coinSelector, 10);
        }
        
        private void Register<T>(T prefab, int amount, Action registerSuccess = null) where T : MonoBehaviour
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Prefab {nameof(prefab)} is null. Ignoring.");
                return;
            }
            poolManager.Register(prefab, amount, true);
            registerSuccess?.Invoke();
        }
        
    }
}