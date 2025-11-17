#if UNITY_EDITOR
using System.Diagnostics;
using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class EditorTools
        {

                public static List<SerializedProperty> events = new List<SerializedProperty>();

                public static void ClearProperty (SerializedProperty property)
                {
                        IEnumerable<SerializedProperty> list = GetChildren(property);
                        // Loop.
                        foreach (SerializedProperty value in list)
                        {
                                if (value.propertyType == SerializedPropertyType.Boolean)
                                        value.boolValue = false;
                                if (value.propertyType == SerializedPropertyType.Float)
                                        value.floatValue = 0;
                                if (value.propertyType == SerializedPropertyType.Integer)
                                        value.intValue = 0;
                                if (value.propertyType == SerializedPropertyType.Enum)
                                        value.enumValueIndex = 0;
                                if (value.propertyType == SerializedPropertyType.String)
                                        value.stringValue = "";
                                if (value.propertyType == SerializedPropertyType.Vector2)
                                        value.vector2Value = Vector2.zero;
                                if (value.propertyType == SerializedPropertyType.Vector3)
                                        value.vector3Value = Vector3.zero;
                                if (value.propertyType == SerializedPropertyType.LayerMask)
                                        value.intValue = 0;
                                if (value.propertyType == SerializedPropertyType.Color)
                                        value.colorValue = Color.black;
                                if (value.propertyType == SerializedPropertyType.ObjectReference)
                                        value.objectReferenceValue = null;

                                SerializedProperty persistentCalls = value.FindPropertyRelative("m_PersistentCalls.m_Calls"); /// clears unity events
                                if (persistentCalls != null)
                                        persistentCalls.arraySize = 0;
                                if (value.isArray)
                                        value.ClearArray();
                        }
                }

                public static void ClearPropertyExceptVectors (SerializedProperty property)
                {
                        IEnumerable<SerializedProperty> list = GetChildren(property);
                        // Loop.
                        foreach (SerializedProperty value in list)
                        {
                                if (value.propertyType == SerializedPropertyType.Boolean)
                                        value.boolValue = false;
                                if (value.propertyType == SerializedPropertyType.Float)
                                        value.floatValue = 0;
                                if (value.propertyType == SerializedPropertyType.Integer)
                                        value.intValue = 0;
                                if (value.propertyType == SerializedPropertyType.Enum)
                                        value.enumValueIndex = 0;
                                if (value.propertyType == SerializedPropertyType.String)
                                        value.stringValue = "";
                                if (value.propertyType == SerializedPropertyType.LayerMask)
                                        value.intValue = 0;
                                if (value.propertyType == SerializedPropertyType.Color)
                                        value.colorValue = Color.black;
                                if (value.propertyType == SerializedPropertyType.ObjectReference)
                                        value.objectReferenceValue = null;

                                SerializedProperty persistentCalls = value.FindPropertyRelative("m_PersistentCalls.m_Calls"); /// clears unity events
                                if (persistentCalls != null)
                                        persistentCalls.arraySize = 0;
                                if (value.isArray)
                                        value.ClearArray();
                        }
                }

                public static void CreateLayer (string layerNameToCreate)
                {
                        //  https://forum.unity3d.com/threads/adding-layer-by-script.41970/reply?quote=2274824
                        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
                        SerializedProperty layers = tagManager.FindProperty("layers");

                        for (int i = 8; i < layers.arraySize; i++)
                        {
                                SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);
                                if (layerSP.stringValue == layerNameToCreate)
                                        return;
                        }
                        for (int j = 8; j < layers.arraySize; j++)
                        {
                                SerializedProperty layerSP = layers.GetArrayElementAtIndex(j);
                                if (layerSP.stringValue == "")
                                {
                                        layerSP.stringValue = layerNameToCreate;
                                        tagManager.ApplyModifiedProperties();
                                        return;
                                }
                        }
                }
                public static GameObject CreatePrefab (string path , GameObject gameObject , bool setActive = false)
                {
#if UNITY_5 || UNITY_2017
                          GameObject newGameObject = PrefabUtility.CreatePrefab (path, gameObject); //       save prefab as inactive
#else
                        GameObject newGameObject = PrefabUtility.SaveAsPrefabAsset(gameObject , path); // save prefab as inactive
#endif
                        newGameObject.gameObject.SetActive(setActive);
                        return newGameObject;
                }

                public static IEnumerable<SerializedProperty> GetChildren (this SerializedProperty property)
                {
                        property = property.Copy();
                        var nextElement = property.Copy();
                        bool hasNextElement = nextElement.NextVisible(false);
                        if (!hasNextElement)
                        {
                                nextElement = null;
                        }

                        property.NextVisible(true);
                        while (true)
                        {
                                if ((SerializedProperty.EqualContents(property , nextElement)))
                                {
                                        yield break;
                                }

                                yield return property;

                                bool hasNext = property.NextVisible(false);
                                if (!hasNext)
                                {
                                        break;
                                }
                        }
                }

                public static SerializedObject[] GetSerializedComponentsInChildren<T> (Transform transform) where T : UnityEngine.Object
                {
                        T[] children = transform.GetComponentsInChildren<T>();
                        SerializedObject[] list = new SerializedObject[children.Length];
                        for (int i = 0; i < list.Length; i++)
                                list[i] = new UnityEditor.SerializedObject(children[i]);
                        return list;
                }

                public static T LoadAsset<T> (string assetPath) where T : UnityEngine.Object
                {
                        return AssetDatabase.LoadAssetAtPath(assetPath , typeof(T)) as T;
                }

                public static void LoadGUI (string directoryName , string assetPath , List<Texture2D> list)
                {
                        list.Clear();
                        string filePath = Util.SearchDirectory(directoryName) + assetPath;
                        string[] textureName = AssetDatabase.FindAssets("t:Texture2D" , new[] { filePath });
                        for (int i = 0; i < textureName.Length; i++)
                        {
                                list.Add(AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(textureName[i])));
                        }
                }

                public static GameObject LoadGameObject (string assetPath)
                {
                        return (GameObject) AssetDatabase.LoadAssetAtPath(assetPath , typeof(GameObject));
                }

                public static bool PathExists (string assetPath)
                {
                        bool exists = AssetDatabase.GetMainAssetTypeAtPath(assetPath) != null;
                        if (!exists)
                                UnityEngine.Debug.LogWarningFormat("Asset at path does not exist:   " + assetPath);
                        return exists;
                }

                public static void Remove<T> (GameObject gameObject) where T : UnityEngine.Object
                {
                        if (gameObject.GetComponent<T>() != null)
                        {
                                MonoBehaviour.DestroyImmediate(gameObject.GetComponent<T>());
                        }
                }

                public static Type RetrieveType (string qualifiedTypeName)
                {
                        Type t = null;
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies)
                        {
                                t = assembly.GetType(qualifiedTypeName);
                                if (t != null)
                                        break;
                        }
                        return t;
                }

                public static bool RetrieveType (string qualifiedTypeName , out Type t)
                {
                        t = null;
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies)
                        {
                                t = assembly.GetType(qualifiedTypeName);
                                if (t != null)
                                        break;
                        }
                        return t != null;
                }

                public static bool ValidateType (string qualifiedTypeName)
                {
                        Type t = null;
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        foreach (var assembly in assemblies)
                        {
                                t = assembly.GetType(qualifiedTypeName);
                                if (t != null)
                                        break;
                        }
                        return t != null;
                }

                public static void FieldOnlyClear (this SerializedProperty property , Rect rect , string field)
                {
                        rect = rect.Offset(-1 , 3 , -2 , -5);
                        GUI.SetNextControlName("user");

                        Color previousBackgroundColor = GUI.backgroundColor;
                        Color previouColor = GUI.color;
                        GUI.backgroundColor = Color.clear;
                        GUI.color = GUI.GetNameOfFocusedControl() == "user" ? Tint.WhiteOpacity25 : Color.clear;
                        EditorGUI.PropertyField(rect , property.Get(field) , GUIContent.none);
                        GUI.backgroundColor = previousBackgroundColor;
                        GUI.color = previouColor;
                }

                public static void FieldRect (this SerializedProperty property , Rect rect , string field , float xOffset = 0 , float yOffset = 0 , float height = 0)
                {
                        rect = rect.Offset(-1 + xOffset , 3 + yOffset , -2 , -5 + height);
                        EditorGUI.PropertyField(rect , property.Get(field) , GUIContent.none);
                }

                public static Vector2 DragRect (Rect rect , Event currentEvent , ref bool isDragged)
                {
                        switch (currentEvent.type)
                        {
                                case EventType.MouseDown:
                                        if (currentEvent.button == 0)
                                        {
                                                if (rect.Contains(currentEvent.mousePosition))
                                                {
                                                        isDragged = true;
                                                }
                                                GUI.changed = true;
                                        }
                                        break;
                                case EventType.MouseUp:
                                        isDragged = false;
                                        GUI.changed = true;
                                        break;

                                case EventType.MouseDrag:
                                        if (currentEvent.button == 0 && isDragged)
                                        {
                                                currentEvent.Use();
                                                return currentEvent.delta;
                                        }
                                        break;
                        }

                        return Vector2.zero;
                }

                public static int CountObjectFields (SerializedObject element)
                {
                        int i = 0;
                        int members = 0;
                        SerializedProperty iterator = element.GetIterator();
                        while (iterator.NextVisible(true) && i++ < 1000) // prevent an infinite loop
                        {
                                if (iterator.name == "m_Script")
                                        continue;
                                SerializedProperty prop = element.FindProperty(iterator.name);
                                if (prop != null && !prop.type.Contains("UnityEvent"))
                                {
                                        if (prop.isArray && prop.propertyType != SerializedPropertyType.String)
                                        {
                                                if (prop.arraySize == 0)
                                                        members++;
                                                else
                                                        members += prop.arraySize;
                                        }
                                        else
                                        {
                                                members++;
                                        }
                                }
                        }
                        return members;
                }

                // If using events, must contain foldOuts list for events
                public static void IterateObject (SerializedObject element , int fields)
                {
                        events.Clear();
                        int i = 0;
                        SerializedProperty iterator = element.GetIterator();
                        while (iterator.NextVisible(true) && i++ < 1000) // prevent an infinite loop
                        {
                                if (iterator.name == "m_Script")
                                        continue;
                                SerializedProperty prop = element.FindProperty(iterator.name);
                                if (prop != null)
                                {
                                        if (prop.type.Contains("UnityEvent"))
                                                events.Add(prop);
                                        else
                                                prop.FieldProperty(iterator.name);
                                }
                        }
                        if (fields != 0)
                                Layout.VerticalSpacing(3);

                        SerializedProperty foldOuts = element.Get("foldOuts");
                        for (int j = 0; j < events.Count; j++)
                        {
                                if (foldOuts != null)
                                {
                                        if (foldOuts.arraySize <= j)
                                                foldOuts.arraySize++;
                                        Fields.EventFoldOut(events[j] , foldOuts.Element(j) , events[j].name , color: FoldOut.boxColor);
                                }
                                else
                                        EditorGUILayout.PropertyField(events[j] , includeChildren: true);
                        }
                }

                public static void CopySerializedObject (SerializedObject copy , SerializedObject source , Type type)
                {
                        copy.Update();
                        {
                                SerializedProperty iterator = source.GetIterator();
                                if (iterator.NextVisible(true))
                                {
                                        while (iterator.NextVisible(true)) //iterate through all serializedProperties
                                        {
                                                SerializedProperty property = copy.FindProperty(iterator.name);

                                                if (property != null && property.propertyType == iterator.propertyType) //validate that the properties are present in both components, and that they're the same type
                                                {
                                                        copy.CopyFromSerializedProperty(iterator);
                                                }
                                        }
                                }
                        }
                        copy.ApplyModifiedProperties();
                }
        }

}
#endif
