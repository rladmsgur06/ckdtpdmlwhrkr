using UnityEngine;
using UnityEngine.Events; 

namespace cowsins2D
{
    public class Teleporter : Trigger
    {
        [System.Serializable]
        public enum TeleportMethod
        {
            ToPosition,
            ToTeleport
        }

        [System.Serializable]
        public class Events // Store your events
        {
            public UnityEvent OnTeleport;
        }
        [SerializeField, Tooltip("onfigures the behaviour of the TP. You can either tp to specific locations or to another TP. "), Title("Teleport", upMargin = 10)]
        private TeleportMethod teleportMethod;

        [SerializeField, Tooltip("Position to TP to. ")] private Vector3 toPosition;

        [SerializeField, Tooltip("Teleport to TP to.")] private Teleporter toTeleport;

        [SerializeField] private Events events;

        public delegate void OnTeleport(GameObject target);

        private OnTeleport onTeleport;

        public bool usable { get; private set; } = true;

        private void Awake()
        {
            // Set up the events
            onTeleport = teleportMethod == TeleportMethod.ToPosition ? ToPositionTP : ToTP;
        }
        public override void EnterTrigger(GameObject target)
        {
            // If the triggerable is unable to be use right now, do not perform any operation.
            if (!usable) return;

            // Handle teleportation
            onTeleport?.Invoke(target);
            events.OnTeleport?.Invoke();
            base.EnterTrigger(target);
        }

        // Handle TP to specific Locations ( Vector3 in World Position )
        private void ToPositionTP(GameObject target)
        {
            target.transform.position = toPosition;
        }

        // Handles movement from this TP to a target Tp
        private void ToTP(GameObject target)
        {
            target.transform.position = toTeleport.transform.position;
            toTeleport.usable = false;
            toTeleport.Invoke(nameof(ResetUsage), .1f);

        }

        private void ResetUsage() => usable = true;

    }

}