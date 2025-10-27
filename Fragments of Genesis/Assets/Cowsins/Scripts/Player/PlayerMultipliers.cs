using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class PlayerMultipliers : MonoBehaviour
    {
        [SerializeField] private UnityEvent OnBecomeInvincible, OnStopInvincibility;
        [HideInInspector] public float speedModifier = 1;
        [HideInInspector] public float damageModifier = 1;
        [HideInInspector] public float jumpHeightModifier = 1;
        [HideInInspector] public float damageReceivedModifier = 1;
        [HideInInspector] public float healingReceivedModifier = 1;
        [HideInInspector] public bool invincible = false;

        public void StartInvincibility() => invincible = true;

        public void StopInvincibility() => invincible = false;
    }
}