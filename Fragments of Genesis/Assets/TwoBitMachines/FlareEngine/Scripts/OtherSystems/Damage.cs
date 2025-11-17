using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class Damage : MonoBehaviour
        {
                //* make sure collider isTrigger is enabled
                [SerializeField] public LayerMask layer;
                [SerializeField] public AttackDirection direction;
                [SerializeField] public float damage = 1f;
                [SerializeField] public float force = 1f;

                private void OnTriggerEnter2D (Collider2D other)
                {
                        if (Compute.ContainsLayer (layer, other.gameObject.layer))
                        {
                                Health.IncrementHealth (transform, other.transform, -damage, Direction (transform, other.transform, direction) * force);
                        }
                }

                private void OnTriggerStay2D (Collider2D collider)
                {
                        if (Compute.ContainsLayer (layer, collider.gameObject.layer))
                        {
                                Health.IncrementHealth (transform, collider.transform, -damage, Direction (transform, collider.transform, direction) * force);
                        }
                }

                public static Vector3 Direction (Transform aiTransform, Transform transform, AttackDirection direction)
                {
                        if (direction == AttackDirection.AI_X_Direction)
                        {
                                return transform.position.x <= aiTransform.position.x ? -Vector2.right : Vector2.right;
                        }
                        if (direction == AttackDirection.Up)
                        {
                                return Vector3.up;
                        }
                        if (direction == AttackDirection.Down)
                        {
                                return Vector3.down;
                        }
                        if (direction == AttackDirection.Left)
                        {
                                return Vector3.left;
                        }
                        return Vector3.right;
                }
        }

        public enum AttackDirection
        {
                AI_X_Direction,
                Up,
                Down,
                Left,
                Right
        }
}