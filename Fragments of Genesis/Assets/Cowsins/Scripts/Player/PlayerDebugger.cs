using UnityEngine;

namespace cowsins2D
{
    public class PlayerDebugger : MonoBehaviour
    {
        private PlayerStates playerStates;

        private void Awake()
        {
            playerStates = GetComponent<PlayerStates>();
        }

        private void OnGUI()
        {
            if (playerStates == null) return;

            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.alignment = TextAnchor.UpperLeft;
            boxStyle.fontSize = 14;
            boxStyle.normal.textColor = Color.white;

            GUILayout.BeginArea(new Rect(10, 10, 300, 32), GUI.skin.box);
            GUILayout.Label("Current State: " + playerStates.CurrentState, boxStyle);
            GUILayout.EndArea();
        }
    }
}
