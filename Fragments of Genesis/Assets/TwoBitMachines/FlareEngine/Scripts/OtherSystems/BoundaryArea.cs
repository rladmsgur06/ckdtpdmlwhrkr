using UnityEngine;

/// <summary>
/// Create 4 collider to limit the player movement
/// Need a Boundary area 
/// 
/// </summary>
namespace TwoBitMachines.FlareEngine
{
        public class BoundaryArea : MonoBehaviour
        {
                [HideInInspector] public SimpleBounds SimpleBounds;
        }
}