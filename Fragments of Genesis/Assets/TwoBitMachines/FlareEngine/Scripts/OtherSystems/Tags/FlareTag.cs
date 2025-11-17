using UnityEngine;
using System.Collections.Generic;

namespace TwoBitMachines.FlareEngine
{
        public class FlareTag : MonoBehaviour
        {
                [SerializeField] public TagListSO tagListSO;
                [SerializeField] public List<string> tags = new List<string>();

                public bool Contains (string id)
                {
                        return tags.Contains(id);
                }

                public void AddTag (string tag)
                {
                        if (!tags.Contains(tag))
                        {
                                tags.Add(tag);
                        }
                }

                public static bool ObjectHasTag (Transform transform, string tag)
                {
                        FlareTag flareTag = transform == null ? null : transform.gameObject.GetComponent<FlareTag>();
                        return flareTag == null ? false : flareTag.Contains(tag);
                }
        }
}
