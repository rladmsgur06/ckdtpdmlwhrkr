#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI.BlackboardData
{
        [AddComponentMenu("")]
        public class Territory : Blackboard
        {
                [SerializeField, HideInInspector] public SimpleBounds bounds = new SimpleBounds();

                private void Awake ()
                {
                        bounds.Initialize();
                }

                public override bool Contains (Vector2 position)
                {
                        return bounds.Contains(position);
                }

                public override Vector2 GetTarget (int index = 0)
                {
                        return GetRandomTarget();
                }

                public override Vector2 GetRandomTarget ()
                {
                        float randomX = Random.Range(0, bounds.size.x);
                        float randomY = Random.Range(0, bounds.size.y);
                        return bounds.position + new Vector2(randomX, randomY);
                }

                #region
#if UNITY_EDITOR
                [SerializeField, HideInInspector] public Vector3 oldPosition;
                [SerializeField, HideInInspector] public bool focus;

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        if (Application.isPlaying)
                        {
                                SceneTools.Square(bounds.position, bounds.size, Color.green);
                                return;
                        }
                        transform.position = Compute.Round(transform.position, 0.25f);
                        Vector3 newPosition = transform.position;

                        if (bounds.position == Vector2.zero)
                        {
                                bounds.position = SceneTools.SceneCenter(transform.position);
                                oldPosition = newPosition;
                        }
                        if (oldPosition.x != newPosition.x || oldPosition.y != newPosition.y) // && TwoBitMachines.Editors.Mouse.ctrl)
                        {
                                bounds.position += (Vector2) (newPosition - oldPosition);
                                oldPosition = newPosition;
                        }
                        else
                        {
                                SceneTools.DrawAndModifyBounds(ref bounds.position, ref bounds.size, Color.green);
                        }

                        SerializedObject parent = new SerializedObject(this);
                        parent.Update();
                        {
                                parent.FindProperty("bounds").Get("size").vector2Value = bounds.size;
                                parent.FindProperty("bounds").Get("position").vector2Value = bounds.position;
                                parent.FindProperty("oldPosition").vector3Value = newPosition;
                        }
                        parent.ApplyModifiedProperties();

                        if (dataName != "")
                        {
                                //   SceneTools.Label(bounds.position + Vector2.up * (bounds.size.y + 4f), dataName, Color.white);
                        }
                }
#endif
                #endregion

        }

}
