#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class Chain : Action
        {
                [SerializeField] private Vector2 offset = new Vector2(1f, 1f);
                [SerializeField] private Transform anchor;
                [SerializeField] private Sprite ropeSprite;
                [SerializeField] private Vector2 tetherSize = new Vector2(1f, 1f);
                [SerializeField] private List<Transform> links = new List<Transform>();

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private int segments = 3; //tethers
                public void CreateRope ()
                {
                        segments = segments < 1 ? 1 : segments;

                        if (ropeSprite == null)
                        {
                                Debug.LogWarning("Chain requires a sprite.");
                                return;
                        }
                        if (anchor == null)
                        {
                                Debug.LogWarning("Chain requires an anchor.");
                                return;
                        }

                        GameObject gameObject = new GameObject();
                        gameObject.name = "Link";
                        if (anchor != null)
                                gameObject.transform.parent = anchor;
                        gameObject.transform.localScale = tetherSize;
                        gameObject.AddComponent<SpriteRenderer>().sprite = ropeSprite;

                        for (int i = 0; i < links.Count; i++)
                        {
                                if (links[i] == null)
                                        continue;
                                if (i == 0)
                                {
                                        SpriteRenderer renderer = links[i].gameObject.GetComponent<SpriteRenderer>();
                                        if (renderer != null)
                                                gameObject.GetComponent<SpriteRenderer>().color = renderer.color;
                                }
                                DestroyImmediate(links[i].gameObject);
                        }

                        links.Clear();

                        links.Add(gameObject.transform);
                        for (int i = 1; i < segments; i++)
                        {
                                GameObject newPlank = Instantiate(gameObject, transform.position, Quaternion.identity, transform);
                                newPlank.transform.parent = anchor;
                                newPlank.name = "Link_" + (i + 1).ToString();
                                links.Add(newPlank.transform);
                        }

                        for (int i = 0; i < links.Count; i++)
                        {
                                links[i].transform.localPosition = Vector3.zero;
                                links[i].transform.localScale = gameObject.transform.localScale;
                        }
                }

                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(85, "A chain created from gameobjects. One end is anchored to a transform, the other is controlled by the by AI's position. Specify the sprite, it's size, and then press the create button." +
                                            "\n \nReturns Running, Failure");
                        }

                        FoldOut.Box(4, color, offsetY: -2);
                        parent.Field("Links", "segments");
                        parent.Field("Offset", "offset");
                        parent.Field("Chain Anchor", "anchor");
                        parent.FieldDouble("Rope Sprite", "ropeSprite", "tetherSize");
                        Layout.VerticalSpacing(3);
                        bool create = FoldOut.LargeButton("Create +", Tint.Orange, Tint.White, Icon.Get("BackgroundLight"));

                        if (create)
                        {
                                parent.ApplyModifiedProperties();
                                CreateRope();
                        }
                        return true;
                }
                public override bool HasNextState () { return false; }
#pragma warning restore 0414
#endif
                #endregion

                public override NodeState RunNodeLogic (Root root)
                {
                        if (anchor == null)
                                return NodeState.Failure;
                        Vector3 distance = transform.position + (Vector3) offset - anchor.position;
                        for (int i = 0; i < links.Count; i++)
                        {
                                float localPercent = (float) i / (float) links.Count;
                                Vector3 position = anchor.transform.position + distance * localPercent; //* i
                                links[i].transform.position = position;
                        }
                        return NodeState.Running;
                }

        }

}
