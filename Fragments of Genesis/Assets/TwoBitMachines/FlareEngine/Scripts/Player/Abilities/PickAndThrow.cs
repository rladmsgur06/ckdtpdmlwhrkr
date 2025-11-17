#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using System.Collections.Generic;
using TwoBitMachines.FlareEngine.AI;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.ThePlayer
{
        [AddComponentMenu("")]
        public class PickAndThrow : Ability
        {
                [SerializeField] public string grabButton;
                [SerializeField] public string dropButton;
                [SerializeField] public float throwTime = 0.5f;
                [SerializeField] public float pickUpTime = 0.25f;
                [SerializeField] public bool pickUpLerp = true;
                [SerializeField] public Vector2 holdPosition = new Vector2(0, 2f);
                [SerializeField] public List<Vector2> pickUpPath = new List<Vector2>();

                [System.NonSerialized] private AIBase block;
                [System.NonSerialized] private PickState state;
                [System.NonSerialized] private int pickUpIndex;
                [System.NonSerialized] private float throwCounter;
                [System.NonSerialized] private float pickUpCounter;
                [System.NonSerialized] private Vector2 currentPath;
                [System.NonSerialized] private BoxCollider2D blockCollider;
                [System.NonSerialized] private BoxCollider2D postBlockCollider;
                [System.NonSerialized] private Transform blockTransform;
                [System.NonSerialized] private bool postContact;
                [System.NonSerialized] private List<AIBase> blocks = new List<AIBase>();

                public enum PickState
                {
                        None,
                        PickingUp,
                        Holding,
                        Throwing
                }

                public override void Reset (AbilityManager player)
                {
                        postContact = true;
                        state = PickState.None;
                        blockTransform = null;
                        pickUpCounter = pickUpIndex = 0;

                        if (block != null)
                        {
                                postBlockCollider = blockCollider;
                                blockCollider.gameObject.layer = WorldManager.platformLayer;
                                block.root.pauseCollision = false;
                                block.world.ResetCollisionInfo();
                                block.world.Reset();
                                block.world.box.Update();
                                blockCollider = null;
                                block = null;
                        }
                        CheckPostContact(player);
                }

                public override bool TurnOffAbility (AbilityManager player)
                {
                        Reset(player);
                        return true;
                }

                public override bool IsAbilityRequired (AbilityManager player, ref Vector2 velocity)
                {
                        if (pause)
                                return false;
                        CheckPostContact(player);
                        if (state == PickState.PickingUp || state == PickState.Holding || state == PickState.Throwing)
                        {
                                return true;
                        }
                        if (velocity.x != 0 || !player.ground)
                        {
                                blockTransform = null;
                        }
                        if (player.world.onWall)
                        {
                                blockTransform = player.world.wallTransform;
                        }
                        if (player.world.onGround && player.inputs.Holding("Down") && player.inputs.Holding(grabButton) && block == null && blockCollider == null) // pick from on top of block
                        {
                                if (player.world.verticalTransform != null && player.world.verticalTransform.CompareTag("Block"))
                                {
                                        return BeginHold(player.world.verticalTransform, player);
                                }
                        }
                        if (player.inputs.Holding(grabButton) && block == null && blockCollider == null && blockTransform != null && blockTransform.CompareTag("Block"))
                        {
                                return BeginHold(blockTransform, player);
                        }
                        if (player.inputs.Pressed(grabButton) && block == null && CheckPreviousBlocks(player))
                        {
                                return BeginHold(blockTransform, player);
                        }
                        return false;
                }

                private void CheckPostContact (AbilityManager player)
                {
                        if (postContact)
                        {
                                if (postBlockCollider == null || postBlockCollider == block)
                                {
                                        postContact = false;
                                }
                                else if (player.world.box.BoxTouchingAABB(postBlockCollider))
                                {
                                        player.world.mp.ignoreBlockCollision = true;
                                        player.world.mp.detected = true;
                                }
                                else
                                {
                                        postBlockCollider = null;
                                        postContact = false;
                                }
                        }
                }

                private bool BeginHold (Transform transform, AbilityManager player)
                {
                        pickUpCounter = pickUpIndex = 0;
                        block = transform.GetComponent<AIBase>();
                        blockCollider = block.world.boxCollider;
                        bool blockIsValid = block != null && blockCollider != null;
                        if (blockIsValid)
                        {
                                state = PickState.PickingUp;
                                block.world.ignoreYDown = true;
                                if (!blocks.Contains(block))
                                {
                                        blocks.Add(block);
                                }
                        }
                        currentPath = !blockIsValid ? Vector2.zero : (Vector2) block.transform.position - player.world.position;
                        return blockIsValid;
                }

                public override void ExecuteAbility (AbilityManager player, ref Vector2 velocity, bool isRunningAsException = false)
                {
                        player.signals.Set("pickAndThrowBlock");
                        player.signals.Set("pickingUpBlock", state == PickState.PickingUp);
                        player.signals.Set("holdingBlock", state == PickState.Holding);
                        player.signals.Set("throwingBlock", state == PickState.Throwing);
                }

                private bool CheckPreviousBlocks (AbilityManager player)
                {
                        for (int i = 0; i < blocks.Count; i++)
                        {
                                if (blocks[i] != null && blocks[i].world.boxCollider != null && player.world.box.BoxTouchingAABB(blocks[i].world.boxCollider))
                                {
                                        blockCollider = blocks[i].world.boxCollider;
                                        blockTransform = blocks[i].world.transform;
                                        return true;
                                }
                        }
                        return false;
                }

                public override void PostCollisionExecute (AbilityManager player, Vector2 velocity)
                {
                        switch (state)
                        {
                                case PickState.None:
                                        break;
                                case PickState.PickingUp:
                                        if (block == null || blockCollider == null || !block.gameObject.activeInHierarchy)
                                        {
                                                Reset(player);
                                                break;
                                        }
                                        if ((pickUpPath.Count == 0 || pickUpTime == 0) && HoldBlock(player, velocity))
                                        {
                                                blockCollider.gameObject.layer = 2;
                                                state = PickState.Holding; // hold here for one frame so press doesn't execute immediately
                                                break;
                                        }

                                        if (blockCollider == null || blockCollider.gameObject == null)
                                        {
                                                break;
                                        }

                                        blockCollider.gameObject.layer = 2;
                                        float time = pickUpTime / pickUpPath.Count;

                                        if (pickUpLerp)
                                        {
                                                pickUpCounter = Mathf.Clamp(pickUpCounter + Time.deltaTime, 0, time);
                                                Vector2 newPosition = Compute.LerpNormal(currentPath, pickUpPath[pickUpIndex], pickUpCounter / time);
                                                newPosition.x = player.playerDirection > 0 ? Mathf.Abs(newPosition.x) : -Mathf.Abs(newPosition.x);
                                                block.transform.position = player.world.position + newPosition;
                                                if (pickUpCounter >= time)
                                                {
                                                        currentPath = pickUpPath[pickUpIndex];
                                                        pickUpCounter = 0;
                                                        pickUpIndex++;
                                                }
                                        }
                                        else
                                        {
                                                if (Clock.Timer(ref pickUpCounter, time))
                                                {
                                                        currentPath = pickUpPath[pickUpIndex];
                                                        pickUpIndex++;
                                                }
                                                currentPath.x = player.playerDirection > 0 ? Mathf.Abs(currentPath.x) : -Mathf.Abs(currentPath.x);
                                                block.transform.position = player.world.position + currentPath;
                                        }
                                        if (pickUpIndex >= pickUpPath.Count)
                                        {
                                                pickUpCounter = pickUpIndex = 0;
                                                state = PickState.Holding;
                                        }
                                        break;
                                case PickState.Holding:

                                        block.velocity.y = 0; // Reset gravity
                                        if (block == null || blockCollider == null || !block.gameObject.activeInHierarchy)// || BlockHitWall(velocity))
                                        {
                                                Reset(player);
                                        }
                                        else if (HoldBlock(player, velocity))
                                        {
                                                if (player.inputs.Pressed(grabButton) & Time.deltaTime != 0)
                                                {
                                                        player.signals.Set("throwingBlock");
                                                        state = PickState.Throwing;
                                                        throwCounter = 0;

                                                        Throw jumpTo = block.GetComponent<Throw>();
                                                        if (jumpTo != null)
                                                        {
                                                                block.ChangeState("Throw");
                                                                jumpTo.SetForce(velocity / Time.deltaTime);
                                                                block.root.signals.characterDirection = player.playerDirection;
                                                                Reset(player);
                                                        }
                                                }
                                                else if (player.inputs.Pressed(dropButton))
                                                {
                                                        throwCounter = 0;
                                                        block.root.signals.characterDirection = player.playerDirection;
                                                        Reset(player);
                                                }
                                        }
                                        break;
                                case PickState.Throwing:
                                        if (Clock.Timer(ref throwCounter, throwTime))
                                        {
                                                Reset(player);
                                        }
                                        break;
                        }
                }

                private bool HoldBlock (AbilityManager player, Vector2 velocity)
                {
                        block.root.pauseCollision = true;
                        Vector2 oldPosition = block.transform.position;
                        Vector2 newHoldPosition = holdPosition;
                        newHoldPosition.x *= Mathf.Sign(player.playerDirection);
                        block.transform.position = player.world.position + newHoldPosition;
                        return true;
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414

                public override bool OnInspector (SerializedObject controller, SerializedObject parent, string[] inputList, Color barColor, Color labelColor)
                {
                        if (Open(parent, "Pick And Throw", barColor, labelColor))
                        {
                                FoldOut.Box(4, FoldOut.boxColorLight, offsetY: -2);
                                {
                                        parent.DropDownList(inputList, "Grab Button", "grabButton");
                                        parent.DropDownList(inputList, "Drop Button", "dropButton");
                                        parent.Field("Throw Time", "throwTime");
                                        parent.Field("Hold Position", "holdPosition");
                                }
                                Layout.VerticalSpacing(3);

                                SerializedProperty array = parent.Get("pickUpPath");
                                if (array.arraySize == 0)
                                {
                                        array.arraySize++;
                                }

                                FoldOut.Box(2 + array.arraySize, FoldOut.boxColorLight);
                                {
                                        parent.Field("Pick Up Time", "pickUpTime");
                                        GUI.enabled = pickUpTime > 0;
                                        parent.Get("pickUpPath").FieldProperty("Pick Up Path");
                                        parent.FieldToggle("Pick Up Lerp", "pickUpLerp");
                                        GUI.enabled = true;
                                }
                                Layout.VerticalSpacing(5);
                        }
                        return true;
                }
#pragma warning restore 0414
#endif
                #endregion
        }

}

//inside hold block
// if (BoxInfo.BoxTouching(blockCollider, WorldManager.collisionMask, 0.0015f, 0.0015f))
// {
//         block.transform.position = oldPosition;
//         Reset(player);
//         return false;
// }

// private bool BlockHitWall (Vector2 velocity)
// {
//         if (blockCollider == null)
//                 return false;

//         BoxInfo.GetColliderCorners(blockCollider);

//         bool hitX = false;
//         Vector2 updateSpeed = Vector3.zero;
//         float signX = Mathf.Sign(velocity.x);
//         float magnitudeX = Mathf.Abs(velocity.x);
//         Vector2 topCorner = signX > 0 ? BoxInfo.topRightCorner : BoxInfo.topLeftCorner;
//         Vector2 bottomCorner = signX > 0 ? BoxInfo.bottomRightCorner : BoxInfo.bottomLeftCorner;
//         topCorner += Vector2.up * 0.01f;
//         bottomCorner += Vector2.up * 0.01f;

//         for (int i = 0; i < 2; i++)
//         {
//                 Vector2 origin = i == 0 ? bottomCorner : topCorner;
//                 RaycastHit2D hit = Physics2D.Raycast(origin, blockCollider.transform.right * signX, magnitudeX, WorldManager.worldMask);
//                 //Debug.DrawRay (origin, blockCollider.transform.right * signX * magnitudeX, Color.red);
//                 if (hit && hit.transform != this.transform && hit.transform != block.transform)
//                 {
//                         hitX = true;
//                         magnitudeX = hit.distance;
//                         // Debug.Log("hit x: " + hit.transform.name);
//                 }
//         }
//         if (hitX)
//         {
//                 updateSpeed = Vector3.right * signX * (magnitudeX - 0.0015f);
//                 block.transform.position += (Vector3) updateSpeed;
//         }

//         float magnitudeY = Mathf.Abs(velocity.y);
//         Vector2 bottomRCorner = BoxInfo.bottomLeftCorner + updateSpeed;
//         Vector2 bottomLCorner = BoxInfo.bottomRightCorner + updateSpeed;
//         bool hitY = false;

//         for (int i = 0; i < 2; i++)
//         {
//                 Vector2 origin = i == 0 ? bottomLCorner : bottomRCorner;
//                 RaycastHit2D hit = Physics2D.Raycast(origin, -blockCollider.transform.up, magnitudeY, WorldManager.worldMask);
//                 // Debug.DrawRay (origin, -blockCollider.transform.up * magnitudeY, Color.red);
//                 if (hit && hit.transform != this.transform && hit.transform != block.transform)
//                 {
//                         hitY = true;
//                         magnitudeY = hit.distance;
//                         // Debug.Log("hit y: " + hit.transform.name);
//                 }
//         }
//         if (hitY)
//         {
//                 block.transform.position += Vector3.down * magnitudeY;
//         }
//         return hitX || hitY;
// }
