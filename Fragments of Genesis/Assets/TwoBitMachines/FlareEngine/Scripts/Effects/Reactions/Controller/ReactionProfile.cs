using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("Flare Engine/ReactionProfile")]
        public class ReactionProfile : MonoBehaviour
        {
                [SerializeField] public List<ReactionBehaviour> reaction = new List<ReactionBehaviour>();

                public void Activate (ImpactPacket packet)
                {
                        if (!gameObject.activeInHierarchy)
                        {
                                return;
                        }
                        for (int i = 0; i < reaction.Count; i++)
                        {
                                if (reaction[i] != null)
                                        reaction[i].Activate(packet);
                        }
                }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool active;
                [SerializeField, HideInInspector] private int signalIndex = -1;
#pragma warning restore 0414
#endif
                #endregion
        }
}
