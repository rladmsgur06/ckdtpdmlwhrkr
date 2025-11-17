using TwoBitMachines.Editors;
using TwoBitMachines.FlareEngine.Interactables;
using UnityEditor;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor(typeof(Ladder))]
        [CanEditMultipleObjects]
        public class LadderEditor : UnityEditor.Editor
        {
                private Ladder main;
                private SerializedObject so;

                private void OnEnable ()
                {
                        main = target as Ladder;
                        so = serializedObject;
                        Layout.Initialize();
                }

                public override void OnInspectorGUI ()
                {
                        Layout.Update();
                        Layout.VerticalSpacing(10);
                        so.Update();

                        if (Block.Header(so).Style(Tint.Box).Fold("Ladder").Build())
                        {
                                SerializedProperty ladder = this.so.Get("ladder");
                                Block.Box(5, FoldOut.boxColor, noGap: true);
                                {
                                        ladder.Field_("Size", "size");
                                        ladder.FieldToggleAndEnable_("Stand On Top", "standOnLadder");
                                        ladder.FieldToggleAndEnable_("Align To Center", "alignToCenter");
                                        ladder.FieldToggleAndEnable_("Can Jump Up", "canJumpUp");
                                        ladder.FieldToggleAndEnable_("Stop Side Jump", "stopSideJump");
                                }
                        }

                        SerializedProperty fence = this.so.Get("fenceFlip");
                        if (Block.Header(fence).Style(Tint.Box).Fold("Fence Flip", "fenceFoldOut").Enable("canFlip").Build())
                        {
                                Block.Box(2, FoldOut.boxColor, noGap: true);
                                {
                                        fence.Field_("Flip time", "flipTime");
                                        fence.Field_("SpriteEngine", "spriteEngine");
                                }
                                GUI.enabled = true;
                        }

                        if (Block.Header(so).Style(Tint.Box).Fold("Fence Reverse", "reverseFoldOut").Enable("canFlip").Build())
                        {
                                Block.Box(3, FoldOut.boxColor, noGap: true);
                                {
                                        so.Field_("Reverse Time", "reverseTime");
                                        so.FieldToggleAndEnable_("Flip X", "canFlipX");
                                        so.FieldToggleAndEnable_("Flip Y", "canFlipY");
                                }
                                GUI.enabled = true;
                        }

                        so.ApplyModifiedProperties();
                        Layout.VerticalSpacing(10);
                }

        }
}

// if (Flex.Get.Bar(Kolor.Box).LabelNormal("Ladder").Fold(ref main.ladder.foldOut))
// {
//         Flex.Get.Box(Kolor.Box, 5, 2);
//         {
//                 Flex.Get.LabelNormal("Size").Vector2(ref main.ladder.size);
//                 Flex.Get.NextLine().Toggle("Stand On Top", ref main.ladder.standOnLadder);
//                 Flex.Get.NextLine(18).Toggle("Align To Center", ref main.ladder.alignToCenter);
//                 Flex.Get.NextLine(18).Toggle("Can Jump Up", ref main.ladder.canJumpUp);
//                 Flex.Get.NextLine(18).Toggle("Stop Side Jump", ref main.ladder.stopSideJump);
//         }
// }

// if (Flex.Get.Bar(Kolor.Box).LabelNormal("Fence").Enable("Circle", ref main.fenceFlip.canFlip).Fold(ref main.ladder.foldOut))
// {
//         GUI.enabled = main.fenceFlip.canFlip;
//         Flex.Get.Box(Kolor.Box, 2, 2);
//         {
//                 Flex.Get.LabelNormal("FlipTime").Float(ref main.fenceFlip.flipTime);
//                 Flex.Get.NextLine().LabelNormal("Sprite Engine").ObjectField(ref main.fenceFlip.spriteEngine);
//         }
//         GUI.enabled = true;
// }
