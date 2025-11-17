using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        public class AINodeEditors
        {
                public static void Clock (UnityEngine.Object obj)
                {
                        SerializedObject parent = new SerializedObject (obj);
                        parent.Update ( );

                        parent.ApplyModifiedProperties ( );
                }

        }
}