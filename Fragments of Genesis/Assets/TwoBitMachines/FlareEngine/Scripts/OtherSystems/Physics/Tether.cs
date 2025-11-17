using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        public class Tether : MonoBehaviour
        {
                [SerializeField] public float impact = 1f;
                [SerializeField, HideInInspector] public int particleIndexA;
                [SerializeField, HideInInspector] public int particleIndexB;
                [System.NonSerialized] private Rope rope;

                public void BridgeRotate (Vector2 position, Vector2 end, float plankOffset)
                {
                        Vector2 dir = end - position;
                        Vector2 mid = dir / 2f + position;
                        transform.position = mid + Vector2.down * plankOffset;
                        transform.rotation = Quaternion.FromToRotation (Vector3.right, dir);
                }

                public void RopeRotate (Particle[] list, int first, int second)
                {
                        Particle a = list[first];
                        Particle b = list[second];
                        Vector2 dir = b.position - a.position;
                        transform.position = dir / 2f + a.position;
                        transform.rotation = Quaternion.FromToRotation (Vector3.up, dir);
                }

                public void HookRotate (Vector2 position, Vector2 end, Vector2 offset)
                {
                        // Vector2 dir = end - position;
                        // transform.position = position + Vector2.right * offset.x + Vector2.up * offset.y; // this needs to use dir.normal instead of Vector2
                        // transform.rotation = Quaternion.FromToRotation (Vector3.up, dir);
                }

                public void ApplyImpact (float value, Vector2 direction)
                {
                        if (rope == null) rope = this.transform.parent.GetComponent<Rope> ( );
                        if (rope != null) rope.ApplyImpact (particleIndexA, particleIndexB, impact, direction);
                }

                public void ApplyImpact (Vector2 direction)
                {
                        if (rope == null) rope = this.transform.parent.GetComponent<Rope> ( );
                        if (rope != null) rope.ApplyImpact (particleIndexA, particleIndexB, impact, direction);
                }
        }
}