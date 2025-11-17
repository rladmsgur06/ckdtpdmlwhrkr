using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class CornerHang //anim signals: wall, wallHang
        {
                [SerializeField] public bool cornerHang;
                [SerializeField] public ClimbType exitType;
                [SerializeField] public string climbUp = "Up";
                [SerializeField] public string climbDown = "Down";
                [SerializeField] public float cornerHangOffset = 0.5f;

                #region 
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] private bool foldOut = false;
                #endif
                #endregion

                public bool Hang (Wall wall, AbilityManager player, ref Vector2 velocity)
                {
                        int wallDirection = wall.Direction (player);

                        if (!cornerHang || velocity.y > 0 || !player.world.missedAHorizontal)
                        {
                                return false;
                        }

                        if (!DetectWall (player, wallDirection) || DetectWall (player, wallDirection, -cornerHangOffset)) //     top corner must hit, while middle part must miss
                        {
                                return false;
                        }

                        if (Escape (player, wallDirection))
                        {
                                return false;
                        }

                        if (player.jumpButtonActive)
                        {
                                velocity.x = 0;
                                return true;
                        }

                        HangOnCorner (player, wallDirection, ref velocity);
                        return true;
                }

                private void HangOnCorner (AbilityManager player, int wallDirection, ref Vector2 velocity)
                {
                        player.signals.Set ("wall");
                        player.signals.Set ("wallHang");
                        player.signals.Set ("wallLeft", wallDirection < 0);
                        player.signals.Set ("wallRight", wallDirection > 0);
                        player.signals.ForceDirection (wallDirection);

                        velocity.x = 0.1f * wallDirection;
                        velocity.y = 0;
                }

                private bool DetectWall (AbilityManager player, int wallDirection, float shift = 0)
                {
                        Vector2 origin = player.world.box.TopExactCorner (wallDirection) + Vector2.up * shift;
                        Vector2 direction = player.world.box.right * wallDirection;
                        float length = Mathf.Abs (wallDirection * 0.5f);
                        return Physics2D.Raycast (origin, direction, length, WorldManager.collisionMask);
                }

                private bool Escape (AbilityManager player, int wallDirection)
                {
                        if (exitType == ClimbType.None) return false;
                        bool useButtons = exitType == ClimbType.Button;
                        bool up = player.inputs.Holding (useButtons ? climbUp : "Left");
                        bool down = player.inputs.Holding (useButtons ? climbDown : "Right");
                        return up || down;
                }
        }
}