using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class RoundPosition : MonoBehaviour
        {
                [SerializeField] public float round = 0.5f;

                public void OnDrawGizmos ()
                {
                        transform.position = Compute.Round(transform.position, round);
                }
        }
}
