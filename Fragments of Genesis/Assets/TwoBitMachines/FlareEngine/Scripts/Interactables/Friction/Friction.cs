using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [AddComponentMenu ("Flare Engine/一Interactables/Friction")]
        public class Friction : MonoBehaviour
        {
                [SerializeField] public FrictionType type;
                [SerializeField] public float friction = 0.25f;
                [SerializeField] public float slideSpeed = 5f;
                [SerializeField] public float autoSpeed = 5f;
                [SerializeField] private bool foldOut;
        }

        public enum FrictionType
        {
                Friction,
                Slide,
                Auto
        }
}