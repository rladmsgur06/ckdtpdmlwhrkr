using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Events;

namespace cowsins2D
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [ReadOnly] public float health;

        [SerializeField, Tooltip("Initial & Maximum Health")] private float maxHealth;

        [ReadOnly] public float shield;

        [SerializeField, Tooltip("Initial & Maximum Shield")] private float maxShield;

        [SerializeField, Tooltip("UI Elements to display health & shield")] private Image healthSlider, shieldSlider;

        [SerializeField, Tooltip("Visual effect instantiated on Death")] private GameObject deathVFX;

        public bool isDead { get; private set; } = false;

        [Tooltip("Set to true if you want the player to blink/flash on hit.")] public bool flashesOnDamage;

        [SerializeField, Tooltip("How long the effect is played.")] private float flashDuration;

        [SerializeField, Tooltip("How fast the effect is played.")] private float flashSpeed;

        [SerializeField, Tooltip("Array that contains all the Renderers that will blink on damage. These must have the Blink material assigned.")] protected Renderer[] graphics;

        public UnityEvent onDamage, onDie, onRestartStats;

        private float blinkValue;

        private void Start()
        {
            // Set initial statistics
            RestartStats();
        }

        private void Update()
        {
            // Handle the sliders
            HandleUI();
        }

        public virtual void Damage(float damage)
        {
            if (shield > 0)
            {
                // If there's shield, deduct damage from shield
                shield -= damage;
                if (shield < 0)
                {
                    // If shield is depleted, deduct the remaining damage from health
                    health += shield; // Adding the negative shield value to health
                    shield = 0;
                }
            }
            else
            {
                // If there's no shield, deduct damage directly from health
                health -= damage;
            }



            // Handle damage flash effect
            if (flashesOnDamage)
                FlashDamage();

            onDamage?.Invoke();

            // Handle death
            Die(health <= 0);
        }

        public virtual void Die(bool condition)
        {
            // If the condition is met, handle the death
            if (!condition) return;

            onDie?.Invoke();

            isDead = true;
            // Instantiate the effects and then destroy this object
            PoolManager.Instance.GetFromPool(deathVFX, transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }

        public void FlashDamage()
        {
            // Stop flashing and reset the flash value
            StopCoroutine(IFlashDamage());
            blinkValue = 0;

            // Restart flash
            StartCoroutine(IFlashDamage());
        }
        public IEnumerator IFlashDamage()
        {
            // While the time is not over, iterate through each object in the sprites array
            // Change the blink value for each of these.
            var elapsedTime = 0f;
            while (elapsedTime <= flashDuration)
            {
                for (int i = 0; i < graphics.Length; i++)
                {
                    var material = graphics[i].material;
                    material.SetFloat("_FlashAmount", blinkValue);
                    blinkValue = Mathf.PingPong(elapsedTime * flashSpeed, 1f);
                    elapsedTime += Time.deltaTime;
                }
                yield return null;
            }
            // re iterate through each object and reset the material value
            for (int i = 0; i < graphics.Length; i++)
            {
                var material = graphics[i].material;
                material.SetFloat("_FlashAmount", 0);
            }

        }

        private void RestartStats()
        {
            health = maxHealth;
            shield = maxShield;

            onRestartStats?.Invoke();
        }

        private void HandleUI()
        {
            if (healthSlider != null)
                healthSlider.fillAmount = Mathf.Lerp(healthSlider.fillAmount, health / maxHealth, Time.deltaTime * 3);

            if (healthSlider != null)
                shieldSlider.fillAmount = Mathf.Lerp(shieldSlider.fillAmount, shield / maxShield, Time.deltaTime * 3);
        }
    }
#if UNITY_EDITOR
    [System.Serializable]
    [CustomEditor(typeof(EnemyHealth))]
    public class EnemyHealthEditor : Editor
    {
        private string[] tabs = { "Basic", "UI", "Effects", "Events" };
        private int currentTab = 0;
        private Texture btnTexture;
        private GUIContent button;
        override public void OnInspectorGUI()
        {
            serializedObject.Update();
            EnemyHealth myScript = target as EnemyHealth;

            Texture2D myTexture = Resources.Load<Texture2D>("CustomEditor/enemy_CustomEditor") as Texture2D;
            GUILayout.Label(myTexture);

            EditorGUILayout.BeginVertical();
            currentTab = GUILayout.Toolbar(currentTab, tabs);
            EditorGUILayout.Space(10f);
            EditorGUILayout.EndVertical();
            #region variables

            if (currentTab >= 0 || currentTab < tabs.Length)
            {
                switch (tabs[currentTab])
                {

                    case "Basic":
                        EditorGUILayout.LabelField("MAIN SETTINGS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("health"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shield"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxShield"));
                        break;
                    case "UI":
                        EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("healthSlider"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("shieldSlider"));
                        break;
                    case "Effects":
                        EditorGUILayout.LabelField("VISUAL EFFECTS", EditorStyles.boldLabel);
                        GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
                        GUILayout.Space(20);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("deathVFX"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("flashesOnDamage"));
                        if (myScript.flashesOnDamage)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flashDuration"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("flashSpeed"));
                            EditorGUILayout.PropertyField(serializedObject.FindProperty("graphics"));
                            EditorGUI.indentLevel--;
                        }
                        break;
                    case "Events":
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("onDamage"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("onDie"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("onRestartStats"));
                        break;

                }
                GUILayout.Space(20);
                GUILayout.Box("", new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(8) });
            }

            #endregion



            serializedObject.ApplyModifiedProperties();

        }
    }
#endif
}