using System.Collections;
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class LevelDesignerConnection
        {
                public static void CreateLadders (GameObject parent, List<Vector2> position, List<Vector2> size)
                {
                        LadderGroup ladderGroup = parent.GetComponentInChildren<LadderGroup> ( );

                        if (ladderGroup == null)
                        {
                                GameObject child = new GameObject ( );
                                ladderGroup = child.AddComponent<LadderGroup> ( );
                                child.transform.parent = parent.transform;
                                child.transform.position = Vector3.zero;
                                child.name = "LadderGroup";
                        }

                        if (ladderGroup != null)
                        {
                                ladderGroup.CreateAndClean (position, size);
                        }
                }
        }
}