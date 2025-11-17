#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI.BlackboardData;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class SpawnGameObject : Action
        {
#pragma warning disable 0108
                [SerializeField] public GameObject gameObject;
#pragma warning restore 0108
                [SerializeField] public bool recycle; // pool
                [SerializeField] public Transform parent;
                [SerializeField] public PositionType type;
                [SerializeField] public Blackboard target;
                [SerializeField] public Vector2 position;
                [SerializeField] public Blackboard intVariable;
                [SerializeField] public int activeLimit = 0;
                [System.NonSerialized] private List<GameObject> objectPool = new List<GameObject>();

                public override NodeState RunNodeLogic (Root root)
                {
                        if (gameObject == null || (type == PositionType.Target && target == null))
                        {
                                return NodeState.Failure;
                        }
                        if (recycle)
                        {
                                if (activeLimit > 0)
                                {
                                        int activeCount = 0;
                                        for (int i = 0; i < objectPool.Count; i++)
                                        {
                                                if (objectPool[i].activeInHierarchy)
                                                        activeCount++;
                                        }
                                        if (intVariable != null && intVariable is IntVariable intVar)
                                        {
                                                intVar.value = activeCount;
                                        }
                                        if (activeCount >= activeLimit)
                                        {
                                                return NodeState.Failure;
                                        }
                                }
                                for (int i = 0; i < objectPool.Count; i++)
                                {
                                        if (objectPool[i] != null && !objectPool[i].activeInHierarchy)
                                        {
                                                objectPool[i].transform.position = type == PositionType.Point ? position : target.GetTarget();
                                                objectPool[i].transform.rotation = Quaternion.identity;
                                                objectPool[i].SetActive(true);
                                                return NodeState.Success;
                                        }
                                }
                        }
                        GameObject obj;
                        if (type == PositionType.Point)
                        {
                                obj = Instantiate(gameObject, position, Quaternion.identity, parent != null ? parent : transform);
                                obj.SetActive(true);
                        }
                        else
                        {
                                obj = Instantiate(gameObject, target.GetTarget(), Quaternion.identity, parent != null ? parent : transform);
                                obj.SetActive(true);
                        }
                        if (recycle && !objectPool.Contains(obj))
                        {
                                objectPool.Add(obj);
                        }
                        return NodeState.Success;
                }

                public enum PositionType
                {
                        Point,
                        Target
                }

                #region ▀▄▀▄▀▄ Custom Inspector▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(105, "Spawn a gameObject at the specified position. If using the object pool, and if the active limit is greater than zero, it will only spawn objects up to this limit. The Int Variable, if set, will keep track of the active limit count." +
                                        "\n \nReturns Success, Failure");
                        }

                        int type = parent.Enum("type");
                        bool poolActive = parent.Bool("recycle");
                        int extraHeight = poolActive ? 1 : 0;

                        FoldOut.Box(5 + extraHeight, color, offsetY: -2);
                        {
                                parent.Field("Game Object", "gameObject");
                                parent.Field("Spawn At Position", "type");
                                parent.Field("Point", "position", execute: type == 0);
                                if (type == 1)
                                        AIBase.SetRef(ai.data, parent.Get("target"), 0);
                                parent.Field("Parent", "parent");
                                parent.FieldAndEnable("Pool Objects", "activeLimit", "recycle");
                                Labels.FieldText("Active Limit", rightSpacing: 17);
                                if (poolActive)
                                        AIBase.SetRef(ai.data, parent.Get("intVariable"), 1);
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}
