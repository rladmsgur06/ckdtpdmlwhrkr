using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class Vector3ListVariable : Blackboard
        {
                [SerializeField] public List<Vector3> value = new List<Vector3> ( );

                public override Vector2 GetNearestTarget (Vector2 position)
                {
                        Vector2 newTarget = this.transform.position;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < value.Count; i++)
                        {
                                float squareDistance = (position - (Vector2) value[i]).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newTarget = value[i];
                                }
                        }
                        return newTarget;
                }

                public override Vector2 GetRandomTarget ( )
                {
                        if (value.Count > 0)
                        {
                                int randomIndex = Random.Range (0, value.Count);
                                return value[randomIndex];
                        }
                        return this.transform.position;
                }

                public override int ListCount ( ) { return value.Count; }

                public override Vector3 GetVector ( )
                {
                        return value.Count > 0 ? value[value.Count - 1] : Vector3.zero;
                }

                public override bool AddToList (Vector3 newItem)
                {
                        if (newItem == null) return false;
                        value.Add (newItem);
                        return true;
                }

                public override bool RemoveFromList (Vector3 vector)
                {
                        for (int i = 0; i < value.Count; i++)
                        {
                                if (value[i] == vector)
                                {
                                        value.RemoveAt (i);
                                        return true;
                                }
                        }
                        return false;
                }
        }
}