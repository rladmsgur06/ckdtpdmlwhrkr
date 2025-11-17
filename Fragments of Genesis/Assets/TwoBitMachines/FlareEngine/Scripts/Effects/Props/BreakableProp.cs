using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class BreakableProp : PropJump
        {
                #pragma warning disable 0108
                [SerializeField] public Collider2D collider2D;
                [SerializeField] public SpriteRenderer renderer;
                #pragma warning restore 0108
                [SerializeField] public UnityEventEffect onBreak = new UnityEventEffect ( );
                [SerializeField] public List<Rigidbody2D> list = new List<Rigidbody2D> ( );
                [System.NonSerialized] private List<Vector3> resetPoint = new List<Vector3> ( );

                private void Start ( )
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] == null) continue;
                                Vector3 localPosition = list[i].transform.localPosition;
                                resetPoint.Add (localPosition);
                        }
                }

                public void Reset ( )
                {
                        if (rigidBody != null)
                        {
                                rigidBody.isKinematic = false;
                                rigidBody.constraints = RigidbodyConstraints2D.None;
                        }
                        if (collider2D != null) collider2D.enabled = true;
                        if (renderer != null) renderer.enabled = true;

                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] == null) continue;
                                list[i].transform.localEulerAngles = Vector3.zero;
                                list[i].transform.localPosition = resetPoint[i];
                                list[i].gameObject.SetActive (false);
                        }
                }

                public void Break (ImpactPacket impact)
                {
                        if (rigidBody != null)
                        {
                                rigidBody.isKinematic = true;
                                rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
                        }
                        if (collider2D != null) collider2D.enabled = false;
                        if (renderer != null) renderer.enabled = false;
                        onBreak.Invoke (impact);

                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i] == null) continue;
                                list[i].gameObject.SetActive (true);
                                float variance = Random.Range (0.75f, 1.75f);
                                float signX = Mathf.Sign (impact.direction.x);
                                list[i].AddForce (Vector3.right * signX * moveForce * variance, ForceMode2D.Impulse);
                                list[i].AddForce (Vector3.up * jumpForce * variance, ForceMode2D.Impulse);
                                list[i].AddTorque (torqueAngle * Mathf.Deg2Rad * -signX, ForceMode2D.Impulse);
                        }
                }
        }
}