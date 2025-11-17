using System.Collections.Generic;
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu("Flare Engine/一Interactables/Zipline")]
        [RequireComponent(typeof(LineRenderer))]
        public class Zipline : MonoBehaviour
        {
                [SerializeField] private int stiffness = 10;
                [SerializeField] private float bounce = 0.1f;
                [SerializeField] private float upFriction = 0;
                [SerializeField] private bool createOnAwake;
                [SerializeField] private LineRenderer line;

                [SerializeField] private Stick[] stick;
                [SerializeField] private Particle[] particle;
                [SerializeField] private Rect rect = new Rect();

                [SerializeField, HideInInspector] private Vector3 endOffset;
                [SerializeField, HideInInspector] private int lines = 1;
                [SerializeField, HideInInspector] private float gravity = 0.01f;
                [SerializeField, HideInInspector] private float areaOffset = 5f;
                [SerializeField, HideInInspector] private float areaHeight = 10f;
                public static List<Zipline> zipLines = new List<Zipline>();

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool view = true;
                [SerializeField, HideInInspector] private bool foldOut = false;
                private void OnDrawGizmos ()
                {
                        if (particle == null || !view)
                                return;
                        Draw.GLStart();
                        for (int i = 0; i < particle.Length; i++)
                        {
                                Draw.GLCircle(new Vector2(particle[i].x, particle[i].y), 0.05f, Color.green, 2);
                                if (i < particle.Length - 1)
                                {
                                        Debug.DrawLine(particle[i].position, particle[i + 1].position, Color.yellow);
                                }
                        }
                        Draw.GLEnd();
                        Draw.Square(rect, Color.yellow);
                }
#pragma warning restore 0414
#endif
                #endregion

                #region Initialize
                private void Awake ()
                {
                        if (createOnAwake)
                        {
                                CreateZipLine();
                        }
                }

                private void OnEnable ()
                {
                        if (!zipLines.Contains(this))
                        {
                                zipLines.Add(this);
                        }
                }
                private void OnDisable ()
                {
                        if (zipLines.Contains(this))
                        {
                                zipLines.Remove(this);
                        }
                }

                public void CreateZipLine ()
                {
                        lines = lines < 1 ? 1 : lines;
                        Vector3 endPoint = transform.position + endOffset;
                        float distance = (transform.position - endPoint).magnitude;
                        Vector2 direction = (endPoint - transform.position) / (distance == 0 ? 1f : distance);
                        float plankLength = (distance / lines) - 0f;
                        int particles = lines + 1;

                        particle = new Particle[particles];
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i] = new Particle((Vector2) transform.position + direction * plankLength * i, -gravity, i == 0 || i == particle.Length - 1);
                                if (i == particle.Length - 1)
                                {
                                        particle[i].SetPosition(endPoint);
                                }
                        }

                        stick = new Stick[lines];
                        for (int i = 0; i < stick.Length; i++)
                        {
                                stick[i] = new Stick(i, i + 1, plankLength);
                        }

                        Vector2 p = transform.position;
                        rect = new Rect(p.x, p.y - areaOffset, Mathf.Abs(endPoint.x - p.x), areaHeight);

                        if (line != null)
                        {
                                line.positionCount = particles;
                        }
                }
                #endregion

                #region Physics
                private void FixedUpdate ()
                {
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i].FixedUpdate();
                        }
                        for (int i = 0; i < stiffness; i++)
                        {
                                for (int j = 0; j < stick.Length; j++)
                                {
                                        stick[j].FixedUpdate(particle);
                                }
                        }
                        if (line != null)
                        {
                                for (int i = 0; i < particle.Length; i++)
                                {
                                        line.SetPosition(i, particle[i].position);
                                }
                        }
                }
                #endregion

                #region Find
                public static void Find (AbilityManager player, Vector2 center, ZipInfo zip, ref Vector2 velocity)
                {
                        if (zipLines.Count == 0 || Time.deltaTime == 0)
                        {
                                return;
                        }

                        for (int i = zipLines.Count - 1; i >= 0; i--)
                        {
                                if (zipLines[i] != null && zipLines[i].enabled)
                                {
                                        zipLines[i].Ziplining(i, player, center, zip, ref velocity);
                                }
                        }

                        if (zip.exitMomentum)
                        {
                                if (zip.counter >= 1f || player.world.touchingASurface)
                                {
                                        zip.exitMomentum = false;
                                        zip.gravityMomentum = 0;
                                }
                                if (zip.exitMomentum)
                                {
                                        velocity.x = Compute.Lerp(zip.gravityMomentum, 0, 1f, ref zip.counter);
                                }
                        }
                }
                #endregion

                #region Player
                public void Ziplining (int index, AbilityManager player, Vector2 center, ZipInfo zip, ref Vector2 velocity)
                {
                        if (rect.Contains(center))
                        {
                                RunZipLineState(index, player, zip, ref velocity);
                        }
                        else if (zip.state.TryGetValue(index, out PlankState state) && state != PlankState.BeginSearch)
                        {
                                zip.state[index] = PlankState.BeginSearch;
                                if (zip.gravityMomentum != 0)
                                {
                                        zip.exitMomentum = true;
                                        zip.counter = 0;
                                }
                        }
                }

                private int LinePointingUp (int index)
                {
                        if (index >= 0 && index < stick.Length)
                        {
                                Vector2 start = particle[stick[index].first].position;
                                Vector2 end = particle[stick[index].second].position;
                                return end.y > start.y ? 1 : -1;
                        }
                        return 0;
                }

                private bool TetherIntersection (Vector2 characterPosition, bool longSearch, float characterHeight, float velY, out Vector2 intersectionPoint, out int index)
                {
                        index = 0;
                        intersectionPoint = Vector2.zero;
                        characterPosition.x = Mathf.Clamp(characterPosition.x, rect.x + 0.01f, rect.x + rect.width - 0.01f); // keep center in bounds so that character always intersects with a bridge plank
                        Vector2 characterTop = longSearch ? new Vector2(characterPosition.x, rect.max.y) : characterPosition;
                        Vector2 characterBottom = longSearch ? new Vector2(characterPosition.x, rect.min.y) : characterPosition - Vector2.up * characterHeight + Vector2.up * (velY * Time.deltaTime - 0.2f); // 0.2f (arbitrary) extend length to find bridge quicker and prevent jump/fall state

                        for (int i = 0; i < stick.Length; i++)
                        {
                                if (Compute.LineIntersection(characterTop, characterBottom, particle[stick[i].first].position, particle[stick[i].second].position, out intersectionPoint))
                                {
                                        index = i;
                                        return true;
                                }
                        }
                        return false;
                }

                private void RunZipLineState (int i, AbilityManager player, ZipInfo zip, ref Vector2 velocity)
                {
                        if (!zip.state.ContainsKey(i))
                        {
                                zip.state.Add(i, PlankState.BeginSearch);
                        }

                        WorldCollision world = player.world;
                        Vector2 velocityAdjust = world.box.right * velocity.x * Time.deltaTime;
                        Vector2 characterTop = world.box.topCenter + velocityAdjust + Vector2.up * zip.yOffset;

                        if (zip.state[i] == PlankState.Jumped)
                        {
                                if (world.onGround)
                                {
                                        zip.state[i] = PlankState.BeginSearch;
                                }
                        }
                        if (zip.state[i] == PlankState.BeginSearch)
                        {
                                if (TetherIntersection(characterTop, true, 0, 0, out Vector2 intersectionPoint, out int index)) //is character above or below current tether? If not, must pass threshold check before ziplining.
                                {
                                        zip.state[i] = characterTop.y > intersectionPoint.y ? PlankState.LerpToTether : zip.state[i];
                                }
                        }
                        if (zip.state[i] == PlankState.LerpToTether)
                        {
                                if (velocity.y > 0) // latch faster
                                {
                                        velocity.y -= Time.deltaTime * 30f;
                                }
                                if (zip.exitOnInput)
                                {
                                        zip.state[i] = PlankState.BeginSearch;
                                }
                                else if (velocity.y <= 0 && TetherIntersection(characterTop, false, world.box.sizeY * 0.3f, velocity.y, out Vector2 intersectionPoint, out int index))
                                {
                                        float distance = intersectionPoint.y - characterTop.y;

                                        if (distance < 0.2f)
                                        {
                                                stick[index].ApplyAcceleration(particle, Vector2.down * bounce * 1.25f);
                                                zip.Enter(player, i);
                                                zip.state[i] = PlankState.Latched;
                                                velocity.y = distance / Time.deltaTime;
                                        }
                                        else
                                        {
                                                velocity.y = distance / Time.deltaTime * 0.7f; // lerp here
                                        }
                                }
                        }
                        if (zip.state[i] == PlankState.Latched)
                        {
                                zip.AutoSlideToCenter(LinePointingUp(zip.zipIndex), player.speed, ref velocity, out bool goingUp);
                                float friction = goingUp && upFriction > 0 ? upFriction : 1f;
                                velocityAdjust = world.box.right * velocity.x * zip.zipSpeed * friction * Time.deltaTime;
                                characterTop = world.box.topCenter + velocityAdjust + Vector2.up * zip.yOffset;

                                if (TetherIntersection(characterTop, true, 0, 0, out Vector2 intersectionPoint, out int index))
                                {
                                        if (zip.exitOnInput)
                                        {
                                                zip.OnEndEvent(player);
                                                zip.gravityMomentum = 0;
                                                zip.state[i] = PlankState.Jumped;
                                        }
                                        else if (player.jumpButtonPressed) // Exit by jumping
                                        {
                                                zip.OnEndEvent(player);
                                                zip.gravityMomentum = 0;
                                                player.CheckForAirJumps();
                                                velocity.y = zip.jumpForce;
                                                stick[index].ApplyAcceleration(particle, Vector2.down * bounce);
                                                zip.state[i] = zip.canRelatch ? PlankState.BeginSearch : PlankState.Jumped;
                                        }
                                        else
                                        {
                                                zip.zipIndex = index;
                                                velocity.x *= zip.zipSpeed * friction;
                                                velocity.y = (intersectionPoint.y - characterTop.y) / Time.deltaTime;
                                                if (velocity.x != 0)
                                                {
                                                        stick[index].ApplyAcceleration(particle, Vector2.down * bounce * Mathf.Abs(velocity.x) * 0.03f);
                                                }
                                        }
                                }
                        }
                }
                #endregion
        }
}
