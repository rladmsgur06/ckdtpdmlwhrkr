using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("Flare Engine/一Interactables/Bridge")]
        public partial class Bridge : MonoBehaviour //* Has partial class BridgeState, no signals
        {
                [SerializeField] private int stiffness = 5;
                [SerializeField] private float bounce = 0.5f;
                [SerializeField] private float plankOffset;
                [SerializeField] private bool createOnAwake;
                [SerializeField] private Sprite plankSprite;

                [SerializeField] private Stick[] stick;
                [SerializeField] private Particle[] particle;
                [SerializeField] private Rect rect = new Rect ( );
                [SerializeField] private List<Tether> plankList = new List<Tether> ( );

                [SerializeField, HideInInspector] private Vector3 endOffset;
                [SerializeField, HideInInspector] private int planks = 2;
                [SerializeField, HideInInspector] private float gravity = 0.05f;
                [SerializeField, HideInInspector] private float areaHeight = 10f;
                [SerializeField, HideInInspector] private float areaOffset = 5f;

                public static List<Bridge> bridges = new List<Bridge> ( );

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool view = true;

                private void OnDrawGizmos ( )
                {
                        if (particle == null || !view) return;

                        Draw.GLStart ( );
                        for (int i = 0; i < particle.Length; i++)
                        {
                                Draw.GLCircle (new Vector2 (particle[i].x, particle[i].y), 0.05f, Color.green, 2);
                                if (i < particle.Length - 1)
                                {
                                        Debug.DrawLine (particle[i].position, particle[i + 1].position, Color.yellow);
                                }
                        }
                        Draw.GLEnd ( );
                        Draw.Square (rect, Color.yellow);
                }
                #pragma warning restore 0414
                #endif
                #endregion

                #region Initialize
                private void Awake ( )
                {
                        if (createOnAwake)
                        {
                                CreateBridge ( );
                        }
                }
                private void OnEnable ( )
                {
                        if (!bridges.Contains (this))
                        {
                                bridges.Add (this);
                        }
                }
                private void OnDisable ( )
                {
                        if (bridges.Contains (this))
                        {
                                bridges.Remove (this);
                        }
                }

                public void CreateBridge ( )
                {
                        planks = planks < 2 ? 2 : planks;
                        Vector3 endPoint = transform.position + endOffset;
                        float distance = (transform.position - endPoint).magnitude;
                        Vector2 direction = (endPoint - transform.position) / (distance == 0 ? 1f : distance);
                        float plankLength = (distance / planks) - 0f;
                        int particles = planks + 1;

                        particle = new Particle[particles];
                        for (int i = 0; i < particle.Length; i++)
                        {
                                particle[i] = new Particle ((Vector2) transform.position + direction * plankLength * i, -gravity, i == 0 || i == particle.Length - 1);
                                if (i == particle.Length - 1) particle[i].SetPosition (endPoint);
                        }

                        stick = new Stick[planks];
                        for (int i = 0; i < stick.Length; i++)
                        {
                                stick[i] = new Stick (i, i + 1, plankLength);
                        }

                        Vector2 p = transform.position;
                        rect = new Rect (p.x, p.y - areaOffset, Mathf.Abs (endPoint.x - p.x), areaHeight);
                        CreateBridgeGameObjects (plankLength, endPoint);
                }

                private void CreateBridgeGameObjects (float plankLength, Vector3 endPoint)
                {
                        if (plankSprite == null)
                        {
                                Debug.LogWarning ("Bridge requires a plank sprite.");
                                return;
                        }

                        GameObject gameObject = new GameObject ( );
                        gameObject.name = "Plank";
                        gameObject.transform.parent = transform;
                        gameObject.AddComponent<Tether> ( );
                        gameObject.AddComponent<SpriteRenderer> ( ).sprite = plankSprite;

                        for (int i = 0; i < plankList.Count; i++)
                        {
                                if (plankList[i] == null || plankList[i].gameObject == null)
                                {
                                        continue;
                                }
                                if (i == 0)
                                {
                                        SpriteRenderer renderer = plankList[i].gameObject.GetComponent<SpriteRenderer> ( );
                                        if (renderer != null) gameObject.GetComponent<SpriteRenderer> ( ).color = renderer.color;
                                        gameObject.transform.localScale = plankList[i].transform.localScale;
                                }
                                DestroyImmediate (plankList[i].gameObject);
                        }

                        plankList.Clear ( );

                        Vector3 startPosition = transform.position;
                        Vector3 direction = (endPoint - startPosition).normalized;
                        startPosition += direction * plankLength * 0.5f;
                        plankList.Add (gameObject.GetComponent<Tether> ( ));
                        gameObject.transform.position = startPosition;

                        for (int i = 1; i < planks; i++)
                        {
                                Vector2 position = startPosition + direction * plankLength * i;
                                GameObject newPlank = Instantiate (gameObject, position, Quaternion.identity, transform);
                                newPlank.name = plankSprite.name + "_" + (i + 1).ToString ( );
                                plankList.Add (newPlank.GetComponent<Tether> ( ));
                        }
                }
                #endregion

                #region Find
                public static void Find (WorldCollision world, Vector2 center, bool hasJumped, ref Vector2 velocity)
                {
                        if (bridges.Count == 0 || Time.deltaTime == 0)
                        {
                                return;
                        }
                        for (int i = bridges.Count - 1; i >= 0; i--)
                        {
                                if (bridges[i] != null && bridges[i].enabled)
                                {
                                        bridges[i].Execute (i, world, center, hasJumped, ref velocity);
                                }
                        }
                }
                #endregion 

                #region Physics
                private void FixedUpdate ( )
                {
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
                        for (int i = 0; i < stick.Length; i++)
                        {
                                if (i < plankList.Count)
                                {
                                        plankList[i].BridgeRotate (particle[stick[i].first].position, particle[stick[i].second].position, plankOffset);
                                }
                        }
                }
                #endregion

                #region Character
                public void Execute (int index, WorldCollision world, Vector2 center, bool hasJumped, ref Vector2 velocity)
                {
                        if (rect.Contains (center))
                        {
                                RunBridgeState (index, world, hasJumped, ref velocity);
                        }
                        else if (world.bridge.TryGetValue (index, out PlankState bridgeState))
                        {
                                if (bridgeState != PlankState.BeginSearch)
                                {
                                        world.bridge[index] = PlankState.BeginSearch;
                                        velocity.y -= velocity.y < 0 ? 0.1f : 0; //   character walked off bridge. Decrease velocity y to get to ground sooner and avoid the jump/falling state
                                }
                        }
                }

                private bool TetherIntersection (Vector2 characterPosition, bool longSearch, float characterHeight, float velY, out Vector2 intersectionPoint, out int index)
                {
                        index = 0;
                        intersectionPoint = Vector2.zero;
                        characterPosition.x = Mathf.Clamp (characterPosition.x, rect.x + 0.01f, rect.x + rect.width - 0.01f); //  keep center in bounds
                        Vector2 characterTop = longSearch ? new Vector2 (characterPosition.x, rect.max.y) : characterPosition + Vector2.up * characterHeight;
                        Vector2 characterBottom = longSearch? new Vector2 (characterPosition.x, rect.min.y) : characterPosition + Vector2.up * (velY - 0.2f); //  0.2f to find bridge quicker 

                        for (int i = 0; i < stick.Length; i++)
                        {
                                if (Compute.LineIntersection (characterTop, characterBottom, particle[stick[i].first].position, particle[stick[i].second].position, out intersectionPoint))
                                {
                                        index = i;
                                        return true;
                                }
                        }
                        return false;
                }

                private Vector2 CornerHop (WorldCollision world, Vector2 bottomCenter, Vector2 intersect, Vector2 adjust, Vector2 corner, Vector2 start, Vector2 end, ref Vector2 velocity)
                {
                        bottomCenter.x = world.box.BottomExactCorner (velocity.x).x + adjust.x;
                        intersect.y = Compute.PointOnLine (start, end, bottomCenter.x, intersect.y);
                        float minimize = Mathf.Abs (bottomCenter.x - corner.x) > 0.75f && intersect.y > bottomCenter.y ? 0.1f : 0;
                        intersect.y = intersect.y > corner.y ? corner.y : intersect.y - minimize;
                        return intersect;
                }

                private void RunBridgeState (int index, WorldCollision world, bool hasJumped, ref Vector2 velocity)
                {
                        if (!world.bridge.ContainsKey (index))
                        {
                                world.bridge.Add (index, PlankState.BeginSearch);
                        }

                        PlankState state = world.bridge[index];
                        Vector2 velocityAdjust = world.box.right * velocity.x;
                        Vector2 bottomCenter = world.oldPosition + velocityAdjust;

                        if (state == PlankState.BeginSearch) // character top over bridge on initial contact
                        {
                                if (TetherIntersection (bottomCenter, true, 0, 0, out Vector2 intersectionPoint, out int i))
                                {
                                        state = (bottomCenter.y + world.box.sizeY) > intersectionPoint.y ? PlankState.LerpToTether : PlankState.ThresholdCheck;
                                }
                        }
                        if (state == PlankState.ThresholdCheck)
                        {
                                if (TetherIntersection (bottomCenter, true, 0, 0, out Vector2 intersectionPoint, out int i))
                                {
                                        state = bottomCenter.y > intersectionPoint.y ? PlankState.LerpToTether : state;
                                }
                        }
                        if (state == PlankState.LerpToTether)
                        {
                                if (velocity.y <= 0 && TetherIntersection (bottomCenter, false, world.box.sizeY, velocity.y, out Vector2 intersectionPoint, out int i))
                                {
                                        stick[i].ApplyAcceleration (particle, Vector2.down * bounce * Mathf.Abs (velocity.y / Time.deltaTime) * 0.075f);
                                        velocity.y = intersectionPoint.y - bottomCenter.y;
                                        state = PlankState.Latched;
                                        world.onBridge = true;
                                }
                        }
                        if (state == PlankState.Latched)
                        {
                                if (TetherIntersection (bottomCenter, true, 0, 0, out Vector2 intersectionPoint, out int i))
                                {
                                        if (hasJumped)
                                        {
                                                stick[i].ApplyAcceleration (particle, Vector2.down * bounce * Mathf.Abs (velocity.y / Time.deltaTime) * 0.075f); // Better landing feel
                                                state = PlankState.BeginSearch;
                                        }
                                        else
                                        {
                                                if (velocity.x != 0)
                                                {
                                                        float nearDistance = world.box.sizeX * 2f;
                                                        Vector2 start = particle[stick[i].first].position;
                                                        Vector2 end = particle[stick[i].second].position;
                                                        if ((i == stick.Length - 1) && velocity.x > 0 && Mathf.Abs (bottomCenter.x - end.x) < nearDistance) //  raise character corner near bridge exit
                                                        {
                                                                intersectionPoint = CornerHop (world, bottomCenter, intersectionPoint, velocityAdjust, end, start, end, ref velocity);
                                                        }
                                                        if (i == 0 && velocity.x < 0 && Mathf.Abs (bottomCenter.x - start.x) < nearDistance)
                                                        {
                                                                intersectionPoint = CornerHop (world, bottomCenter, intersectionPoint, velocityAdjust, start, start, end, ref velocity);
                                                        }
                                                        stick[i].ApplyAcceleration (particle, Vector2.down * bounce * 0.05f * (Mathf.Abs (velocity.x) / Time.deltaTime));
                                                }
                                                velocity.y = (intersectionPoint.y + 0.02f) - bottomCenter.y;
                                                state = PlankState.Latched;
                                                world.onBridge = true;
                                        }
                                }
                        }
                        world.bridge[index] = state;
                }
                #endregion
        }

}

namespace TwoBitMachines.FlareEngine
{
        public enum PlankState
        {
                BeginSearch,
                LerpToTether,
                ThresholdCheck,
                Latched,
                Jumped
        }
}