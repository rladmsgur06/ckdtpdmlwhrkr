using System.Collections.Generic;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class StateSignals
        {
                public SignalPacket[] all = new SignalPacket[]
                {
                        new SignalPacket ("alwaysTrue"),
                        new SignalPacket ("alwaysFalse"),
                        new SignalPacket ("airGlide"),
                        new SignalPacket ("airJump"),
                        new SignalPacket ("autoGround"),
                        new SignalPacket ("autoCornerJump"),
                        new SignalPacket ("cannonBlast"),
                        new SignalPacket ("ceilingClimb"),
                        new SignalPacket ("changedDirection"),
                        new SignalPacket ("crouch"),
                        new SignalPacket ("crouchSlide"),
                        new SignalPacket ("crouchWalk"),
                        new SignalPacket ("dashing"),
                        new SignalPacket ("dashDiagonal"),
                        new SignalPacket ("dashX"),
                        new SignalPacket ("dashY"),
                        new SignalPacket ("facingLeft"),
                        new SignalPacket ("facingRight"),
                        new SignalPacket ("flutter"),
                        new SignalPacket ("floating"),
                        new SignalPacket ("friction"),
                        new SignalPacket ("highJump"),
                        new SignalPacket ("highFallDamage"),
                        new SignalPacket ("holdingBlock"),
                        new SignalPacket ("hover"),
                        new SignalPacket ("inputX"),
                        new SignalPacket ("inputXZero"),
                        new SignalPacket ("inWater"),
                        new SignalPacket ("jumping"),
                        new SignalPacket ("jumpOnEnemy"),
                        new SignalPacket ("ladderClimb"),
                        new SignalPacket ("meleeCombo"),
                        new SignalPacket ("meleeLeft"),
                        new SignalPacket ("meleeRight"),
                        new SignalPacket ("mouseDirectionLeft"),
                        new SignalPacket ("mouseDirectionRight"),
                        new SignalPacket ("onGround"),
                        new SignalPacket ("onGroundWall"),
                        new SignalPacket ("onVehicle"),
                        new SignalPacket ("onRail"),
                        new SignalPacket ("pickAndThrowBlock"),
                        new SignalPacket ("pickingUpBlock"),
                        new SignalPacket ("pushBack"),
                        new SignalPacket ("pushBackLeft"),
                        new SignalPacket ("pushBackRight"),
                        new SignalPacket ("pushBlock"),
                        new SignalPacket ("pullBlock"),
                        new SignalPacket ("recoil"),
                        new SignalPacket ("recoilDown"),
                        new SignalPacket ("recoilLeft"),
                        new SignalPacket ("recoilRight"),
                        new SignalPacket ("recoilUp"),
                        new SignalPacket ("recoilShake"),
                        new SignalPacket ("recoilSlide"),
                        new SignalPacket ("rope"),
                        new SignalPacket ("ropeClimbing"),
                        new SignalPacket ("ropeHanging"),
                        new SignalPacket ("ropeHolding"),
                        new SignalPacket ("ropeSwinging"),
                        new SignalPacket ("running"),
                        new SignalPacket ("sameDirection"),
                        new SignalPacket ("slamOnEnemy"),
                        new SignalPacket ("slamRecover"),
                        new SignalPacket ("sliding"),
                        new SignalPacket ("slopeSlide"),
                        new SignalPacket ("slopeSlideAuto"),
                        new SignalPacket ("staticFlying"),
                        new SignalPacket ("swimming"),
                        new SignalPacket ("throwingBlock"),
                        new SignalPacket ("vehicleMounted"),
                        new SignalPacket ("wall"),
                        new SignalPacket ("wallLeft"),
                        new SignalPacket ("wallRight"),
                        new SignalPacket ("wallClimb"),
                        new SignalPacket ("wallHold"),
                        new SignalPacket ("wallSlide"),
                        new SignalPacket ("wallHang"),
                        new SignalPacket ("wallSlideJump"),
                        new SignalPacket ("wallCornerGrab"),
                        new SignalPacket ("windJump"),
                        new SignalPacket ("velX"),
                        new SignalPacket ("velXZero"),
                        new SignalPacket ("velY"),
                        new SignalPacket ("velYUp"),
                        new SignalPacket ("velYDown"),
                        new SignalPacket ("velYZero"),
                        new SignalPacket ("zipline")
                };

                public List<SignalPacket> extra = new List<SignalPacket>();

                public bool foldOutVelocity;
                public bool foldOutWall;
                public bool foldOutWorld;
                public bool foldOutRecoil;
                public bool foldOutAttack;
                public bool foldOutUtility;
                public bool foldOutExtra;
                public bool button;
                public string createSignal;
        }

        [System.Serializable]
        public class SignalPacket
        {
                public string name;
                public bool use = false;
                public bool canDelete = false;

                public SignalPacket (string name)
                {
                        this.name = name;
                }
        }

}
