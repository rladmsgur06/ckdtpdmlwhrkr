#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class HighJump : Action
        {
                [System.NonSerialized] private Vector2 force;
                [System.NonSerialized] private Vector2 forceOrigin;
                [System.NonSerialized] private int type;
                [System.NonSerialized] private bool initialized;
                [System.NonSerialized] private float timer;
                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private float slowCounter = 0;

                [System.NonSerialized] private Transform oldBoost;
                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                type = 0;
                                counter = 0;
                                slowCounter = 0;
                                initialized = false;
                                oldBoost = null;
                                force = Vector2.zero;
                        }
                        // Find Exit
                        if (type == 1 && root.world.onGround)
                        {
                                return NodeState.Success;
                        }
                        if (type == 2 && !Found(root))
                        {
                                return NodeState.Success;
                        }
                        if (type == 4 && !Found(root))
                        {
                                return NodeState.Success;
                        }
                        if (type == 3 && !Found(root))
                        {
                                oldBoost = null;
                        }
                        // Latch
                        if (type == 0 && Found(root))
                        {
                                counter = 0;
                                forceOrigin = force;
                                initialized = false;
                        }
                        // Execute
                        if (type == 1)
                        {
                                if (initialized)
                                {
                                        initialized = false;
                                        root.velocity.y = force.y;
                                }
                                if (force.x != 0)
                                {
                                        root.velocity.x = force.x;
                                }
                                root.signals.Set("highJump", root.velocity.y > 0);
                        }
                        if (type == 2)
                        {
                                if (Found(root))
                                {
                                        root.velocity.y += force.y;
                                        root.velocity.x += force.x;
                                        root.signals.Set("windJump");
                                        root.signals.Set("windLeft", force.x < 0);
                                        root.signals.Set("windRight", force.x > 0);
                                        force.y = 0;
                                }
                        }
                        else if (type == 3)
                        {
                                counter += Root.deltaTime;
                                float time = timer <= 0 ? 1f : timer;
                                force = Vector2.Lerp(forceOrigin, Vector2.zero, counter / time);
                                if (counter >= time || root.velocity.magnitude >= force.magnitude)
                                {
                                        return NodeState.Success;
                                }
                                root.velocity = force;
                                root.signals.Set("speedBoost");
                        }
                        else
                        {
                                slowCounter += Root.deltaTime;
                                float adjust = Mathf.Lerp(1f - force.x, 0, slowCounter / 0.75f);
                                if (root.velocity.x != 0)
                                        root.velocity.x *= (force.x + adjust);
                                if (root.velocity.y != 0)
                                        root.velocity.y *= (force.y + adjust);
                        }
                        return NodeState.Running;
                }

                private bool Found (Root root)
                {
                        return Interactables.HighJump.Find(root.world, root.velocity.y, ref type, ref timer, ref force, ref oldBoost);
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(55, "Detect the High Jump interactable." +
                                        "\n \nReturns Running, Success");
                        }

                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }
}
