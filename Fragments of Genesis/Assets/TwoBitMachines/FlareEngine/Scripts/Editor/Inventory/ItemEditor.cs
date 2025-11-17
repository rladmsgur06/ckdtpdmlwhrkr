using TwoBitMachines.Editors;
using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Item) , true)]
        [CanEditMultipleObjects]
        public class ItemEditor : UnityEditor.Editor
        {
                private Item main;
                private SerializedObject parent;

                private void OnEnable ()
                {
                        main = target as Item;
                        parent = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);

                        parent.Update();

                        int type = parent.Enum("type");
                        FoldOut.BoxSingle(type == 0 ? 3 : 2 , Tint.Blue);
                        {
                                parent.Field("Type" , "type");
                                parent.Field("ItemSO" , "itemSO" , execute: type == 0);
                                parent.Field("Add To Journal" , "journal" , execute: type == 0);
                        }
                        Layout.VerticalSpacing(2);

                        Fields.EventFoldOutEffect(parent.Get("onFound") , parent.Get("itemWE") , parent.Get("foldOut") , "On Found");

                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }
        }
}
