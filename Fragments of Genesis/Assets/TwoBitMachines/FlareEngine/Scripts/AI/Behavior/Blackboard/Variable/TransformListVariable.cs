using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu ("")]
        public class TransformListVariable : Blackboard
        {
                public List<Transform> value = new List<Transform> ( );

                public override Vector2 GetNearestTarget (Vector2 position)
                {
                        Vector2 newTarget = this.transform.position;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < value.Count; i++)
                        {
                                if (value[i] == null) continue;

                                float squareDistance = (position - (Vector2) value[i].position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newTarget = value[i].position;
                                }
                        }
                        return newTarget;
                }

                public override Transform GetNearestTransformTarget (Vector2 position)
                {
                        Transform newTransform = this.transform;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < value.Count; i++)
                        {
                                if (value[i] == null) continue;

                                float squareDistance = (position - (Vector2) value[i].position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newTransform = value[i];
                                }
                        }
                        return newTransform;
                }

                public override Vector2 GetRandomTarget ( )
                {
                        if (value.Count > 0)
                        {
                                int randomIndex = Random.Range (0, value.Count);
                                return value[randomIndex] != null ? value[randomIndex].position : this.transform.position;
                        }
                        return this.transform.position;
                }

                public override Transform GetRandomTransformTarget ( )
                {
                        if (value.Count > 0)
                        {
                                int randomIndex = Random.Range (0, value.Count);
                                return value[randomIndex] != null ? value[randomIndex] : this.transform;
                        }
                        return this.transform;
                }

                public override int ListCount ( ) { return value.Count; }

                public override Transform GetTransform ( )
                {
                        return value.Count > 0 ? value[value.Count - 1] : null;
                }

                public override bool AddToList (Transform newItem)
                {
                        if (newItem == null) return false;
                        value.Add (newItem);
                        return true;
                }

                public override bool RemoveFromList (Transform item)
                {
                        if (value.Contains (item))
                        {
                                value.Remove (item);
                                return true;
                        }
                        return false;
                }
        }
}