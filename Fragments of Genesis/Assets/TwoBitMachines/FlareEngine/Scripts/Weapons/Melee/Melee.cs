using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        // since colliders need to be flipped
        // melee attacks must live on a separate game objects and be registered to player
        // collider position must be set somewhere else, use sprite engine
        // this will only use the collider to deal damage
        // only one hit per combo, or else attack will become frame dependent.
        [AddComponentMenu("Flare Engine/一Weapons/Melee")]
        public class Melee : MonoBehaviour
        {
                [SerializeField] public string meleeName = "Melee Name";
                [SerializeField] public Collider2D collider2DRef;
                [SerializeField] public ChainedCombo melee = new ChainedCombo();
                [SerializeField] public MeleeBlock block = new MeleeBlock();

                [SerializeField] public UnityEventFloat onCoolDown;
                [SerializeField] public float coolDown = 0;
                [SerializeField] public bool pause = false;
                [SerializeField] public bool attackNow = false;

                [System.NonSerialized] public bool inMelee = false;
                [System.NonSerialized] private float coolDownCounter = 0;
                [System.NonSerialized] private ThePlayer.Melee meleeRef;

                #region 
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] public bool foldOut = false;
                [SerializeField, HideInInspector] public bool blockFoldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        melee.Initialize(collider2DRef);
                        block.Initialize(collider2DRef);

                }

                public void ResetAll ()
                {
                        inMelee = false;
                        attackNow = false;
                        coolDownCounter = 0;
                        melee.Reset();
                        block.Reset();
                }

                public void SkipCoolDown ()
                {
                        coolDownCounter = Mathf.Infinity;
                }

                public void CompleteAttack ()
                {
                        melee.AttackComplete();
                }

                public void SetReference (ThePlayer.Melee meleeRef)
                {
                        this.meleeRef = meleeRef;
                }

                public void Attack ()
                {
                        if (meleeRef == null)
                        {
                                return;
                        }
                        meleeRef.ChangeMeleeAttack(meleeName, !gameObject.activeInHierarchy);
                        attackNow = true;
                }

                public bool ActivateFromSleep ()
                {
                        return melee.attackFromSleep && melee.ValidateInputs(); //&& !this.gameObject.activeInHierarchy 
                }

                public bool MeleeIsActive (Character character, UserInputs inputs)
                {
                        if (attackNow)
                        {
                                melee.input.ButtonPressed();
                        }
                        if (attackNow)
                        {
                                melee.input2.ButtonPressed();
                        }
                        attackNow = false;

                        if (pause)
                        {
                                return false;
                        }

                        FlipCollider(character.signals.characterDirection); // check to flip every frame in case there is a visual element to it
                        if (!gameObject.activeInHierarchy)
                        {
                                return false;
                        }
                        bool isBlocking = block.IsBlocking();
                        if (!isBlocking && !melee.inMelee && collider2DRef != null && collider2DRef.enabled)
                        {
                                collider2DRef.enabled = false;
                        }
                        if (block.needToRelease && block.input.Released())
                        {
                                block.needToRelease = false;
                        }
                        if (Clock.TimerInverseExpired(ref coolDownCounter, coolDown))
                        {
                                if (coolDown > 0)
                                {
                                        onCoolDown.Invoke(Mathf.Clamp(coolDownCounter / coolDown, 0, 1f));
                                }
                                return false;
                        }
                        else if (coolDown > 0)
                        {
                                onCoolDown.Invoke(1f);
                        }
                        return inMelee || isBlocking || (!block.needToRelease && !inputs.block && melee.Attack());
                }

                public void Execute (AnimationSignals signals, int direction, bool onGround, bool crouching, Vector2 position, ref Vector2 velocity)
                {
                        if (block.canBlock && block.IsBlocking())
                        {
                                if (melee.inMelee && block.cancelCombo)
                                {
                                        melee.Reset();
                                }
                                if (!melee.inMelee)
                                {
                                        block.Block(signals, direction, onGround, ref velocity);
                                        return;
                                }
                        }

                        inMelee = true;

                        if (coolDownCounter >= coolDown && melee.ExecuteMelee(signals, direction, onGround, crouching, position, coolDown, ref velocity))
                        {
                                ResetAll();
                        }
                }

                public void FlipCollider (float playerDirection)
                {
                        transform.localPosition = Util.FlipXSign(transform.localPosition, playerDirection); // change weapon position x depending on side
                        Vector3 r = transform.localEulerAngles;
                        transform.localRotation = Quaternion.Euler(r.x, playerDirection < 0 ? 180f : 0f, r.z);
                }

                public void UnlockAll ()
                {
                        for (int i = 0; i < melee.combo.Count; i++)
                        {
                                if (melee.combo[i].isLocked)
                                {
                                        melee.combo[i].isLocked = false;
                                }
                        }
                }

                public void Unlock (int index)
                {
                        for (int i = 0; i < melee.combo.Count; i++)
                        {
                                if (i == index && melee.combo[i].isLocked)
                                {
                                        melee.combo[i].isLocked = false;
                                }
                        }
                }

                public void Lock (int index)
                {
                        for (int i = 0; i < melee.combo.Count; i++)
                        {
                                if (i == index)
                                {
                                        melee.combo[i].isLocked = true;
                                }
                        }
                }

                public void BeginVelX (bool value)
                {
                        melee.BeginVelX(value);
                }

                public void BeginVelY (bool value)
                {
                        melee.BeginVelY(value);
                }

                public void AttackFromSleep (bool value)
                {
                        melee.attackFromSleep = value;
                }

                public void Pause (bool value)
                {
                        pause = value;
                        ResetAll();
                }

        }
}
