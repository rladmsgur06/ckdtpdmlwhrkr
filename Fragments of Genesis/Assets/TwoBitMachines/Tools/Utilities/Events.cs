using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
        [System.Serializable]
        public class UnityEventInt : UnityEvent<int> { }

        [System.Serializable]
        public class UnityEventFloat : UnityEvent<float> { }

        [System.Serializable]
        public class UnityEventBool : UnityEvent<bool> { }

        [System.Serializable]
        public class UnityEventVector2 : UnityEvent<Vector2> { }

        [System.Serializable]
        public class UnityEventVector3 : UnityEvent<Vector3> { }

        [System.Serializable]
        public class UnityEventString : UnityEvent<string> { }

        [System.Serializable]
        public class UnityEventStringBool : UnityEvent<string, bool> { }

        [System.Serializable]
        public class UnityEventNamePosition : UnityEvent<string, Vector3> { }

        [System.Serializable]
        public class UnityEventFloatBool : UnityEvent<float, bool> { }

        [System.Serializable]
        public class UnityEventFloatVector2 : UnityEvent<float, Vector2> { }

        [System.Serializable]
        public class UnityEventGameObject : UnityEvent<GameObject> { }

        [System.Serializable]
        public class UnityEventItem : UnityEvent<ItemEventData> { }

        [System.Serializable]
        public class UnityEventEffect : UnityEvent<ImpactPacket> { }

        [System.Serializable]
        public class UnityEventTransform : UnityEvent<Transform> { }

        public delegate void WorldUpdate (bool gameReset = false);

        public delegate void NormalCallback ();

        public delegate void WorldResetAll ();

        [System.Serializable]
        public class ImpactPacket
        {
                [System.NonSerialized] public string name;
                [System.NonSerialized] public float damageValue;
                [System.NonSerialized] public Vector2 bottomPosition;
                [System.NonSerialized] public Vector2 direction;
                [System.NonSerialized] public Transform transform;
                [System.NonSerialized] public Transform attacker;
                [System.NonSerialized] public Collider2D colliderRef;
                [System.NonSerialized] public static ImpactPacket impact = new ImpactPacket();
                [System.NonSerialized] public int activateType = 0;
                [System.NonSerialized] public int directionX = 0; // character/object x direction

                public ImpactPacket Set (string worldEffect, Vector2 position, Vector2 direction)
                {
                        this.damageValue = 0;
                        this.transform = null;
                        this.colliderRef = null;
                        this.name = worldEffect;
                        this.bottomPosition = position;
                        this.direction = direction;
                        return this;
                }

                public ImpactPacket Set (string worldEffect, Transform targetTransform, Collider2D targetCollider, Vector2 targetPosition, Transform attackerTransform, Vector2 direction, int directionX, float damageValue)
                {
                        this.name = worldEffect;
                        this.damageValue = damageValue;
                        this.bottomPosition = targetPosition;
                        this.direction = direction;
                        this.transform = targetTransform;
                        this.colliderRef = targetCollider;
                        this.attacker = attackerTransform;
                        this.directionX = directionX;
                        return this;
                }

                public void Copy (ImpactPacket copy)
                {
                        if (copy == null)
                                return;

                        this.name = copy.name;
                        this.damageValue = copy.damageValue;
                        this.bottomPosition = copy.bottomPosition;
                        this.direction = copy.direction;
                        this.transform = copy.transform;
                        this.colliderRef = copy.colliderRef;
                        this.attacker = copy.attacker;
                }

                public Vector2 Center ()
                {
                        return colliderRef != null ? (Vector2) colliderRef.bounds.center : bottomPosition;
                }

                public Vector2 Top ()
                {
                        return colliderRef != null ? (Vector2) colliderRef.bounds.center + Vector2.up * colliderRef.bounds.extents.y : bottomPosition;
                }
        }

        [System.Serializable]
        public class ItemEventData
        {
                [SerializeField] public float genericFloat = 0;
                [SerializeField] public string genericString = "";
                [SerializeField] public bool toggle = false;
                [SerializeField] public bool? success = false;

                public void Reset (float genericFloat, string genericString, bool toggle)
                {
                        this.genericFloat = genericFloat;
                        this.genericString = genericString;
                        this.toggle = toggle;
                        success = null;
                }
        }

        [System.Serializable]
        public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
        {
                [SerializeField]
                public List<TKey> keys = new List<TKey>();

                [SerializeField]
                public List<TValue> values = new List<TValue>();

                public void OnBeforeSerialize ()
                {
                        keys.Clear();
                        values.Clear();
                        foreach (KeyValuePair<TKey, TValue> pair in this)
                        {
                                keys.Add(pair.Key);
                                values.Add(pair.Value);
                        }
                }

                // load dictionary from lists
                public void OnAfterDeserialize ()
                {
                        this.Clear();
                        //  Debug.Log("Deserializing dictinary");
                        if (keys.Count != values.Count)
                        {
                                throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));
                        }

                        for (int i = 0; i < keys.Count; i++)
                        {
                                this.Add(keys[i], values[i]);
                        }
                }
        }
}
