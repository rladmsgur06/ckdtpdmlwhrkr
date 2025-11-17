using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class ChargedProjectile
        {
                [SerializeField] public WaitForAnimation waitForAnimation = new WaitForAnimation();
                [SerializeField] public Recoil recoil = new Recoil();
                [SerializeField] public bool canCharge = false;
                [SerializeField] public float chargeMaxTime = 2f;
                [SerializeField] public float chargeMinTime = 2f;
                [SerializeField] public float dischargeTime = 3f;
                [SerializeField] public float coolDownTime = 0f;
                [SerializeField] public float damageBoost = 0f;
                [SerializeField] public ProjectileBase projectile;
                [SerializeField] public RecoilOnDischarge recoilOnDischarge;

                [SerializeField] public UnityEventFloat onCoolingDown;
                [SerializeField] public UnityEventFloat onDischarging;
                [SerializeField] public UnityEventFloat onCharging;
                [SerializeField] public UnityEvent onDischargingComplete;
                [SerializeField] public UnityEvent onDischargingFailed;
                [SerializeField] public UnityEvent onChargingComplete;
                [SerializeField] public UnityEvent onChargingBegin;

                [SerializeField] private float percentCharged = 0;
                [System.NonSerialized] private bool isDischarging = false;
                [System.NonSerialized] private bool isCoolingDown = false;
                [System.NonSerialized] private float dischargeCounter = 0f;
                [System.NonSerialized] private float coolDownCounter = 0f;
                [System.NonSerialized] private float chargeCounter = 0f;
                [System.NonSerialized] private float counter = 0f;

                public bool recoilMany => recoilOnDischarge == RecoilOnDischarge.RecoilMany;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField, HideInInspector] private bool onDischargingCompleteFoldOut = false;
                [SerializeField, HideInInspector] private bool onDischargingFailedFoldOut = false;
                [SerializeField, HideInInspector] private bool onChargingCompleteFoldOut = false;
                [SerializeField, HideInInspector] private bool onChargingBeginFoldOut = false;
                [SerializeField, HideInInspector] private bool onDischargingFoldOut = false;
                [SerializeField, HideInInspector] private bool onChargingFoldOut = false;
                [SerializeField, HideInInspector] private bool onCoolDownFoldOut = false;
                [SerializeField, HideInInspector] private bool modifyFoldOut = false;
                [SerializeField, HideInInspector] private bool recoilFoldOut = false;
                [SerializeField, HideInInspector] private bool eventsFoldOut = false;
                [SerializeField, HideInInspector] private bool chargeFoldOut = false;
#pragma warning restore 0414
#endif
                #endregion

                public void Reset (bool clearPercent = true)
                {
                        recoil.Reset();
                        waitForAnimation.Reset();
                        recoil.canRecoil = false;
                        isDischarging = false;
                        isCoolingDown = false;
                        counter = 0;
                        chargeCounter = 0;
                        coolDownCounter = 0;
                        dischargeCounter = 0;
                        if (clearPercent)
                                percentCharged = 0;
                }

                public void Execute (Firearm fireArm, Character equipment, bool fire = true)
                {
                        if (waitForAnimation.AnimationNotPlaying(ref fire, ref counter, equipment.signals))
                        {
                                if (Discharge(equipment.signals) && fireArm.FireProjectile(projectile, recoil, true, ref counter))
                                {
                                        recoil.canRecoil = recoilMany;
                                }
                        }
                }

                public bool Charge (bool mouseOverUI, InputButtonSO input, AnimationSignals signals)
                {
                        if (!canCharge || projectile == null)
                                return false;

                        if (isDischarging)
                        {
                                return true;
                        }
                        if (isCoolingDown)
                        {
                                SignalAndEvent(signals, onCoolingDown, NormalizedTime(coolDownCounter, coolDownTime), "firearmIsCoolingDown");
                                if (Clock.Timer(ref coolDownCounter, coolDownTime))
                                {
                                        Reset();
                                }
                                return false;
                        }
                        if (input.Holding() && !mouseOverUI && projectile.EnoughAmmo()) // Charging
                        {
                                waitForAnimation.BlockIfReady();
                                if (percentCharged == 0)
                                {
                                        SignalAndEvent(signals, onChargingBegin, "onChargingBegin");
                                }
                                if (Clock.TimerExpired(ref chargeCounter, chargeMaxTime))
                                {
                                        percentCharged = 1;
                                        SignalAndEvent(signals, onChargingComplete, "firearmIsFullyCharged");
                                }
                                else if (chargeMaxTime > 0)
                                {
                                        percentCharged = chargeCounter / chargeMaxTime;
                                        SignalAndEvent(signals, onCharging, percentCharged, "firearmIsCharging");
                                }
                        }
                        if (input.Released())
                        {
                                if (chargeCounter >= chargeMinTime && projectile.EnoughAmmo())
                                {
                                        Reset(false);
                                        isDischarging = true;
                                        recoil.canRecoil = true;
                                        waitForAnimation.Unblock();
                                        return true;
                                }
                                Reset();
                        }
                        return false;
                }

                public bool Discharge (AnimationSignals signals, bool fire = true)
                {
                        float time = dischargeTime * percentCharged;
                        SignalAndEvent(signals, onDischarging, 1f - NormalizedTime(dischargeCounter, time), "firearmIsDischarging");
                        if (Clock.Timer(ref dischargeCounter, time)) // timer complete, must discharge at least once (provided there is enough ammo)
                        {
                                DischargeOver(failed: false);
                        }
                        return true;
                }

                public void DischargeOver (bool failed = true)
                {
                        if (!isDischarging)
                                return;
                        Reset();
                        isCoolingDown = coolDownTime > 0;
                        if (!failed)
                                onDischargingFailed.Invoke(); // charging failed
                        if (failed)
                                onDischargingComplete.Invoke();
                }

                private void SignalAndEvent (AnimationSignals signals, UnityEvent currentEvent, string signalName)
                {
                        signals.Set("chargeModeActive", true);
                        signals.Set(signalName, true);
                        currentEvent.Invoke();
                }

                private void SignalAndEvent (AnimationSignals signals, UnityEventFloat currentEvent, float value, string signalName)
                {
                        signals.Set("chargeModeActive", true);
                        signals.Set(signalName, true);
                        currentEvent.Invoke(value);
                }

                private float NormalizedTime (float counter, float time)
                {
                        return time == 0 ? 0 : counter / time;
                }
        }

        public enum RecoilOnDischarge
        {
                RecoilOnce,
                RecoilMany
        }
}
