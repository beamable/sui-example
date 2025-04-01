using System;
using MoeBeam.Game.Scripts.Managers;
using MoeBeam.Game.Scripts.Player;
using UnityEngine;

namespace Game.Scripts.PickUp
{
    public class HealthPickUp : MonoBehaviour
    {
        [SerializeField] private int healthAmount = 25;
        [SerializeField] private AudioClip pickUpSound;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent<PlayerHealth>(out var playerHealth)) return;
            
            playerHealth.AddHealth(healthAmount);
            AudioManager.Instance.PlaySfx(pickUpSound);
            Destroy(this.gameObject);
        }
    }
}