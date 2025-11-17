using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetTransforms : Blackboard
        {
                [SerializeField] public TargetFindType findType;
                [SerializeField] public bool loopSequence = true;
                [SerializeField] public bool resetOnComplete = false;
                [SerializeField] public List<Transform> transforms = new List<Transform>();
                [System.NonSerialized] private int previousRandom = -1;

                public override Vector2 GetTarget (int index = 0)
                {
                        if (findType == TargetFindType.FindNearest)
                        {
                                return GetNearestTarget(transform.position);
                        }
                        else if (findType == TargetFindType.GetRandom)
                        {
                                return GetRandomTarget();
                        }
                        else if (index >= 0 && index < transforms.Count)
                        {
                                return transforms[index] != null ? transforms[index].position : transform.position;
                        }
                        return transform.position;
                }

                public override Vector2 GetNearestTarget (Vector2 position)
                {
                        hasNoTargets = false;
                        Vector2 target = transform.position;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < transforms.Count; i++)
                        {
                                if (transforms[i] == null)
                                        continue;
                                float squareDistance = (position - (Vector2) transforms[i].position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        target = transforms[i].position;
                                }
                        }
                        hasNoTargets = true;
                        return target;
                }

                public override Transform GetNearestTransformTarget (Vector2 position)
                {
                        hasNoTargets = false;
                        Transform newTransform = this.transform;
                        float sqrMagnitude = Mathf.Infinity;
                        for (int i = 0; i < transforms.Count; i++)
                        {
                                if (transforms[i] == null)
                                        continue;
                                float squareDistance = (position - (Vector2) transforms[i].position).sqrMagnitude;
                                if (squareDistance < sqrMagnitude)
                                {
                                        sqrMagnitude = squareDistance;
                                        newTransform = transforms[i];
                                }
                        }
                        hasNoTargets = true;
                        return newTransform;
                }

                public override Vector2 GetRandomTarget ()
                {
                        hasNoTargets = false;
                        if (transforms.Count > 0)
                        {
                                int randomIndex = Random.Range(0, transforms.Count);
                                return transforms[randomIndex] != null ? transforms[randomIndex].position : transform.position;
                        }
                        hasNoTargets = true;
                        return this.transform.position;
                }

                public override Transform GetRandomTransformTarget ()
                {
                        hasNoTargets = false;
                        if (transforms.Count > 0)
                        {
                                int randomIndex = Randomize();
                                return transforms[randomIndex] != null ? transforms[randomIndex] : this.transform;
                        }
                        hasNoTargets = true;
                        return this.transform;
                }

                public override NodeState NextTarget (ref Vector3 target, ref bool reversing, ref int index, Transform transform)
                {
                        if (findType != TargetFindType.FollowSequence)
                        {
                                return NodeState.Success;
                        }
                        if (!loopSequence)
                        {
                                if (reversing)
                                {
                                        bool complete = index - 1 < 0;
                                        index = complete ? 0 : index - 1;
                                        target = transforms[index] != null ? transforms[index].position : target;
                                        if (complete)
                                                reversing = false;
                                        return complete ? NodeState.Success : NodeState.Running;
                                }
                                else
                                {
                                        bool complete = index + 1 >= transforms.Count;
                                        index = complete ? transforms.Count - 1 : index + 1;
                                        target = transforms[index] != null ? transforms[index].position : target;
                                        if (complete)
                                        {
                                                reversing = true;
                                        }
                                        if (complete && resetOnComplete)
                                        {
                                                reversing = false;
                                                index = 0;
                                                transform.position = target;
                                        }
                                        return complete ? NodeState.Success : NodeState.Running;
                                }
                        }
                        else
                        {
                                bool complete = index + 1 >= transforms.Count;
                                index = complete ? 0 : index + 1;
                                target = transforms[index] != null ? transforms[index].position : target;
                                if (complete && resetOnComplete)
                                {
                                        transform.position = target;
                                }
                                return complete ? NodeState.Success : NodeState.Running;
                        }
                }

                public override Transform GetTransform ()
                {
                        return transforms.Count > 0 ? transforms[transforms.Count - 1] : null;
                }

                public override bool AddToList (Transform newItem)
                {
                        if (newItem == null)
                                return false;
                        transforms.Add(newItem);
                        return true;
                }

                public override bool RemoveFromList (Transform item)
                {
                        if (transforms.Contains(item))
                        {
                                transforms.Remove(item);
                                return true;
                        }
                        return false;
                }

                private int Randomize ()
                {
                        int newRand = Random.Range(0, transforms.Count);
                        if (newRand != previousRandom || transforms.Count == 1)
                                return previousRandom = newRand;
                        newRand = newRand + 1 < transforms.Count ? newRand + 1 : 0;
                        return previousRandom = newRand;
                }

                public override int ListCount () { return transforms.Count; }

                #region
#if UNITY_EDITOR
                [SerializeField, HideInInspector] public Vector3 oldPosition = -Vector3.one;
                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (!Application.isPlaying)
                        {
                                this.transform.position = Compute.Round(this.transform.position, 0.25f);
                        }
                        if (oldPosition == -Vector3.one)
                        {
                                oldPosition = this.transform.position;
                        }

                        Vector3 newPosition = this.transform.position;
                        if (oldPosition == Vector3.zero)
                        {
                                oldPosition = newPosition;
                        }
                        if ((oldPosition.x != newPosition.x || oldPosition.y != newPosition.y) && !Application.isPlaying) // && TwoBitMachines.Editors.Mouse.ctrl)
                        {
                                MovePoints(-10, newPosition - oldPosition);
                        }
                        else
                        {
                                if (findType == TargetFindType.FollowSequence && transforms.Count > 1)
                                {
                                        for (int i = 0; i < transforms.Count - 1; i++)
                                        {
                                                if (transforms[i] == null || transforms[i + 1] == null)
                                                        continue;
                                                SceneTools.Line(transforms[i].position, transforms[i + 1].position, Color.green);
                                        }
                                }
                        }
                        oldPosition = newPosition;
                }
                private void MovePoints (int except, Vector3 velocity)
                {
                        for (int i = 0; i < transforms.Count; i++)
                        {
                                if (i != except)
                                        transforms[i].position += velocity;
                        }
                }
#endif
                #endregion
        }
}
