
using UnityEngine;
namespace cowsins2D
{

    public class InvincibilityPowerUp : PowerUp
    {
        [SerializeField] private float invincibilityDuration;


        // this method gets called when the power up is triggered
        public override void TriggerAction(GameObject target)
        {
            PlayerMultipliers player = target.GetComponent<PlayerMultipliers>();
            player.StartInvincibility();
            player.Invoke(nameof(player.StopInvincibility), invincibilityDuration);
            base.TriggerAction(target);
        }

    }
}