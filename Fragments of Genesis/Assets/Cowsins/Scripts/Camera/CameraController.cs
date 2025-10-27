using UnityEngine;
using UnityEditor;

namespace cowsins2D
{
    public class CameraController : MonoBehaviour
    {
        [System.Serializable]
        public enum CameraControllerMethod
        {
            Static,
            FollowTarget,
            FollowTargetWhenCloseToBounds
        };

        [Tooltip("Configures the style of the camera, the way it behaves")] public CameraControllerMethod cameraMethod;

        [SerializeField, Tooltip("What object does this camera follow.")] private Transform target;

        [SerializeField, Tooltip("Camera position variation")] private Vector3 cameraOffset;

        [SerializeField, Tooltip("Size of the camera vision. Once the target is outside of these boundaries, it will start following the target.")] private Vector2 boundary;

        [SerializeField, Tooltip("The lower, the faster it will reach the destination.")] private float cameraLaziness;

        [SerializeField] private float fovSmoothness;


        private float fov;

        private Camera cam;

        private void Awake()
        {
            cam = transform.GetChild(0).GetComponent<Camera>();
            fov = cam.fieldOfView;
        }

        private void Update()
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, fov, fovSmoothness * Time.deltaTime);
        }


        // Called in LateUpdate for proper smoothness
        private void LateUpdate()
        {
            // Depending on the camera method, call different functions
            switch (cameraMethod)
            {
                case CameraControllerMethod.FollowTarget:
                    SimpleFollowTarget();
                    break;
                case CameraControllerMethod.FollowTargetWhenCloseToBounds:
                    BoundsFollow();
                    break;
            }
        }

        // Simply lerp the position of this object from the current one to the target and add the camera offset
        private void SimpleFollowTarget() => transform.position = Vector3.Lerp(transform.position, target.position + (Vector3)cameraOffset, Time.deltaTime * 1 / cameraLaziness);

        // Only follow if out of bounds
        private void BoundsFollow()
        {
            Vector2 dif = Vector2.zero;

            // Grab the distance in the horizontal axis from this to the target
            float difX = target.position.x - transform.position.x;


            // If this distance is greater than the boundary horizontal size ( left or right )
            if (difX > boundary.x || difX < -boundary.x)
            {
                if (transform.position.x < target.position.x) dif.x = difX - boundary.x;
                else dif.x = difX + boundary.x;
            }

            // Same process for the vertical axis (y)
            float difY = target.position.y - transform.position.y;

            if (difY > boundary.y || difY < -boundary.y)
            {
                if (transform.position.y < target.position.y) dif.y = difY - boundary.y;
                else dif.y = difY + boundary.x;
            }

            // Lerp the position
            transform.position = Vector3.Lerp(transform.position, transform.position + (Vector3)dif, Time.deltaTime * 1 / cameraLaziness);
        }

        public void CameraToStatic() => cameraMethod = CameraControllerMethod.Static;

        public void CameraToSimple() => cameraMethod = CameraControllerMethod.FollowTarget;

        public void CameraToBoundary() => cameraMethod = CameraControllerMethod.FollowTargetWhenCloseToBounds;
    }


#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            CameraController myScript = target as CameraController;

            #region variables
            GUILayout.Space(20);
            EditorGUILayout.LabelField("CAMERA CONTROLLER", EditorStyles.boldLabel);
            GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            GUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraMethod"));
            if (myScript.cameraMethod == CameraController.CameraControllerMethod.FollowTargetWhenCloseToBounds)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("boundary"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.PropertyField(serializedObject.FindProperty("target"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraOffset"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("cameraLaziness"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("fovSmoothness"));
            #endregion



            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}
