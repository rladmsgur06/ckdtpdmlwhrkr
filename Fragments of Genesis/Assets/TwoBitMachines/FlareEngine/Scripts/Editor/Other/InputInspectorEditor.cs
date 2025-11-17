using TwoBitMachines.Editors;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class InputInspectorEditor
        {
                public static string inputName = "Name";

                public static void Display (SerializedObject parent, SerializedProperty inputs, ThePlayer.Player main, PlayerEditor pEditor)
                {
                        if (inputs.arraySize == 0 && !parent.Bool("initInputs")) // if no inputs detected, create base inputs. Only check once, or it will interfere if there is more than one player.
                        {
                                parent.SetTrue("initInputs");
                                CreateBaseInput(inputs, main);
                        }

                        if (FoldOut.Bar(parent, Tint.Blue).Label("Inputs", Color.white).RightButton("clearInputs", "Delete", toolTip: "Clear Input Player Prefs").FoldOut("inputFoldOut"))
                        {

                                for (int i = 0; i < inputs.arraySize; i++)
                                {
                                        SerializedProperty element = inputs.Element(i);
                                        if (element == null || element.objectReferenceValue == null)
                                        {
                                                inputs.DeleteArrayElement(i);
                                                return;
                                        }

                                        SerializedObject input = new SerializedObject(element.objectReferenceValue);
                                        input.Update();
                                        {
                                                FoldOut.BoxSingle(1, FoldOut.boxColor, extraHeight: 3);

                                                int type = input.Enum("type");
                                                if (type == 0 && Fields.Start(input, input.String("buttonName"), -5).F("type", S.CWB + 5).F("key", S.CWB).B("Delete", S.B))
                                                {
                                                        inputs.DeleteArrayElement(i);
                                                        return;
                                                }
                                                if (type == 1 && Fields.Start(input, input.String("buttonName"), -5).F("type", S.CWB + 5).F("mouse", S.CWB).B("Delete", S.B))
                                                {
                                                        inputs.DeleteArrayElement(i);
                                                        return;
                                                }
                                                if (type == 2 && Fields.Start(input, input.String("buttonName"), -5).F("type", S.CWB + 5).F("axisName", S.CWB).B("Delete", S.B))
                                                {
                                                        inputs.DeleteArrayElement(i);
                                                        return;
                                                }
                                                if (type == 3 && Fields.Start(input, input.String("buttonName"), -5).F("type", S.CWB + 5).F("axisName", S.CWB).B("Delete", S.B))
                                                {
                                                        inputs.DeleteArrayElement(i);
                                                        return;
                                                }

                                                if (FoldOut.FoldOutButton(input.Get("foldOut"), offsetY: -3))
                                                {
                                                        SerializedProperty array = input.Get("bindings");
                                                        if (array.arraySize == 0)
                                                        {
                                                                FoldOut.BoxSingle(1, color: FoldOut.boxColor);
                                                                {
                                                                        Fields.ConstructField();
                                                                        Fields.ConstructString("Bind Inputs", S.LW);
                                                                        Fields.ConstructSpace(S.CW - S.B);
                                                                        if (Fields.ConstructButton("Add"))
                                                                        { array.arraySize++; }
                                                                }
                                                                Layout.VerticalSpacing(2);
                                                        }
                                                        if (array.arraySize != 0)
                                                        {
                                                                FoldOut.Box(array.arraySize, FoldOut.boxColor);
                                                                for (int j = 0; j < array.arraySize; j++)
                                                                {
                                                                        SerializedProperty binding = array.Element(j);
                                                                        InputButtonSO inputSO = (InputButtonSO) binding.objectReferenceValue;
                                                                        Fields.ArrayProperty(array, binding, j, inputSO != null ? inputSO.buttonName : "");
                                                                }
                                                                Layout.VerticalSpacing(5);
                                                        }
                                                }
                                        }
                                        input.ApplyModifiedProperties();
                                }

                                if (Fields.InputAndButtonBox("Create New Input", "Add", Tint.Blue, ref inputName))
                                {
                                        CreateScriptableObject(inputs, inputName, main);
                                        inputName = "Name";
                                        pEditor.InputList(main);
                                }
                                DragAndDropArea(inputs);
                        }

                        if (parent.ReadBool("clearInputs"))
                        {
                                main.inputs.ClearInputs();
                                Debug.Log("Cleared Inputs Player Prefs");
                        }

                }

                private static InputButtonSO CreateScriptableObject (SerializedProperty inputs, string name, ThePlayer.Player main)
                {
                        string path = "Assets/TwoBitMachines/FlareEngine/AssetsFolder/Inputs/" + name + ".asset";
                        InputButtonSO oldAsset = AssetDatabase.LoadAssetAtPath(path, typeof(InputButtonSO)) as InputButtonSO;
                        bool containsName = false;
                        for (int i = 0; i < main.inputs.inputSO.Count; i++)
                        {
                                if (main.inputs.inputSO[i] == null)
                                        continue;
                                if (main.inputs.inputSO[i].buttonName == name)
                                        containsName = true;
                        }
                        if (oldAsset != null && !containsName)
                        {
                                inputs.arraySize++;
                                inputs.LastElement().objectReferenceValue = oldAsset;
                                return null;
                        }
                        InputButtonSO asset = ScriptableObject.CreateInstance<InputButtonSO>();
                        asset.buttonName = name;
                        AssetDatabase.CreateAsset(asset, path);
                        AssetDatabase.SaveAssets();
                        inputs.arraySize++;
                        inputs.LastElement().objectReferenceValue = asset;
                        return asset;
                }

                private static void CreateBaseInput (SerializedProperty inputs, ThePlayer.Player main)
                {
                        SetInputsButton(CreateScriptableObject(inputs, "Left", main), "Left", KeyCode.A);
                        SetInputsButton(CreateScriptableObject(inputs, "Right", main), "Right", KeyCode.D);
                        SetInputsButton(CreateScriptableObject(inputs, "Up", main), "Up", KeyCode.W);
                        SetInputsButton(CreateScriptableObject(inputs, "Down", main), "Down", KeyCode.S);
                        SetInputsButton(CreateScriptableObject(inputs, "Jump", main), "Jump", KeyCode.Space);
                        SetInputsButton(CreateScriptableObject(inputs, "Fire", main), "Fire", KeyCode.None, true);
                        AssetDatabase.SaveAssets();
                }

                private static void SetInputsButton (InputButtonSO button, string name, KeyCode key, bool isMouse = false)
                {
                        if (button == null)
                                return;
                        SerializedObject serObj = new SerializedObject(button);
                        serObj.Update();
                        serObj.Get("key").enumValueIndex = (int) key;
                        serObj.Get("buttonName").stringValue = name;
                        if (isMouse)
                                serObj.Get("type").enumValueIndex = 1;
                        if (isMouse)
                                serObj.Get("mouse").enumValueIndex = 0;
                        serObj.ApplyModifiedProperties();
                        button.key = key;
                        EditorUtility.SetDirty(button);
                }

                private static void DragAndDropArea (SerializedProperty inputs)
                {
                        Rect dropArea = Layout.CreateRect(Layout.longInfoWidth + 4, 27, offsetX: -13); //, texture : Texture2D.whiteTexture, color : Tint.Blue);
                        Labels.Centered(dropArea, "-- Add Existing Input Here --", Color.white, fontSize: 13, shiftY: 0);
                        Fields.DropAreaGUI<InputButtonSO>(dropArea, inputs);
                }
        }
}
