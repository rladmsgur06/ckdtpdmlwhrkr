using System.Collections.Generic;
using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(WorldEventListener))]
        public class WorldEventListenerEditor : UnityEditor.Editor
        {
                private WorldEventListener main;
                private SerializedObject parent;
                private GameObject objReference;

                private WorldManager worldManager = null;
                private List<string> names = new List<string>();

                private void OnEnable ()
                {
                        main = target as WorldEventListener;
                        parent = serializedObject;
                        objReference = main.gameObject;
                        Layout.Initialize();
                        worldManager = GameObject.FindObjectOfType<WorldManager>();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        parent.Update();
                        {
                                names.Clear();
                                for (int i = 0; i < worldManager.worldEvents.Count; i++)
                                {
                                        names.Add(worldManager.worldEvents[i].eventName);
                                }
                                FoldOut.Box(1 , FoldOut.boxColor);
                                parent.DropDownList(names.ToArray() , "World Event" , "eventName");
                                Layout.VerticalSpacing(5);
                                Fields.EventFoldOut(parent.Get("onWorldEvent") , parent.Get("foldOut") , "On World Event");

                                string eventName = parent.String("eventName");
                                for (int i = 0; i < worldManager.worldEvents.Count; i++)
                                {
                                        if (eventName == worldManager.worldEvents[i].eventName)
                                        {
                                                parent.Get("worldEvent").objectReferenceValue = worldManager.worldEvents[i];
                                                break;
                                        }
                                }
                        }
                        parent.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);

                }
        }
}
