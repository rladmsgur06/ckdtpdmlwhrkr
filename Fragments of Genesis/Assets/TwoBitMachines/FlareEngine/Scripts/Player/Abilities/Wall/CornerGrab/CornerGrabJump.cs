using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [System.Serializable]
        public class CornerGrabJump
        {
                public bool isJumping { get; private set; }

                [System.NonSerialized] private int wallDirRef;
                [System.NonSerialized] private Vector2 jumpToVelocity;

                public void Reset ( )
                {
                        isJumping = false;
                }

                public void StartJump (Wall wall, AbilityManager player, int wallDirection, float topCornerY, ref Vector2 velocity)
                {
                        float offsetX = player.world.box.sizeX * wallDirection;
                        Vector2 jumpTo = new Vector2 (player.world.position.x + offsetX, topCornerY);
                        velocity = Compute.ArchObject (player.world.box.bottomCenter, jumpTo, 0.25f, player.gravity.gravity);
                        wallDirRef = wallDirection;
                        jumpToVelocity = velocity;
                        isJumping = true;
                        player.world.mp.Follow ( );
                }

                public void AutoJumpToCorner (AbilityManager player, ref Vector2 velocity)
                {
                        if (!isJumping) return;

                        if (player.ground || Wall.CheckForGround (player.world.box, jumpToVelocity.x, velocity.y) || (player.inputX != 0 && !Compute.SameSign (player.inputX, wallDirRef)))
                        {
                                isJumping = false;
                        }
                        velocity.x = isJumping ? jumpToVelocity.x : velocity.x;
                }

        }
}