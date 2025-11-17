#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class SerializedValue
        {
                public static bool Bool (this SerializedProperty property , string field)
                {
                        return property.Get(field).boolValue;
                }
                public static bool Bool (this SerializedObject property , string field)
                {
                        return property.Get(field).boolValue;
                }
                public static int Enum (this SerializedProperty property , string field)
                {
                        return property.Get(field).enumValueIndex;
                }
                public static int Enum (this SerializedObject property , string field)
                {
                        return property.Get(field).enumValueIndex;
                }
                public static float Float (this SerializedProperty property , string field)
                {
                        return property.Get(field).floatValue;
                }
                public static float Float (this SerializedObject property , string field)
                {
                        return property.Get(field).floatValue;
                }
                public static int Int (this SerializedProperty property , string field)
                {
                        return property.Get(field).intValue;
                }

                public static int Int (this SerializedObject property , string field)
                {
                        return property.Get(field).intValue;
                }

                public static bool ReadBool (this SerializedProperty property)
                {
                        bool value = property.boolValue;
                        if (value)
                        {
                                property.boolValue = false;
                                return true;
                        }
                        return false;
                }

                public static bool ReadBool (this SerializedProperty property , string field)
                {
                        bool value = property.Get(field).boolValue;
                        if (value)
                        {
                                property.Get(field).boolValue = false;
                                return true;
                        }
                        return false;
                }

                public static bool ReadBool (this SerializedObject property , string field)
                {
                        if (property.Get(field).boolValue)
                        {
                                property.Get(field).boolValue = false;
                                return true;
                        }
                        return false;
                }
                public static bool SetFalse (this SerializedProperty property , string field)
                {
                        return property.Get(field).boolValue = false;
                }
                public static bool SetFalse (this SerializedObject property , string field)
                {
                        return property.Get(field).boolValue = false;
                }
                public static bool SetTrue (this SerializedProperty property , string field)
                {
                        return property.Get(field).boolValue = true;
                }
                public static bool SetTrue (this SerializedObject property , string field)
                {
                        return property.Get(field).boolValue = true;
                }
                public static string String (this SerializedProperty property , string field)
                {
                        return property.Get(field).stringValue;
                }
                public static string String (this SerializedObject property , string field)
                {
                        return property.Get(field).stringValue;
                }
                public static bool Toggle (this SerializedProperty property , string field)
                {
                        return property.Get(field).boolValue = !property.Bool(field);
                }
                public static bool Toggle (this SerializedObject property , string field)
                {
                        return property.Get(field).boolValue = !property.Bool(field);
                }
                public static bool Toggle (this SerializedProperty property)
                {
                        return property.boolValue = !property.boolValue;
                }
                public static Vector3 Vector3 (this SerializedProperty property , string field)
                {
                        return property.Get(field).vector3Value;
                }
                public static Vector3 Vector3 (this SerializedObject property , string field)
                {
                        return property.Get(field).vector3Value;
                }
                public static Vector2 Vector2 (this SerializedProperty property , string field)
                {
                        return property.Get(field).vector2Value;
                }
                public static Vector2 Vector2 (this SerializedObject property , string field)
                {
                        return property.Get(field).vector2Value;
                }

        }
}
#endif
