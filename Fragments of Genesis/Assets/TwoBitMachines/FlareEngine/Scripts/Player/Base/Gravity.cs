using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class Gravity
        {
                [SerializeField] public float jumpHeight = 4f;
                [SerializeField] public float jumpTime = 0.75f;
                [SerializeField] public float multiplier = 1.5f;
                [SerializeField] public float terminalVelocity = 40f;

                public float gravity { get; private set; }
                public float jumpVelocity { get; private set; }
                public float gravityEffect { get; private set; }

                public void Initialize ()
                {
                        terminalVelocity = Mathf.Abs(terminalVelocity);
                        gravity = jumpTime == 0 ? 0 : -(2 * jumpHeight) / Mathf.Pow(jumpTime / 2f, 2f);
                        jumpVelocity = Mathf.Abs(gravity) * jumpTime / 2f;
                        gravityEffect = 0;
                        multiplier = multiplier <= 0 ? 1f : multiplier;
                }

                public void Execute (bool onSurface, ref Vector2 velocity)
                {
                        if (velocity.y < -terminalVelocity)
                        {
                                velocity.y = -terminalVelocity;
                        }
                        if (onSurface)
                        {
                                velocity.y = 0;
                        }
                        gravityEffect = velocity.y < 0 ? multiplier * gravity * Time.deltaTime : gravity * Time.deltaTime;
                        velocity.y += gravityEffect;
                }
        }
}
