using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class GameObjectListVariable : Blackboard
        {
                public List<GameObject> value = new List<GameObject>();

                public override Vector2 GetNearestTarget (Vector2 position)
                {
                        Vector2 newTarget = this.transform.position;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < value.Count; i++)
                        {
                                if (value[i] == null)
                                        continue;

                                float squareDistance = (position - (Vector2) value[i].transform.position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newTarget = value[i].transform.position;
                                }
                        }
                        return newTarget;
                }

                public override GameObject GetNearestGameObjectTarget (Vector2 position)
                {
                        GameObject newGameObject = this.gameObject;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < value.Count; i++)
                        {
                                if (value[i] == null)
                                        continue;

                                float squareDistance = (position - (Vector2) value[i].transform.position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newGameObject = value[i];
                                }
                        }
                        return newGameObject;
                }

                public override Vector2 GetRandomTarget ()
                {
                        if (value.Count > 0)
                        {
                                int randomIndex = Random.Range(0, value.Count);
                                return value[randomIndex] != null ? value[randomIndex].transform.position : this.transform.position;
                        }
                        return this.transform.position;
                }

                public override GameObject GetRandomGameObjectTarget ()
                {
                        if (value.Count > 0)
                        {
                                int randomIndex = Random.Range(0, value.Count);
                                return value[randomIndex] != null ? value[randomIndex] : this.gameObject;
                        }
                        return this.gameObject;
                }

                public override GameObject GetGameObject ()
                {
                        return value.Count > 0 ? value[value.Count - 1] : null;
                }

                public override Transform GetTransform ()
                {
                        return GetGameObject() != null ? GetGameObject().transform : null;
                }

                public override bool AddToList (GameObject newItem)
                {
                        if (newItem == null)
                                return false;
                        value.Add(newItem);
                        return true;
                }

                public override bool RemoveFromList (GameObject item)
                {
                        if (value.Contains(item))
                        {
                                value.Remove(item);
                                return true;
                        }
                        return false;
                }

                public override int ListCount () { return value.Count; }
        }
}
