#region 
#if UNITY_EDITOR
using TwoBitMachines.Editors;
using UnityEditor;
#endif
#endregion
using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.AI
{
        [AddComponentMenu("")]
        public class SceneDoorExit : Action
        {
                [SerializeField] public Player player;
                [SerializeField] public ManageScenes manageScenes;
                [SerializeField] public SceneDoorDirection directional;
                [SerializeField] public string sceneName;
                [SerializeField] public int doorIndex;
                [SerializeField] public Vector3 area;
                [SerializeField] public bool playAnimation;
                [SerializeField] public string signalName;
                [SerializeField] public float signalTime;
                [SerializeField] public bool onButtonPress;
                [SerializeField] public InputButtonSO inputButtonSO;

                [System.NonSerialized] private float counter = 0;
                [System.NonSerialized] private bool found;

                public override NodeState RunNodeLogic (Root root)
                {
                        if (nodeSetup == NodeSetup.NeedToInitialize)
                        {
                                counter = 0;
                                found = false;
                                if (player == null)
                                {
                                        player = ThePlayer.Player.mainPlayer;
                                }
                        }
                        if (found)
                        {
                                if (playAnimation)
                                {
                                        WorldManager.get.BlockPlayerInput(true);
                                        player.signals.Set(signalName);
                                        if (TwoBitMachines.Clock.TimerExpired(ref counter, signalTime))
                                        {
                                                LoadScene(root);
                                        }
                                        return NodeState.Running;
                                }
                                LoadScene(root);
                                return NodeState.Success;
                        }
                        if (player == null || manageScenes == null || WorldManager.get == null)
                        {
                                return NodeState.Failure;
                        }
                        if (!InsideArea(player.transform.position + Vector3.up * 0.1f))
                        {
                                return NodeState.Failure;
                        }
                        if (onButtonPress && inputButtonSO != null && !inputButtonSO.Pressed())
                        {
                                return NodeState.Failure;
                        }
                        found = true;
                        return NodeState.Running;
                }

                private void LoadScene (Root root)
                {
                        int direction = directional == SceneDoorDirection.Fixed ? 1 : (int) Mathf.Sign(player.abilities.playerDirection);
                        WorldManager.get.save.sceneDoor = doorIndex * direction;
                        WorldManager.get.save.sceneDoorPlayerDirection = player.abilities.playerDirection;
                        manageScenes.LoadScene(sceneName);
                }

                private bool InsideArea (Vector2 target)
                {
                        Vector2 position = this.transform.position - Vector3.right * area.z;
                        if (target.x > (position.x + (area.x * 0.5f)) || target.x < (position.x - (area.x * 0.5f)))
                        {
                                return false;
                        }
                        if (target.y > (position.y + area.y) || target.y < position.y)
                        {
                                return false;
                        }
                        return true;
                }

                public enum SceneDoorDirection
                {
                        Fixed,
                        Directional
                }

                #region ▀▄▀▄▀▄ Custom Inspector ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                public static string[] sceneNames = new string[] { "Empty" };
                public override bool HasNextState ()
                {
                        return !playAnimation;
                }
                public override bool OnInspector (AIBase ai, SerializedObject parent, Color color, bool onEnable)
                {
                        if (onEnable)
                        {
                                int sceneCount = Util.SceneCount();
                                if (sceneCount > 0)
                                {
                                        sceneNames = new string[sceneCount];
                                        for (int i = 0; i < sceneCount; i++)
                                        {
                                                sceneNames[i] = Util.GetSceneName(i);
                                        }
                                }
                                else
                                {
                                        Debug.Log("Include scenes into Build Settings to use SceneDoorExit");
                                }
                        }
                        if (parent.Bool("showInfo"))
                        {
                                Labels.InfoBoxTop(125,
                                        "If the player enters the door area, the specified scene will load by calling the manage scenes component." +
                                        " If Door Entry is Directional, the player will enter the next door moving in the direction it exit." +
                                        " The area's z value is treated as an x offset for the door area. If player is empty, the system will use the first player it finds." +
                                        "\n \nReturns Failure, Running, Success");
                        }

                        FoldOut.Box(7, color, offsetY: -2);
                        {
                                parent.Field("Door Index", "doorIndex");
                                parent.Field("Door Area", "area");
                                parent.Field("Door Entry", "directional");
                                parent.FieldAndDropDownList(sceneNames, "Manage Scenes", "manageScenes", "sceneName");
                                parent.Field("Player", "player");
                                parent.FieldDoubleAndEnable("Animation", "signalName", "signalTime", "playAnimation");
                                Labels.FieldDoubleText("Signal", "Time", rightSpacing: 18, show1: parent.String("signalName") == "", show2: parent.Float("signalTime") == 0);
                                parent.FieldAndEnable("On Button Press", "inputButtonSO", "onButtonPress");
                        }
                        Layout.VerticalSpacing(3);
                        return true;
                }

                public override void OnSceneGUI (UnityEditor.Editor editor)
                {
                        Draw.Square(transform.position - Vector3.right * (area.x * 0.5f + area.z), area, Color.blue);
                }

#pragma warning restore 0414
#endif
                #endregion

        }

}
