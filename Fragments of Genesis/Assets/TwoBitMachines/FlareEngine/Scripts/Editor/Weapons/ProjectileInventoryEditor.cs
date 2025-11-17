using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(ProjectileInventory))]
        public class ProjectileInventoryEditor : UnityEditor.Editor
        {
                private ProjectileInventory main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as ProjectileInventory;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        FoldOut.Bar(parent, Tint.Orange).SL(2).Label("Projectile References").BR();
                        SerializedProperty list = parent.Get("list");
                        if (parent.ReadBool("add"))
                        {
                                list.arraySize++;
                        }

                        for (int i = 0; i < list.arraySize; i++)
                        {
                                if (list.Element(i).FieldFullAndButtonBox("Delete", Tint.Orange, space: 2))
                                {
                                        list.DeleteArrayElement(i);
                                        break;
                                }
                        }

                        if (FoldOut.Bar(parent, Tint.Orange).Label("Enable UI").BRE("enableUI").FoldOut())
                        {
                                FoldOut.Box(2, Tint.Orange, offsetY: -2);
                                GUI.enabled = GUI.enabled && parent.Bool("enableUI");
                                {
                                        parent.Field("Image", "image");
                                        parent.Field("TextMesh", "textMesh");
                                }
                                GUI.enabled = true;
                                Layout.VerticalSpacing(3);
                        }

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }
        }
}
