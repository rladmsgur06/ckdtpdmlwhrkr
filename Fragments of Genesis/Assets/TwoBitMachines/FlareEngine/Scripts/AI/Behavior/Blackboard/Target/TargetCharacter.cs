using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class TargetCharacter : Blackboard
        {
                [SerializeField] public LayerMask layer;
                [SerializeField] public Vector2 offset;
                [SerializeField] public bool ignoreInactive;

                public override Vector2 GetTarget (int index = 0)
                {
                        Vector2 position = transform.position;
                        return NearestPosition(position);
                }

                public Vector2 NearestPosition (Vector2 returnPosition)
                {
                        List<WorldCollision> npc = Character.characters;
                        hasNoTargets = false;
                        int npcCount = 0;
                        float distance = float.MaxValue;
                        Vector2 position = returnPosition;

                        for (int i = 0; i < npc.Count; i++)
                        {
                                if (npc[i] == null || npc[i].transform == this.transform || !Compute.ContainsLayer(layer, npc[i].transform.gameObject.layer))
                                {
                                        continue;
                                }
                                if (ignoreInactive && (!npc[i].transform.gameObject.activeInHierarchy || (npc[i].boxCollider != null && !npc[i].boxCollider.enabled)))
                                {
                                        continue;
                                }
                                npcCount++;
                                float sqrMag = (returnPosition - (Vector2) npc[i].transform.position).sqrMagnitude;
                                if (sqrMag < distance)
                                {
                                        distance = sqrMag;
                                        position = npc[i].transform.position;
                                }
                        }
                        if (npcCount == 0)
                        {
                                hasNoTargets = true;
                                return returnPosition;
                        }
                        return position + offset;
                }

                private Transform NearestTransform (Vector2 position)
                {
                        List<WorldCollision> npc = Character.characters;
                        hasNoTargets = false;
                        int npcCount = 0;
                        float distance = float.MaxValue;
                        Transform newTransform = null;

                        for (int i = 0; i < npc.Count; i++)
                        {
                                if (npc[i] == null || npc[i].transform == this.transform || !Compute.ContainsLayer(layer, npc[i].transform.gameObject.layer))
                                {
                                        continue;
                                }
                                if (ignoreInactive && (!npc[i].transform.gameObject.activeInHierarchy || (npc[i].boxCollider != null && !npc[i].boxCollider.enabled)))
                                {
                                        continue;
                                }
                                npcCount++;
                                float sqrMag = (position - (Vector2) npc[i].transform.position).sqrMagnitude;
                                if (sqrMag < distance)
                                {
                                        distance = sqrMag;
                                        newTransform = npc[i].transform;
                                }
                        }
                        if (npcCount == 0)
                        {
                                hasNoTargets = true;
                                return null;
                        }
                        return newTransform;
                }

                public override Transform GetTransform ()
                {
                        return NearestTransform(transform.position);
                }

                private Transform GetNPCTransform ()
                {
                        return NearestTransform(transform.position);
                }

                public override void Set (Vector3 vector3)
                {
                        Transform transform = GetNPCTransform();
                        if (transform != null)
                                transform.position = vector3;
                }

                public override void Set (Vector2 vector2)
                {
                        Transform transform = GetNPCTransform();
                        if (transform != null)
                                transform.position = vector2;
                }

                public override Vector2 GetOffset ()
                {
                        return offset;
                }
        }

}
