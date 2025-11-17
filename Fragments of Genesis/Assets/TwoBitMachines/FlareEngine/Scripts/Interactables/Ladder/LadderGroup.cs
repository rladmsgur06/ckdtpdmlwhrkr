using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("")]
        public class LadderGroup : MonoBehaviour
        {
                [SerializeField] private List<LadderInstance> ladder = new List<LadderInstance> ( );

                private void OnEnable ( )
                {
                        // for (int i = 0; i < ladder.Count; i++)
                        // {
                        //         if (!Ladder.ladders.Contains (ladder[i]))
                        //         {
                        //                 Ladder.ladders.Add (ladder[i]);
                        //         }
                        // }
                }

                private void OnDisable ( )
                {
                        // for (int i = 0; i < ladder.Count; i++)
                        // {
                        //         if (Ladder.ladders.Contains (ladder[i]))
                        //         {
                        //                 Ladder.ladders.Remove (ladder[i]);
                        //         }
                        // }
                }

                public void CreateAndClean (List<Vector2> position, List<Vector2> size)
                {
                        for (int i = 0; i < ladder.Count; i++)
                        {
                                ladder[i].editorCheck = false;
                        }

                        for (int i = 0; i < position.Count; i++)
                        {
                                if (i < ladder.Count)
                                {
                                        ladder[i].SetPositionAndSize (position[i], size[i]);
                                        ladder[i].editorCheck = true;
                                }
                                else
                                {
                                        LadderInstance newLadder = new LadderInstance ( );
                                        newLadder.SetPositionAndSize (position[i], size[i]);
                                        newLadder.editorCheck = true;
                                        ladder.Add (newLadder);
                                }
                        }

                        for (int i = ladder.Count - 1; i >= 0; i--)
                        {
                                if (!ladder[i].editorCheck)
                                {
                                        ladder.RemoveAt (i);
                                }
                        }
                }

        }

}