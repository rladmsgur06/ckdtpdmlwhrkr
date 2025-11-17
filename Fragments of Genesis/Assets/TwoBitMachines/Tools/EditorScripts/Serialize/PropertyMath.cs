#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.Editors
{
        public static class PropertyMath
        {
                public static void Clamp (this SerializedProperty property , string field , float min = 0 , float max = 1f)
                {
                        SerializedProperty f = property.Get(field);
                        f.floatValue = Mathf.Clamp(f.floatValue , min , max);
                }

                public static void ClampInt (this SerializedProperty property , string field , int min = 0 , int max = 1)
                {
                        SerializedProperty f = property.Get(field);
                        f.intValue = Mathf.Clamp(f.intValue , min , max);
                }
                public static void ClampV2Int (this SerializedProperty property , string field , int min = 0 , int max = 1)
                {
                        SerializedProperty f = property.Get(field);
                        Vector2Int v = f.vector2IntValue;
                        v.x = Mathf.Clamp(v.x , min , max);
                        v.y = Mathf.Clamp(v.y , min , max);
                        f.vector2IntValue = v;
                }

                public static void ClampV2 (this SerializedProperty property , string field , int min = 0 , int max = 1)
                {
                        SerializedProperty f = property.Get(field);
                        Vector2 v = f.vector2Value;
                        v.x = Mathf.Clamp(v.x , min , max);
                        v.y = Mathf.Clamp(v.y , min , max);
                        f.vector2Value = v;
                }

                public static void ClampV2 (this SerializedObject property , string field , int min = 0 , int max = 1)
                {
                        SerializedProperty f = property.Get(field);
                        Vector2 v = f.vector2Value;
                        v.x = Mathf.Clamp(v.x , min , max);
                        v.y = Mathf.Clamp(v.y , min , max);
                        f.vector2Value = v;
                }

                public static void Clamp (this SerializedObject property , string field , float min = 0 , float max = 1f)
                {
                        SerializedProperty f = property.Get(field);
                        f.floatValue = Mathf.Clamp(f.floatValue , min , max);
                }

                public static void ClampInt (this SerializedObject property , string field , int min = 0 , int max = 1)
                {
                        SerializedProperty f = property.Get(field);
                        f.intValue = Mathf.Clamp(f.intValue , min , max);
                }

                public static void Round (this SerializedObject property , string field , float roundSize)
                {
                        SerializedProperty f = property.Get(field);
                        f.floatValue = Compute.Round(f.floatValue , roundSize);
                }
        }
}
#endif
