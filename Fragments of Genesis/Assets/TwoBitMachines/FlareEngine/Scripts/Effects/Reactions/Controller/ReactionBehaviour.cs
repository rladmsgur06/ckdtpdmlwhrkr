#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]
        public class ReactionBehaviour : MonoBehaviour
        {
                public virtual void Activate (ImpactPacket packet)
                {

                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool add;
                [SerializeField, HideInInspector] private bool foldOut;
                [SerializeField, HideInInspector] private bool delete;
                [SerializeField, HideInInspector] private bool deleteAsk;
                [SerializeField, HideInInspector] private int signalIndex = -1;

                public virtual bool OnInspector (SerializedObject parent , Color barColor , Color labelColor)
                {
                        return false;
                }
                public static bool Open (SerializedObject parent , string name , Color barColor , Color labelColor)
                {
                        return FoldOut.Bar(parent , barColor , space: 25).Label(name , labelColor).BR("deleteAsk" , "Delete").BR("delete" , "Close" , execute: parent.Bool("deleteAsk")).FoldOut();
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
