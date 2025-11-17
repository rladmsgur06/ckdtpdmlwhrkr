using System.Collections.Generic;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("Flare Engine/一Interactables/Rope")]
        public class Rope : MonoBehaviour
        {
                [SerializeField] public Particle[] particle;
                [SerializeField] public float tetherRadius = 1;
                [SerializeField] public bool isClimbable;

                [SerializeField] private RopeType type;
                [SerializeField] private GameObject ropeEnd;
                [SerializeField] private Sprite ropeSprite;
                [SerializeField] private int stiffness = 5;
                [SerializeField] private float force = 0.05f;
                [SerializeField] private bool doubleAnchor;
                [SerializeField] private Vector2 endOffset;
                [SerializeField] private Vector2 tetherSize = new Vector2 (1f, 1f);

                [SerializeField] private float ropeRadius = 5f;
                [SerializeField] private float plankLength;
                [SerializeField] private Vector2 searchPoint;
                [SerializeField] private Stick[] stick;
                [SerializeField] private List<Tether> tethers = new List<Tether> ( );

                public static List<Rope> ropes = new List<Rope> ( );
                public Particle ropeHandle => particle[particle.Length - 1];
                public int last => particle.Length - 1;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool view = true;
                [SerializeField] private bool foldOut = false;
                [SerializeField] private int segments = 3; //tethers
                [SerializeField] private float gravity = 0.05f;

                public void CreateRope ( )
                {
                        Vector2 startPosition = transform.position;
                        Vector2 direction = endOffset; // Same
                        float length = direction.magnitude;
                        direction = direction.normalized;
                        segments = segments < 1 ? 1 : segments;
                        plankLength = length / segments;
                        int particles = segments + 1;
                        gravity = Mathf.Abs (gravity);
                        searchPoint = startPosition + endOffset * 0.5f;

                        particle = new Particle[particles];
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i] = new Particle (startPosition + direction * plankLength * i, -gravity, i == 0);
                                if (particle.Length - 1 == i)
                                {
                                        particle[i].Set (startPosition + endOffset, -gravity, doubleAnchor);
                                }
                        }

                        stick = new Stick[segments];
                        for (int i = 0; i < stick.Length; i++)
                        {
                                stick[i] = new Stick (i, i + 1, plankLength);
                        }
                        CreateRopeGameObjects (direction);
                }

                private void CreateRopeGameObjects (Vector2 direction)
                {
                        if (ropeSprite == null)
                        {
                                Debug.LogWarning ("Rope requires a tether sprite.");
                                return;
                        }

                        GameObject gameObject = new GameObject ( );
                        gameObject.name = "Tether";
                        gameObject.transform.parent = this.transform;
                        gameObject.transform.localScale = tetherSize;
                        gameObject.AddComponent<Tether> ( );
                        gameObject.AddComponent<SpriteRenderer> ( ).sprite = ropeSprite;

                        for (int i = 0; i < tethers.Count; i++)
                        {
                                if (tethers[i] == null || (ropeEnd != null && ropeEnd == tethers[i].gameObject)) continue;
                                if (i == 0)
                                {
                                        SpriteRenderer renderer = tethers[i].gameObject.GetComponent<SpriteRenderer> ( );
                                        if (renderer != null) gameObject.GetComponent<SpriteRenderer> ( ).color = renderer.color;
                                }
                                DestroyImmediate (tethers[i].gameObject);
                        }

                        tethers.Clear ( );
                        tethers.Add (gameObject.GetComponent<Tether> ( ));

                        for (int i = 1; i < segments; i++)
                        {
                                GameObject newPlank = Instantiate (gameObject, transform.position, Quaternion.identity, transform);
                                newPlank.name = "Tether_" + (i + 1).ToString ( );
                                tethers.Add (newPlank.GetComponent<Tether> ( ));
                        }

                        if (ropeEnd != null && tethers.Count > 1)
                        {
                                if (ropeEnd.GetComponent<Tether> ( ) == null) ropeEnd.AddComponent<Tether> ( );
                                DestroyImmediate (tethers[tethers.Count - 1].gameObject);
                                tethers.RemoveAt (tethers.Count - 1);
                                tethers.Add (ropeEnd.GetComponent<Tether> ( ));
                        }

                        for (int i = 0; i < tethers.Count; i++)
                        {
                                tethers[i].transform.position = transform.position + (Vector3) direction * plankLength * i;
                                tethers[i].transform.localScale = gameObject.transform.localScale;
                                tethers[i].RopeRotate (particle, stick[i].first, stick[i].second);
                                tethers[i].particleIndexA = stick[i].first;
                                tethers[i].particleIndexB = stick[i].second;
                        }
                }

                private void OnDrawGizmos ( )
                {
                        if (particle == null || !view) return;

                        Draw.GLStart ( );
                        if (type == RopeType.Idle)
                        {
                                Draw.GLCircle (searchPoint, ropeRadius, Color.yellow, (int) ropeRadius * 2);
                        }
                        for (int i = 0; i < particle.Length; i++)
                        {
                                Draw.GLCircle (new Vector2 (particle[i].x, particle[i].y), 0.05f, Color.green, 2);
                                if (i < particle.Length - 1)
                                {
                                        Debug.DrawLine (particle[i].position, particle[i + 1].position, Color.yellow);
                                }
                        }
                        if (type == RopeType.Swing && particle.Length > 0)
                        {
                                Draw.GLCircle (particle[particle.Length - 1].position, tetherRadius, Color.yellow, 4);
                        }
                        Draw.GLEnd ( );
                }

                #pragma warning restore 0414
                #endif
                #endregion

                private void OnEnable ( )
                {
                        if (!ropes.Contains (this))
                        {
                                ropes.Add (this);
                        }
                }

                private void OnDisable ( )
                {
                        if (ropes.Contains (this))
                        {
                                ropes.Remove (this);
                        }
                }

                private void FixedUpdate ( )
                {
                        if (particle == null || particle.Length == 0)
                        {
                                return;
                        }

                        particle[0].SetPosition (transform.position);
                        if (doubleAnchor)
                        {
                                particle[last].SetPosition (transform.position + (Vector3) endOffset);
                        }
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i].FixedUpdate ( );
                        }
                        for (int i = 0; i < stiffness; i++)
                        {
                                for (int j = 0; j < stick.Length; j++)
                                {
                                        stick[j].FixedUpdate (particle);
                                }
                        }
                        for (int i = 0; i < tethers.Count; i++)
                        {
                                tethers[i].RopeRotate (particle, stick[i].first, stick[i].second);
                        }
                }

                public void ResetAll ( )
                {
                        Vector2 startPosition = transform.position;
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i].SetPosition (startPosition + Vector2.down * plankLength * i);
                        }
                }

                public void UnlatchEndAnchor ( )
                {
                        if (particle.Length > 0)
                        {
                                particle[last].anchor = false;
                        }

                        doubleAnchor = false;
                        searchPoint = transform.position; //                   Set search radius to start position
                }

                public static void ResetRopes ( )
                {
                        for (int i = ropes.Count - 1; i >= 0; i--)
                        {
                                ropes[i]?.ResetAll ( );
                        }
                }

                public static bool HoldPoint (RopeSwing ropeSwing, Particle[] particle, Vector2 center, Vector2 right, float radius, ref Vector2 newHoldPoint)
                {
                        Vector2 pointLeft = center - right * radius;
                        Vector2 pointRight = center + right * radius;

                        for (int i = 1; i < particle.Length; i++)
                        {
                                if (Compute.LineIntersection (particle[i].position, particle[i - 1].position, pointLeft, pointRight, out Vector2 intersect))
                                {
                                        ropeSwing.particle1 = i;
                                        ropeSwing.particle2 = i - 1;
                                        ropeSwing.grabDistance = (intersect - particle[i].position).magnitude;
                                        newHoldPoint = intersect;
                                        return true;
                                }
                        }
                        return false;
                }

                public static bool Find (Vector2 characterCenter, Vector2 noneGravityVel, RopeSwing ropeSwing) // Velocity that doesn't include the effect of gravity
                {
                        for (int i = ropes.Count - 1; i >= 0; i--)
                        {
                                if (ropes[i] == null)
                                {
                                        ropes.RemoveAt (i);
                                }
                                else if (ropes[i].Find (ropeSwing, characterCenter, noneGravityVel))
                                {
                                        ropeSwing.rope = ropes[i];
                                        return ropeSwing.rope != ropeSwing.oldRope;
                                }
                        }
                        return false;
                }

                public bool Find (RopeSwing ropeSwing, Vector2 characterCenter, Vector2 noneGravityVel)
                {
                        if (type == RopeType.Swing)
                        {
                                return LatchToRope (ropeSwing, characterCenter);
                        }
                        else
                        {
                                PushThroughRope (characterCenter, noneGravityVel);
                                return false;
                        }
                }

                private bool LatchToRope (RopeSwing ropeSwing, Vector2 center)
                {
                        if (!isClimbable)
                        {
                                return (ropeHandle.position - center).sqrMagnitude <= tetherRadius * tetherRadius;
                        }
                        else
                        {
                                return HoldPoint (ropeSwing, particle, center, Vector2.right, tetherRadius, ref center); // ref center is dummy variable
                        }
                }

                private void PushThroughRope (Vector2 center, Vector2 noneGravityVel)
                {
                        if ((searchPoint - center).sqrMagnitude <= ropeRadius * ropeRadius) // within distance
                        {
                                int skipLast = doubleAnchor ? 1 : 0;
                                for (int i = 1; i < particle.Length - skipLast; i++) // Start at one since first is always an anchor
                                {
                                        Vector2 position = particle[i].position;
                                        if ((position - center).sqrMagnitude <= tetherRadius * tetherRadius)
                                        {
                                                particle[i].ApplyAcceleration (noneGravityVel.normalized * force);
                                        }
                                }
                        }
                }

                public void ApplyImpactAtEnd (float directionX, float impact)
                {
                        ropeHandle.ApplyAcceleration (Vector2.right * directionX * impact);
                }

                public void ApplyImpact (int indexA, int indexB, float impact, Vector2 direction)
                {
                        if (indexA < particle.Length) particle[indexA].ApplyAcceleration (direction * impact);
                        if (indexB < particle.Length) particle[indexB].ApplyAcceleration (direction * impact);
                }

        }

        public enum RopeType
        {
                Swing,
                Idle
        }
}