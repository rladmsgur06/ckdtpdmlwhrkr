using UnityEditor;

namespace TwoBitMachines.FlareEngine.Editors
{
        [CustomEditor (typeof (ProjectileClean))]
        public class ProjectileCleanEditor : UnityEditor.Editor
        {
                public override void OnInspectorGUI ( )
                {
                        ProjectileClean main = target as ProjectileClean;

                        if (main == null) return;

                        if (main.gameObject.GetComponent<Projectile> ( ) != null)
                        {
                                DestroyImmediate (main); //If it exists, destroy clean script
                                return;
                        }

                        ProjectileBase[] allChildren = main.transform.GetComponents<ProjectileBase> ( );
                        for (int i = 0; i < allChildren.Length; i++)
                        {
                                DestroyImmediate (allChildren[i]);
                        }

                        DestroyImmediate (main);
                }
        }
}