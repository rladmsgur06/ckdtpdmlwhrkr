using UnityEngine;
using System;

namespace TwoBitMachines.FlareEngine.Timeline
{
        public class ChronoTypes
        {
                public static void Serialize (ExtraSerializeTypes extraType , System.Object data , ref string stringData)
                {
                        if (data == null)
                        {
                                stringData = "null-";
                                return;
                        }

                        var type = data.GetType();

                        if (type == typeof(int))
                        {
                                stringData = "int--" + data.ToString();
                        }
                        else if (type == typeof(float))
                        {
                                stringData = "float" + data.ToString();
                        }
                        else if (type == typeof(bool))
                        {
                                stringData = "bool-" + ((bool) data ? "T" : "F");
                        }
                        else if (type.IsEnum && type.FullName.Contains("+")) // nested enum
                        {
                                stringData = "enum-" + type.FullName + "|" + data.ToString();
                        }
                        else if (type.IsEnum)
                        {
                                stringData = "enums" + type.FullName + "|" + data.ToString();
                        }
                        else if (type == typeof(Color))
                        {
                                stringData = "color" + JsonUtility.ToJson((Color) data);
                        }
                        else if (type == typeof(Color32))
                        {
                                stringData = "col32" + JsonUtility.ToJson((Color32) data);
                        }
                        else if (type == typeof(UnityEngine.UI.ColorBlock))
                        {
                                stringData = "colBl" + JsonUtility.ToJson((UnityEngine.UI.ColorBlock) data);
                        }
                        else if (type == typeof(Vector2))
                        {
                                stringData = "vect2" + JsonUtility.ToJson((Vector2) data);
                        }
                        else if (type == typeof(Vector3))
                        {
                                stringData = "vect3" + JsonUtility.ToJson((Vector3) data);
                        }
                        else if (type == typeof(Matrix4x4))
                        {
                                Matrix4x4 matrix = (Matrix4x4) data;
                                stringData = "matrx"
                                 + SerializeVector4(matrix.GetRow(0)) + "~"
                                 + SerializeVector4(matrix.GetRow(1)) + "~"
                                 + SerializeVector4(matrix.GetRow(2)) + "~"
                                 + SerializeVector4(matrix.GetRow(3));
                        }
                        else if (type == typeof(Transform))
                        {
                                // Transform transform = (Transform) data;
                                // stringData = "trfrm"
                                //  + SerializeVector3(transform.position) + "~"
                                //  + SerializeQuaternion(transform.rotation) + "~"
                                //  + SerializeVector3(transform.localScale);
                                stringData = "trfrm";
                                if (extraType != null)
                                        extraType.transform = (Transform) data;
                        }
                        else if (type == typeof(Quaternion))
                        {
                                stringData = "quatr" + JsonUtility.ToJson((Quaternion) data); // SerializeQuaternion((Quaternion) data);
                        }
                        else if (type == typeof(Sprite))
                        {
                                stringData = "sprit";
                                if (extraType != null)
                                        extraType.sprite = (Sprite) data;
                        }
                        else
                        {
                                stringData = "";
                                Debug.Log("Type: " + type.ToString() + " could not be serialized.  " + type.IsEnum);
                                return;
                        }
                }

                public static void Deserialize (ExtraSerializeTypes extraType , ref System.Object data , string stringData)
                {
                        if (stringData.Length == 0)
                        {
                                return;
                        }
                        if (stringData == "null-")
                        {
                                data = null;
                                return;
                        }

                        string dataType = stringData.Substring(0 , 5);
                        string value = stringData.Substring(5);

                        if (dataType == "int--")
                        {
                                data = int.Parse(value);
                        }
                        else if (dataType == "float")
                        {
                                data = float.Parse(value);
                        }
                        else if (dataType == "bool-")
                        {
                                data = value.EndsWith("T"); // Assuming "T" represents true and "F" represents false
                        }
                        else if (dataType == "enum-")
                        {
                                string[] enumInfo = value.Split('|');
                                string enumTypeName = enumInfo[0];

                                string[] typeNames = enumTypeName.Split('+'); // + is the separator for nested types
                                string parentTypeName = typeNames[0];
                                string nestedTypeName = typeNames[1];
                                int lastDotIndex = parentTypeName.LastIndexOf('.');
                                string assembly = parentTypeName.Substring(0 , lastDotIndex);

                                Type parentType = Type.GetType(parentTypeName + ", " + assembly); // qualified name:  "UnityEngine.UI.Image, UnityEngine.UI"

                                if (parentType != null)
                                {
                                        Type enumType = parentType.GetNestedType(nestedTypeName);
                                        if (enumType != null && enumType.IsEnum)
                                        {
                                                object enumValue = Enum.Parse(enumType , enumInfo[1]);
                                                data = enumValue;
                                        }
                                }
                        }
                        else if (dataType == "enums")
                        {
                                string[] enumInfo = value.Split('|');
                                string enumTypeName = enumInfo[0];
                                int lastDotIndex = enumTypeName.LastIndexOf('.');
                                string assembly = enumTypeName.Substring(0 , lastDotIndex); // qualified name:  "UnityEngine.SpriteTileMode, UnityEngine"

                                Type enumType = Type.GetType(enumTypeName + ", " + assembly);
                                if (enumType != null && enumType.IsEnum)
                                {
                                        object enumValue = Enum.Parse(enumType , enumInfo[1]);
                                        data = enumValue;
                                }
                        }
                        else if (dataType == "color")
                        {
                                data = JsonUtility.FromJson<Color>(value);
                        }
                        else if (dataType == "col32")
                        {
                                data = JsonUtility.FromJson<Color32>(value);
                        }
                        else if (dataType == "colBl")
                        {
                                data = JsonUtility.FromJson<UnityEngine.UI.ColorBlock>(value);
                        }
                        else if (dataType == "vect2")
                        {
                                data = JsonUtility.FromJson<Vector2>(value);
                        }
                        else if (dataType == "vect3")
                        {
                                data = JsonUtility.FromJson<Vector3>(value);
                        }
                        else if (dataType == "matrx")
                        {
                                string[] components = value.Split('~');
                                Matrix4x4 matrix = Matrix4x4.identity;
                                matrix.SetRow(0 , DeserializeVector4(components[0]));
                                matrix.SetRow(1 , DeserializeVector4(components[1]));
                                matrix.SetRow(2 , DeserializeVector4(components[2]));
                                matrix.SetRow(3 , DeserializeVector4(components[3]));
                                data = matrix;
                        }
                        else if (dataType == "trfrm")
                        {
                                // string[] components = value.Split('~');
                                // Transform transform = (Transform) data;
                                // transform.position = DeserializeVector3(components[0]);
                                // transform.rotation = DeserializeQuaternion(components[1]);
                                // transform.localScale = DeserializeVector3(components[2]);
                                if (extraType != null)
                                        data = extraType.transform;
                        }
                        else if (dataType == "quatr")
                        {
                                data = JsonUtility.FromJson<Quaternion>(value);
                        }
                        else if (dataType == "sprit")
                        {
                                if (extraType != null)
                                        data = extraType.sprite;
                        }
                        else
                        {
                                Debug.Log("Type: " + dataType + " could not be deserialized  ");
                        }


                }

                #region helpers
                private static string SerializeVector4 (Vector4 vector)
                {
                        return $"{vector.x}|{vector.y}|{vector.z}|{vector.w}";
                }
                private static string SerializeVector3 (Vector3 value)
                {
                        return $"{value.x}|{value.y}|{value.z}";
                }

                private static string SerializeQuaternion (Quaternion quaternion)
                {
                        return $"{quaternion.x}|{quaternion.y}|{quaternion.z}|{quaternion.w}";
                }

                private static string SerializeColor (Color value)
                {
                        return $"{value.r}|{value.g}|{value.b}|{value.a}";
                }

                private static string SerializeColor32 (Color32 value)
                {
                        return $"{value.r}|{value.g}|{value.b}|{value.a}";
                }

                private static Color DeserializeColor (string value)
                {
                        string[] components = value.Split('|');
                        float r = float.Parse(components[0]);
                        float g = float.Parse(components[1]);
                        float b = float.Parse(components[2]);
                        float a = float.Parse(components[3]);
                        return new Color(r , g , b , a);
                }

                private static Color32 DeserializeColor32 (string value)
                {
                        string[] components = value.Split('|');
                        byte r = byte.Parse(components[0]);
                        byte g = byte.Parse(components[1]);
                        byte b = byte.Parse(components[2]);
                        byte a = byte.Parse(components[3]);
                        return new Color32(r , g , b , a);
                }

                private static Vector3 DeserializeVector3 (string value)
                {
                        string[] components = value.Split('|');
                        float x = float.Parse(components[0]);
                        float y = float.Parse(components[1]);
                        float z = float.Parse(components[2]);
                        return new Vector3(x , y , z);
                }

                private static Vector4 DeserializeVector4 (string value)
                {
                        string[] components = value.Split('|');
                        float x = float.Parse(components[0]);
                        float y = float.Parse(components[1]);
                        float z = float.Parse(components[2]);
                        float w = float.Parse(components[3]);
                        return new Vector4(x , y , z , w);
                }

                private static Quaternion DeserializeQuaternion (string value)
                {
                        string[] components = value.Split('|');
                        float x = float.Parse(components[0]);
                        float y = float.Parse(components[1]);
                        float z = float.Parse(components[2]);
                        float w = float.Parse(components[3]);
                        return new Quaternion(x , y , z , w);
                }

                #endregion
        }
}
