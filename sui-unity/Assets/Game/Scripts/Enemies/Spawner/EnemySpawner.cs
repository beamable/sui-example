using System.Collections;
using System.Collections.Generic;
using MoeBeam.Game.Scripts.Data;
using MoeBeam.Game.Scripts.Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace MoeBeam.Game.Scripts.Enemies.Spawner
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Spawner Settings")] 
        [SerializeField] private BaseEnemy[] enemyPrefabs; 
        [SerializeField] private BaseEnemy finalBossPrefab; 
        [SerializeField] private Transform bossSpawnPoint; 

        [Header("Spawn Areas")] 
        [SerializeField] private BoxCollider2D spawnArea1; 
        [SerializeField] private BoxCollider2D spawnArea2; 

        [Header("Spawn Control")] 
        [SerializeField] private int totalEnemiesToSpawn = 250;
        [SerializeField] private int maxEnemiesAtOnce = 10;
        [SerializeField] private float spawnInterval = 3f; 

        [Header("MiniBoss Settings")] 
        [SerializeField] private int miniBossInterval = 15;
        [SerializeField] private float miniBossChance = 0.05f; 
        
        private int _enemiesSpawned = 0;
        private int _enemiesAlive = 0;
        private bool _playerDied = false;
        private List<BaseEnemy> _spawnedEnemies = new();


        private void Start()
        {
            StartCoroutine(SpawnEnemies());
            EventCenter.Subscribe(GameData.OnPlayerDiedEvent, OnPlayerDied);
            EventCenter.Subscribe(GameData.OnEnemyDiedEvent, EnemyDied);
        }

        private void OnPlayerDied(object obj)
        {
            _playerDied = true;
        }

        private IEnumerator SpawnEnemies()
        {
            yield return new WaitUntil(()=>GameManager.Instance.HasGameStarted);
            while (_enemiesSpawned < totalEnemiesToSpawn)
            {
                if(_playerDied) yield break;
                if (_enemiesAlive < maxEnemiesAtOnce)
                {
                    if (_enemiesSpawned == totalEnemiesToSpawn - 1) // Last enemy is Boss
                    {
                        SpawnBoss();
                        yield break;
                    }
                    else
                    {
                        SpawnEnemy();
                    }
                }

                yield return new WaitForSeconds(spawnInterval);
            }
        }

        private void SpawnEnemy()
        {
            var spawnZone = Random.value > 0.5f ? spawnArea1.transform : spawnArea2.transform;
            var spawnLocation = GetRandomPointInArea(spawnZone);

            BaseEnemy enemyPrefab;
            enemyPrefab = _enemiesSpawned < 15 ? enemyPrefabs[0] :
                // First 15 are always Enemy 0
                enemyPrefabs[Random.Range(0, enemyPrefabs.Length)]; 

            var enemyInstance = GenericPoolManager.Instance.Get(enemyPrefab, spawnLocation);
            _enemiesSpawned++;
            _enemiesAlive++;

            enemyInstance.OnInit();
            
            // Handle MiniBoss logic after 15th enemy
            if (_enemiesSpawned > miniBossInterval && Random.value < miniBossChance)
            {
                enemyInstance.SetMiniBoss(true);
            }

            _spawnedEnemies.Add(enemyInstance);
        }

        private void SpawnBoss()
        {
            //TODO: add some sort of effects or animations
            var bossInstance = Instantiate(finalBossPrefab, bossSpawnPoint.position, Quaternion.identity);
            bossInstance.OnInit();
        }

        private void EnemyDied(object obj)
        {
            if(obj is not BaseEnemy enemy) return;
            enemy.SetMiniBoss(false);
            _enemiesAlive--;
            _spawnedEnemies.Remove(enemy);
            GenericPoolManager.Instance.Return(enemy);
        }

        /// <summary>
        /// Gets a random point inside a BoxCollider2D area.
        /// </summary>
        private Vector2 GetRandomPointInArea(Transform areaTransform)
        {
            var boxCollider = areaTransform.GetComponent<BoxCollider2D>();
            if (boxCollider == null)
            {
                Debug.LogError("No BoxCollider2D found on spawn area: " + areaTransform.name);
                return areaTransform.position; // Fallback to the center
            }

            var bounds = boxCollider.bounds;
            var x = Random.Range(bounds.min.x, bounds.max.x);
            var y = Random.Range(bounds.min.y, bounds.max.y);
            return new Vector2(x, y);
        }
    }
}