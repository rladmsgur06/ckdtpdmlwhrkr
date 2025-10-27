using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cowsins2D
{
    public class LevelNode : MonoBehaviour
    {
        public string levelName;
        public string levelDescription;
        public UnityEvent levelEvent;
        public bool isUnlocked = true;

        public LevelNode up;
        public LevelNode down;
        public LevelNode left;
        public LevelNode right;

        public void TriggerLevelEvent() => levelEvent?.Invoke();
    }

#if UNITY_EDITOR

    [System.Serializable]
    [CustomEditor(typeof(LevelNode))]
    public class LevelNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelNode myScript = target as LevelNode;
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDescription"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isUnlocked"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("levelEvent"));

            if (myScript.up == myScript)
                EditorGUILayout.HelpBox("Up node cannot be the same as this node!", MessageType.Error);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("up"));

            if (myScript.down == myScript)
                EditorGUILayout.HelpBox("Down node cannot be the same as this node!", MessageType.Error);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("down"));

            
            if (myScript.left == myScript)
                EditorGUILayout.HelpBox("Left node cannot be the same as this node!", MessageType.Error);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("left"));
            
            
            if (myScript.right == myScript)
                EditorGUILayout.HelpBox("Right node cannot be the same as this node!", MessageType.Error);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("right"));

            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}