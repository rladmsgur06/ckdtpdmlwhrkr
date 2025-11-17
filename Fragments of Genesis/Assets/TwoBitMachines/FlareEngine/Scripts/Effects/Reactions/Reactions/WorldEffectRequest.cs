#region
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu("")]

        public class WorldEffectRequest : ReactionBehaviour
        {
                [SerializeField] public WorldEffectActivate activateType;
                [SerializeField] public float probability = 1f;
                [SerializeField] public int quantity = 1;
                [SerializeField] public int min = 1;
                [SerializeField] public int max = 2;
                [SerializeField] public bool random;
                [SerializeField] public bool hasFlareTag;
                [SerializeField] public string flareTag;
                [SerializeField] public WorldEffects controller;
                [SerializeField] public List<string> effect = new List<string>();
                [SerializeField] public WorldEffectModify modify = new WorldEffectModify();

                public enum WorldEffectActivate
                {
                        Activate,
                        ActivateWithDirection,
                        ActivateWithInvertedDirection,
                        ActivateAndClearDirection
                        // ActivateWithMirrorDirection
                }

                public override void Activate (ImpactPacket impact)
                {
                        WorldEffects worldEffect = controller != null ? controller : WorldEffects.get;
                        if (worldEffect == null || effect.Count == 0)
                        {
                                return;
                        }
                        if (hasFlareTag && !FlareTag.ObjectHasTag(impact.attacker, flareTag))
                        {
                                return;
                        }
                        if (probability < 1f && Random.Range(0, 1f) > probability)
                        {
                                return;
                        }

                        int iterate = random ? Random.Range(min, max + 1) : quantity <= 0 ? 1 : quantity;

                        for (int i = 0; i < iterate; i++)
                        {
                                WorldEffectPool.currentGameObject = null;
                                impact.name = effect.Count == 1 ? effect[0] : effect[Random.Range(0, effect.Count)];

                                if (activateType == WorldEffectActivate.Activate)
                                {
                                        worldEffect.Activate(impact);
                                }
                                else if (activateType == WorldEffectActivate.ActivateWithDirection)
                                {
                                        worldEffect.ActivateWithDirection(impact);
                                }
                                else if (activateType == WorldEffectActivate.ActivateWithInvertedDirection)
                                {
                                        worldEffect.ActivateWithInvertedDirection(impact);
                                }
                                else if (activateType == WorldEffectActivate.ActivateAndClearDirection)
                                {
                                        worldEffect.ActivateAndClearDirection(impact);
                                }
                                if (WorldEffectPool.currentGameObject != null)
                                {
                                        modify.Activate(WorldEffectPool.currentGameObject, impact);
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public bool modifyFoldOut;
                public override bool OnInspector (SerializedObject parent, Color barColor, Color labelColor)
                {
                        if (Open(parent, "World Effect:  " + (effect.Count > 0 ? effect[0] : ""), barColor, labelColor))
                        {
                                SerializedProperty modify = parent.Get("modify");

                                FoldOut.Box(7, Tint.Box);
                                {
                                        parent.Field("Activate", "activateType");
                                        modify.Field("Type", "type");
                                        modify.FieldDouble("Position", "position", "yOffset");
                                        Labels.FieldText("Y Offset");

                                        bool random = parent.Bool("random");
                                        parent.FieldAndToggle("Quantity", "quantity", "random", execute: !random);
                                        parent.FieldDoubleAndEnable("Random Quantity", "min", "max", "random", execute: random);
                                        parent.Slider("Probability", "probability");
                                        if (random)
                                                Labels.FieldDoubleText("Min", "Max", rightSpacing: 19);
                                        parent.Field("Controller", "controller");
                                        parent.FieldAndEnable("Attack Flare Tag", "flareTag", "hasFlareTag");
                                }
                                Layout.VerticalSpacing(5);

                                Modify(modify);

                                SerializedProperty array = parent.Get("effect");
                                if (array.arraySize == 0)
                                        array.arraySize++;

                                FoldOut.Box(array.arraySize, Tint.Blue);
                                {
                                        array.FieldProperty("Effect Name");
                                }
                                Layout.VerticalSpacing(5);
                        }
                        return true;
                }

                public void Modify (SerializedProperty modify)
                {
                        FoldOut.Box(5, Tint.Box);
                        modify.FieldDoubleAndEnable("Random Rotation", "randomRotationMin", "randomRotationMax", "useRandomRotation");
                        Labels.FieldDoubleText("Min", "Max", rightSpacing: 19);
                        modify.FieldDoubleAndEnable("Random X Offset", "randomXOffsetMin", "randomXOffsetMax", "useRandomX");
                        Labels.FieldDoubleText("Min", "Max", rightSpacing: 19);
                        modify.FieldDoubleAndEnable("Random Y Offset", "randomYOffsetMin", "randomYOffsetMax", "useRandomY");
                        Labels.FieldDoubleText("Min", "Max", rightSpacing: 19);
                        modify.FieldToggleAndEnable("Check For Walls", "checkForWalls");
                        modify.FieldToggleAndEnable("Mirror X", "flipX");
                        Layout.VerticalSpacing(5);
                }
#pragma warning restore 0414
#endif
                #endregion
        }
}
