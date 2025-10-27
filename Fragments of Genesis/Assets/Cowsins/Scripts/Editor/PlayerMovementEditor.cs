#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace cowsins2D
{
    [System.Serializable]
    [CustomEditor(typeof(PlayerMovement))]
    public class PlayerMovementEditor : Editor
    {
        private string[] tabs = { "Assignables", "Movement", "Jump", "Crouch", "Advanced Movement", "Stamina", "Assist", "Others", "Events" };
        private int currentTab = 0;

        private bool showWallRun = false;
        private bool showWallSlide = false;
        private bool showDash = false;
        private bool showGlide = false;

        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            PlayerMovement myScript = target as PlayerMovement;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/playerMovement_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);


            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.SelectionGrid(currentTab, tabs, 6);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {
                    case "Assignables":
                        EditorGUILayout.LabelField("ASSIGNABLES", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("graphics"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("whatIsGround"));
                        break;
                    case "Movement":
                        EditorGUILayout.LabelField("MOVEMENT SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("autoRun"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("walkSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("runSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("horizontalLadderSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("verticalLadderSpeed"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("runAcceleration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("runDecceleration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airAcceleration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("airDeceleration"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFloorAngle"));

                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("ORIENTATION", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("playerOrientationMethod"));

                        GUILayout.Space(5); 
                        if (myScript._TurnMethod == TurnMethod.Smooth) EditorGUILayout.HelpBox("Smooth Turn MethodRecommended for 2.5D games only.", MessageType.Info);
                        else EditorGUILayout.HelpBox("Simple Turn Method Recommended for 2D games.", MessageType.Info);

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("turnMethod"));
                        if(myScript._TurnMethod == TurnMethod.Smooth)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("turnDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightRotation"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftRotation"));
                            EditorGUI.indentLevel--;
                        }
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("GRAVITY", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("gravityScale"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fallGravityMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFallSpeed"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxUpwardsSpeed"));
                        
                        break;
                    case "Jump":
                        EditorGUILayout.LabelField("JUMP", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpMethod"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("amountOfJumps"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpForce"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("apexReachSharpness"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangGravityMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangTimeThreshold"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangAccelerationMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpHangMaxSpeedMult"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("fastFallMultiplier"));
                        break;
                    case "Crouch":
                        EditorGUILayout.LabelField("CROUCHING", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("allowCrouch"));
                        if (myScript.AllowCrouch)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchScaleMultiplier"));
                            if(myScript.crouchScaleMultiplier.x < .1f) myScript.crouchScaleMultiplier = new Vector2(.1f, myScript.crouchScaleMultiplier.y);
                            if (myScript.crouchScaleMultiplier.x > 1) myScript.crouchScaleMultiplier = new Vector2(1, myScript.crouchScaleMultiplier.y);
                            if (myScript.crouchScaleMultiplier.y < .1f) myScript.crouchScaleMultiplier = new Vector2(myScript.crouchScaleMultiplier.x, .1f);
                            if (myScript.crouchScaleMultiplier.y > 1) myScript.crouchScaleMultiplier = new Vector2(myScript.crouchScaleMultiplier.x, 1);
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("canCrouchSlideMidAir"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSlideSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchSlideDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("crouchCollisionOffset"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Advanced Movement":
                        EditorGUILayout.LabelField("ADVANCED MOVEMENT", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
                        {
                            // Wall Jump foldout
                            EditorGUI.indentLevel++;
                            showWallRun = EditorGUILayout.Foldout(showWallRun, "WALL JUMP", true);
                            if (showWallRun)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowWallJump"));
                                if (myScript.AllowWallJump)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpForce"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("cancelInputsOnWallJump"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallJumpTime"));
                                    GUILayout.Space(5);
                                    EditorGUI.indentLevel--;
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);

                        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
                        {
                            // Wall Slide foldout
                            EditorGUI.indentLevel++;
                            showWallSlide = EditorGUILayout.Foldout(showWallSlide, "WALL SLIDE", true);
                            if (showWallSlide)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("allowSlide"));
                                if (myScript.AllowSlide)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("requireHorizontalInputToSlide"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideSpeed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlidingResetsJumps"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideVFXInterval"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("wallSlideVFX"));
                                    GUILayout.Space(5);
                                    EditorGUI.indentLevel--;
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);

                        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
                        {
                            // Dash foldout
                            EditorGUI.indentLevel++;
                            showDash = EditorGUILayout.Foldout(showDash, "DASH", true);
                            if (showDash)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("dashMethod"));
                                if (myScript.DashMethod != DashMethod.None)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("amountOfDashes"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashCooldown"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashGravityScale"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashDuration"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashSpeed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("dashInterval"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("invincibleWhileDashing"));
                                    GUILayout.Space(5);
                                    EditorGUI.indentLevel--;
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);

                        EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
                        {
                            // Glide foldout
                            EditorGUI.indentLevel++;
                            showGlide = EditorGUILayout.Foldout(showGlide, "GLIDE", true);
                            if (showGlide)
                            {
                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(serializedObject.FindProperty("canGlide"));
                                if (myScript.CanGlide)
                                {
                                    EditorGUI.indentLevel++;
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("glideSpeed"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("glideGravity"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canDashWhileGliding"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("glideDurationMethod"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("maximumGlideTime"));
                                    EditorGUILayout.PropertyField(serializedObject.FindProperty("handleOrientationWhileGliding"));
                                    GUILayout.Space(5);
                                    EditorGUI.indentLevel--;
                                }
                                EditorGUI.indentLevel--;
                            }
                            EditorGUI.indentLevel--;
                        }
                        EditorGUILayout.EndVertical();
                        GUILayout.Space(10);
                        break;
                    case "Stamina":
                        EditorGUILayout.LabelField("STAMINA", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("usesStamina"));
                        if (myScript.UsesStamina)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("minStaminaRequiredToRun"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("maxStamina"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaRegenMultiplier"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnJump"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnWallJump"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("staminaLossOnCrouchSlide"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Assist":
                        EditorGUILayout.LabelField("ASSISTANCE OPTIONS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(5);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("coyoteTime"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("jumpInputBufferTime"));
                        break;
                    case "Events":
                        EditorGUILayout.LabelField("EVENTS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(10);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("events"));
                        break;
                    case "Others":
                        EditorGUILayout.LabelField("OTHER SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("sounds"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("step"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsInterval"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsVolume"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("stepHeight"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landOnEnemyDamage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landOnEnemyImpulse"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landVolume"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("landCameraShake"));
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("CHECK SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundCheckSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundRaycastOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("groundRayLength"));

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("ceilingCheckSize"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("leftSideCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("rightSideCheckOffset"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("_wallCheckSize"));
                        break;

                }
            }

            #endregion
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }
}
#endif