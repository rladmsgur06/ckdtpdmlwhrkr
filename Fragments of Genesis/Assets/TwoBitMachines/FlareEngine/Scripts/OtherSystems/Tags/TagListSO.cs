using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        // [CreateAssetMenu(menuName = "FlareEngine/TagListSO")]
        public class TagListSO : ScriptableObject
        {
                [SerializeField] public List<string> tags = new List<string>();
                [SerializeField] public bool editorOpen;
        }
}
