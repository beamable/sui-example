using System;
using DG.Tweening;
using UnityEngine;

namespace MoeBeam.Game.Scripts.Helpers
{
    public class DamageFlasher : MonoBehaviour
    {
        [ColorUsage(true, true), SerializeField] private Color flashColor;
        [SerializeField] private float flashDuration = 0.35f;
        
        [Header("References")]
        [SerializeField] private SpriteRenderer[] spriteRenderers;
        
        private Material[] _materials;
        
        private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            _materials = new Material[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                _materials[i] = spriteRenderers[i].material;
                _materials[i].SetFloat(FlashAmount, 0f);
            }
        }

        public void Flash()
        {
            foreach (var material in _materials)
            {
                material.SetColor(FlashColor, flashColor);
                material.DOFloat(1f, FlashAmount, 0f).OnComplete(() =>
                {
                    material.DOFloat(0f, FlashAmount, flashDuration).SetEase(Ease.InOutBack);
                });
            }
        }
    }
}