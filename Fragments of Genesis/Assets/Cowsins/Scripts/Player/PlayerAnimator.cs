using System;
using UnityEngine;

namespace cowsins2D
{

    public class PlayerAnimator : MonoBehaviour
    {
        private PlayerMovement player;
        private PlayerStats playerStats;
        private PlayerControl playerControl;

        private string currentState;
        private Animator animator;

        private void OnEnable()
        {
            // Initial settings
            animator = GetComponentInChildren<Animator>();

            if(animator == null) Debug.LogWarning("[COWSINS] There is no Animator component assigned within your Player.");

            player = GetComponent<PlayerMovement>();
            playerControl = GetComponent<PlayerControl>();
            playerStats = GetComponent<PlayerStats>();

            IdleAnim(); 

            // Subscribe to the Movement events in order to play the animations properly
            player.events.onStartGlide.AddListener(GlideAnim);
            player.events.onStartWallSliding.AddListener(WallSlidingAnim);
            player.events.onWallJump.AddListener(WallJumpAnim);
            player.events.onStartFall.AddListener(FallAnim);
            player.events.onJump.AddListener(JumpAnim);
            player.events.onCrouchedIdle.AddListener(CrouchAnim);
            player.events.onCrouchWalking.AddListener(CrouchWalkAnim);
            player.events.onIdleLadder.AddListener(LadderAnim);
            player.events.onMovingLadder.AddListener(LadderMoveAnim);
            player.events.onIdle.AddListener(IdleAnim);
            player.events.onWalking.AddListener(WalkAnim);
            player.events.onRunning.AddListener(RunAnim);
            player.events.onLand.AddListener(LandAnim);
            playerControl.events.onLoseControl.AddListener(IdleAnim);
            playerStats.events.onDie.AddListener(DieAnim); 
        }

        private void OnDisable()
        {
            player.events.onStartGlide.RemoveListener(GlideAnim);
            player.events.onStartWallSliding.RemoveListener(WallSlidingAnim);
            player.events.onWallJump.RemoveListener(WallJumpAnim);
            player.events.onStartFall.RemoveListener(FallAnim);
            player.events.onJump.RemoveListener(JumpAnim);
            player.events.onCrouchedIdle.RemoveListener(CrouchAnim);
            player.events.onCrouchWalking.RemoveListener(CrouchWalkAnim);
            player.events.onIdleLadder.RemoveListener(LadderAnim);
            player.events.onMovingLadder.RemoveListener(LadderMoveAnim);
            player.events.onIdle.RemoveListener(IdleAnim);
            player.events.onWalking.RemoveListener(WalkAnim);
            player.events.onRunning.RemoveListener(RunAnim);
            player.events.onLand.RemoveListener(LandAnim);
            playerControl.events.onLoseControl.RemoveListener(IdleAnim);
            playerStats.events.onDie.RemoveListener(DieAnim);
        }

        public void IdleAnim()
        {
            if (!player.IsGrounded || player.isJumping) return;
            ChangeAnimationState("Idle");
        }

        public void WalkAnim()
        {
            if (!player.IsGrounded) return;
            ChangeAnimationState("Walk");
        }

        public void RunAnim()
        {
            if (!player.IsGrounded) return;
            ChangeAnimationState("Run");
        }

        public void LandAnim() => ChangeAnimationState("Land");

        public void GlideAnim() => ChangeAnimationState("Glide");

        private void WallSlidingAnim() => ChangeAnimationState("Slide");

        private void WallJumpAnim() => ChangeAnimationState("WallJump");

        private void FallAnim()
        {
            if (player.isWallSliding) return; 
            ChangeAnimationState("Fall");
        }

        private void JumpAnim()
        {
            if (player.isWallSliding) return;
            ChangeAnimationState("Jump");
        }
        private void CrouchAnim()
        {
            if(player.rb.velocity.magnitude > 0.5f) return;
            ChangeAnimationState("Crouch");
        }

        private void CrouchWalkAnim()
        {
            ChangeAnimationState("WalkCrouch");
        }

        private void LadderAnim() => ChangeAnimationState("LadderIdle");

        private void LadderMoveAnim() => ChangeAnimationState("LadderMove");

        private void DieAnim() => ChangeAnimationState("Die"); 

        private void ChangeAnimationState(string newState)
        {
            if (currentState == newState) return;

            if(!String.IsNullOrEmpty(currentState)) animator?.ResetTrigger(currentState);
            animator?.SetTrigger(newState);
            currentState = newState;
        }
    }
}