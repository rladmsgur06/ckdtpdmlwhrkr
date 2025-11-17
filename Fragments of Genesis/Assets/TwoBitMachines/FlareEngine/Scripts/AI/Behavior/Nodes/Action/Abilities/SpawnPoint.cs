#region 
#if UNITY_EDITOR
using System.Threading;
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class SpawnPoint : Action
        {
                [SerializeField] private Vector3 spawnPoint;
                [SerializeField] private Vector3 eulerAngle;
                [SerializeField] private Collider2D collider2DRef;

                private void Awake ()
                {
                        spawnPoint = transform.position;
                        eulerAngle = transform.localEulerAngles;
                }

                public override NodeState RunNodeLogic (Root root)
                {
                        transform.position = spawnPoint;
                        transform.localEulerAngles = eulerAngle;
                        if (collider2DRef == null)
                        {
                                collider2DRef = gameObject.GetComponent<Collider2D>();
                        }
                        if (collider2DRef != null)
                        {
                                collider2DRef.enabled = true;
                        }
                        return NodeState.Success;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool HasNextState () { return false; }
                public override bool OnInspector (AIBase ai , SerializedObject parent , Color color , bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(65 , "Reset the AI to the position and angle it had at the beginning of the scene. This will also enable its collider2D." +
                                        "\n \nReturns Success");
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
