using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cowsins2D
{
    public class LevelSelectorPlayer : MonoBehaviour
    {
        [System.Serializable]
        public class LevelDataUI
        {
            public GameObject UI;
            public TextMeshProUGUI levelName, levelDescription, worldName, enterLevelKeyText;
        }

        [SerializeField] private Animator animator, levelAnimator;
        [SerializeField] private float movementSpeed = 5f;
        [SerializeField] private string worldName;
        [SerializeField] private LevelDataUI levelDataUI;
        [SerializeField] private AudioClip[] footstepsSFX;
        [SerializeField] private float footstepInterval = 0.15f, footstepVolume = 0.5f;
        [SerializeField] private AudioClip reachLevelSFX, loadSceneSFX;
        [SerializeField] private LevelNode startNode;

        private LevelNode currentNode;
        private AudioSource audioSource;
        private bool isMoving = false;

        public static PlayerActions inputActions;

        private void Start()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerActions();
                inputActions.Enable();
            }

            audioSource = GetComponent<AudioSource>();

            currentNode = startNode;
            transform.position = currentNode.transform.position;

            levelDataUI.worldName.text = worldName;
            UpdateUI();

            PlaySFX(reachLevelSFX);

            string device = DeviceDetection.Instance.mode == DeviceDetection.InputMode.Keyboard ? "Keyboard" : "Controller";
            levelDataUI.enterLevelKeyText.text = "Press " + inputActions.GameControls.Jumping.GetBindingDisplayString(InputBinding.MaskByGroup(device));
        }

        private void Update()
        {
            if (!isMoving)
            {
                Vector2 input = new Vector2(InputManager.PlayerInputs.HorizontalMovement, InputManager.PlayerInputs.VerticalMovement);

                if (input.x == 1 && currentNode.right != null && currentNode.right.isUnlocked)
                    MoveToNode(currentNode.right);
                else if (input.x == -1 && currentNode.left != null && currentNode.left.isUnlocked)
                    MoveToNode(currentNode.left);
                else if (input.y == 1 && currentNode.up != null && currentNode.up.isUnlocked)
                    MoveToNode(currentNode.up);
                else if (input.y == -1 && currentNode.down != null && currentNode.down.isUnlocked)
                    MoveToNode(currentNode.down);
                else
                    animator.SetTrigger("idle");

                if (InputManager.PlayerInputs.JumpingDown)
                {
                    currentNode.TriggerLevelEvent();
                }
            }
            else
            {
                animator.SetTrigger("move");
                levelDataUI.UI.SetActive(false);
            }
        }

        private void MoveToNode(LevelNode targetNode)
        {
            Vector3 targetPos = targetNode.transform.position;
            int moveDir = (targetPos.x - transform.position.x) > 0 ? 1 : -1;

            animator.SetTrigger("move");
            StartCoroutine(MovePlayer(targetPos, moveDir));
            currentNode = targetNode;
            UpdateUI();
        }

        private IEnumerator MovePlayer(Vector3 targetPosition, int moveDirection)
        {
            isMoving = true;

            // Flip player orientation
            Vector3 targetScale = new Vector3(moveDirection, transform.localScale.y, transform.localScale.z);
            animator.transform.parent.localScale = targetScale;

            float nextFootstepTime = Time.time;

            while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * movementSpeed);

                if (Time.time >= nextFootstepTime)
                {
                    PlayFootstep();
                    nextFootstepTime = Time.time + footstepInterval;
                }

                yield return null;
            }

            isMoving = false;
            if(!string.IsNullOrEmpty(levelDataUI.levelName.text))
            {
                levelDataUI.UI.SetActive(true);
                levelAnimator.SetTrigger("reached");
            }
            else levelDataUI.UI.SetActive(false);
            PlaySFX(reachLevelSFX);
        }

        private void PlayFootstep()
        {
            int rand = Random.Range(0, footstepsSFX.Length);
            PlaySFX(footstepsSFX[rand], footstepVolume);
        }

        private void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }

        private void UpdateUI()
        {
            levelDataUI.levelName.text = currentNode.levelName;
            levelDataUI.levelDescription.text = currentNode.levelDescription;
        }

        public void LoadScene(int sceneID)
        {
            StartCoroutine(LoadSceneGivenId(sceneID));
        }

        private IEnumerator LoadSceneGivenId(int id)
        {
            PlaySFX(loadSceneSFX);
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene(id);
        }
    }

#if UNITY_EDITOR

    [System.Serializable]
    [CustomEditor(typeof(LevelSelectorPlayer))]
    public class LevelSelectorPlayerEditor : Editor
    {
        private bool showGlobal = true;
        private bool showReferences = false;
        private bool showOthers = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            LevelSelectorPlayer myScript = target as LevelSelectorPlayer;

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                showGlobal = EditorGUILayout.Foldout(showGlobal, "GLOBAL", true);
                if (showGlobal)
                {
                    EditorGUI.indentLevel++;   
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("movementSpeed"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("worldName"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("startNode"));
                    EditorGUI.indentLevel--;   
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                showReferences = EditorGUILayout.Foldout(showReferences, "REFERENCES", true);
                if (showReferences)
                {
                    EditorGUI.indentLevel++;   
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("animator"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("levelAnimator"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("levelDataUI"));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();


            EditorGUILayout.Space(5);
            EditorGUILayout.BeginVertical(GUI.skin.GetStyle("HelpBox"));
            {
                showOthers = EditorGUILayout.Foldout(showOthers, "OTHERS", true);
                if (showOthers)
                {
                    EditorGUI.indentLevel++;   
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepsSFX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepInterval"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("footstepVolume"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("reachLevelSFX"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("loadSceneSFX"));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10f);
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
