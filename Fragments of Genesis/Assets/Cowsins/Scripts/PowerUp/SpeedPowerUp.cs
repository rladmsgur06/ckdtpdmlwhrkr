using UnityEngine;

namespace cowsins2D
{
    public class SpeedPowerUp : PowerUp
    {
        [SerializeField] private float valueAdded;

        // this method gets called when the power up is triggered
        public override void TriggerAction(GameObject target)
        {
            PlayerMultipliers playerMultipliers = target.GetComponent<PlayerMultipliers>();
            if (playerMultipliers == null) return;
            playerMultipliers.speedModifier += valueAdded;
            base.TriggerAction(target);
        }
    }
}