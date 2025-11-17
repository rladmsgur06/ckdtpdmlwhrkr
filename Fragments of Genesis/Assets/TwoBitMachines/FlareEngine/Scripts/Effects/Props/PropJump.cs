using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class PropJump : Impact
        {
                [SerializeField] public bool activeOnEnable;
                [SerializeField] public float jumpForce = 5f;
                [SerializeField] public float moveForce = 2f;
                [SerializeField] public float torqueAngle = 90f;
                [SerializeField] public Rigidbody2D rigidBody;

                public void OnEnable ( )
                {
                        if (activeOnEnable)
                        {
                                Activate ( );
                        }
                }

                public override void Activate (ImpactPacket impact)
                {
                        Activate (impact.direction.x);
                }

                public void Activate ( )
                {
                        Activate (Random.Range (-1, 1));
                }

                public void Activate (float directionX)
                {
                        if (rigidBody == null) return;
                        SetRigidbodyDynamic ( );
                        float variance = Random.Range (0.75f, 1.25f);
                        float signX = Mathf.Sign (directionX);
                        rigidBody.AddForce (Vector3.right * signX * moveForce * variance, ForceMode2D.Impulse);
                        rigidBody.AddForce (Vector3.up * jumpForce * variance, ForceMode2D.Impulse);
                        rigidBody.AddTorque (torqueAngle * variance * Mathf.Deg2Rad * -signX, ForceMode2D.Impulse);
                }

                public void RemoveAllForces ( )
                {
                        if (rigidBody == null) return;
                        rigidBody.velocity = Vector2.zero;
                        rigidBody.angularVelocity = 0f;
                }

                public void SetRigidbodyDynamic ( )
                {
                        if (rigidBody == null || rigidBody.bodyType == RigidbodyType2D.Dynamic) return;
                        rigidBody.bodyType = RigidbodyType2D.Dynamic;
                }

                public void SetRigidbodyKinematic ( )
                {
                        if (rigidBody == null || rigidBody.bodyType == RigidbodyType2D.Kinematic) return;
                        rigidBody.bodyType = RigidbodyType2D.Kinematic;
                }

                public void SetRigidbodyStatic ( )
                {
                        if (rigidBody == null || rigidBody.bodyType == RigidbodyType2D.Static) return;
                        rigidBody.bodyType = RigidbodyType2D.Static;
                }
        }
}