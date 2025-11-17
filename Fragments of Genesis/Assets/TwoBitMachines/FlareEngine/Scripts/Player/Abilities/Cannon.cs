#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class Cannon : Ability
        {
                [System.NonSerialized] private Transform cannon;
                [System.NonSerialized] private Vector2 direction;
                [System.NonSerialized] private float airFriction;
                [System.NonSerialized] private float shootOffset;
                [System.NonSerialized] private float force;
                [System.NonSerialized] private bool active;
                [System.NonSerialized] private bool follow;

                public void Activate (Vector2 directionRef , float forceRef , float airFrictionRef , float shootOffsetRef)
                {
                        force = forceRef;
                        direction = directionRef;
                        airFriction = airFrictionRef;
                        shootOffset = shootOffsetRef;

                        cannon = null;
                        active = true;
                        follow = false;
                }

                public void Follow (Transform cannonRef)
                {
                        cannon = cannonRef;
                        follow = true;
                        SetToCannonPosition();
                }

                public override void Reset (AbilityManager player)
                {
                        active = false;
                        follow = false;
                }

                private void SetToCannonPosition ()
                {
                        if (cannon != null)
                        {
                                transform.position = new Vector3(cannon.position.x , cannon.position.y , transform.position.z);
                        }
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player , ref Vector2 velocity)
                {
                        return !pause && (active || follow);
                }

                public override void ExecuteAbility (AbilityManager player , ref Vector2 velocity , bool isRunningAsException = false)
                {
                        if (follow)
                        {
                                velocity = Vector2.zero;
                                if (cannon != null)
                                {
                                        SetToCannonPosition();
                                }
                                else
                                {
                                        player.inputs.Block(false);
                                        Reset(player);
                                }
                        }
                        else
                        {
                                // Set signal!!
                                player.signals.Set("cannonBlast");
                                force += -airFriction * 10f * Time.deltaTime;
                                force = Mathf.Clamp(force , 0 , 1000f);
                                velocity = direction * (force + shootOffset);
                                shootOffset = 0;

                                if (player.world.onWall)
                                {
                                        direction.x = 0;
                                }
                                if (player.world.onCeiling)
                                {
                                        direction.y = 0;
                                }
                                if (direction == Vector2.zero || force <= 0 || player.world.onGround)
                                {
                                        player.inputs.Block(false);
                                        player.signals.Set("cannonBlast" , false);
                                        Reset(player);
                                }
                        }
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public override bool OnInspector (SerializedObject controller , SerializedObject parent , string[] inputList , Color barColor , Color labelColor)
                {
                        if (Open(parent , "Cannon" , barColor , labelColor))
                        {
                                Layout.VerticalSpacing(3);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion

        }

}
