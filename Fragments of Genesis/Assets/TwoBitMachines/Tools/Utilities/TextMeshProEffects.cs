using System;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TwoBitMachines
{
        public class TextMeshProEffects : MonoBehaviour
        {
                [SerializeField] private TextMeshProUGUI textMesh;
                [SerializeField] private bool startOnEnable = true;

                [SerializeField] private int typewriterFade = 5;
                [SerializeField] private float typewriterSpeed = 5.5f;
                [SerializeField] private float typewriterWobble = 2f;

                [SerializeField] private float wobble = 1f;
                [SerializeField] private float wobbleSpeed = 1f;

                [SerializeField] private float waveSpeed = 2f;
                [SerializeField] private float wavePhase = 0.01f;
                [SerializeField] private float waveStrength = 0.1f;

                [SerializeField] private float waveSpeedX = 2f;
                [SerializeField] private float wavePhaseX = 0.01f;
                [SerializeField] private float waveStrengthX = 0.1f;

                [SerializeField] private float jitterStrength = 0.1f;
                [SerializeField] private float jitterRate = 0.01f;

                [SerializeField] private float distortionStrength = 0.1f;
                [SerializeField] private float distortionRate = 0.1f;

                [SerializeField] public string worldEffect = "";
                [SerializeField] public float typingRate = 1f;
                [SerializeField] public UnityEventEffect onTyping = new UnityEventEffect();
                [SerializeField] private UnityEvent onComplete = new UnityEvent();

                [System.NonSerialized] private Vector3[] oldVertices;
                [System.NonSerialized] private int currentCharacter;
                [System.NonSerialized] private int characterRange;
                [System.NonSerialized] private float distortionCounter;
                [System.NonSerialized] private float pauseCounter;
                [System.NonSerialized] private float pauseLimit;
                [System.NonSerialized] private float speedOrigin;
                [System.NonSerialized] private float jitterCounter;
                [System.NonSerialized] private float typingCounter;
                [System.NonSerialized] private float counter;
                [System.NonSerialized] private bool typeWriterOn;
                [System.NonSerialized] private bool pauseDetected;
                [System.NonSerialized] private bool pauseTypeWriter;
                [System.NonSerialized] private ITypeWriterComplete dialogueRef;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] public ShowType showType;
                [SerializeField] private bool completeFoldOut;
                [SerializeField] private bool eventsFoldOut;
                [SerializeField] private bool typingFoldOut;
#pragma warning restore 0414
#endif
                #endregion

                private void Awake ()
                {
                        speedOrigin = typewriterSpeed;
                }

                void OnEnable ()
                {
                        if (startOnEnable)
                        {
                                BeginTextMeshEffects();
                        }
                }

                public void BeginTextMeshEffects ()
                {
                        if (dialogueRef == null)
                        {
                                dialogueRef = gameObject.GetComponent<ITypeWriterComplete>();
                        }

                        dialogueRef?.TypingCommence();
                        textMesh.ForceMeshUpdate();
                        oldVertices = textMesh.mesh.vertices;
                        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0);
                        counter = 0;
                        pauseCounter = 0;
                        characterRange = 0;
                        currentCharacter = 0;
                        typingCounter = 10000f;
                        typeWriterOn = true;
                        pauseDetected = false;
                        pauseTypeWriter = false;
                        typewriterSpeed = speedOrigin;
                }

                private void Update ()
                {
                        if (oldVertices == null)
                        {
                                return; // not ready yet
                        }

                        bool update = false;
                        TypeWriter(ref update);
                        Effects(ref update);
                        if (update) //                      only update if changes have been detected
                        {
                                textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
                        }
                }

                private void TypeWriter (ref bool update)
                {
                        if (pauseTypeWriter && Clock.Timer(ref pauseCounter, pauseLimit))
                        {
                                pauseTypeWriter = false;
                                pauseDetected = false;
                                TMP_TextInfo textInfo = textMesh.textInfo;
                                if (characterRange + 1 < textInfo.characterCount)
                                {
                                        characterRange += 1; // increase here or system can go back into pause mode!
                                }
                        }
                        if (!pauseTypeWriter && typeWriterOn && Clock.Timer(ref typingCounter, typingRate))
                        {
                                ImpactPacket impact = ImpactPacket.impact.Set(worldEffect, this.transform, null, this.transform.position, null, Vector2.zero, 1, 0);
                                onTyping.Invoke(impact);
                        }

                        float rate = (10f - typewriterSpeed) * 0.01f + 0.01f;
                        if (typeWriterOn && Clock.Timer(ref counter, rate))
                        {
                                update = true;
                                TMP_TextInfo textInfo = textMesh.textInfo;
                                int characterCount = textInfo.characterCount;
                                float fadeSteps = Mathf.Max(1f, 255f / typewriterFade);

                                for (int i = currentCharacter; i < characterRange + 1; i++)
                                {
                                        int materialIndex = textInfo.characterInfo[i].materialReferenceIndex; // get the index of the material used by the current character.
                                        int index = textInfo.characterInfo[i].vertexIndex; //                    get the index of the first vertex used by this text element.
                                        Color32[] colors = textInfo.meshInfo[materialIndex].colors32; //         get the vertex colors of the mesh used by this text element (character or sprite).                 
                                        Vector3[] vertices = textMesh.textInfo.meshInfo[materialIndex].vertices;

                                        if (i > 0 && colors[0].a == 0)
                                        {
                                                SetAlpha(colors, 0, (byte) 255f);
                                                SetVertices(vertices, 0, Vector3.zero);
                                        }

                                        float lerp = Mathf.Clamp(colors[index + 0].a + fadeSteps, 0, 255f);

                                        if (!textInfo.characterInfo[i].isVisible)
                                        {
                                                if (i == currentCharacter && ++currentCharacter >= characterCount)
                                                {
                                                        Complete();
                                                }
                                                continue;
                                        }

                                        float angle = Mathf.Lerp(0, 180f, lerp / 255f);
                                        SetAlpha(colors, index, (byte) lerp);
                                        SetVertices(vertices, index, Vector3.up * Mathf.Sin(angle * Mathf.Deg2Rad) * typewriterWobble);

                                        if (lerp >= 255f && ++currentCharacter >= characterCount)
                                        {
                                                Complete();
                                        }
                                }

                                if (!pauseTypeWriter && characterRange + 1 < characterCount)
                                {
                                        characterRange += 1;
                                }
                        }
                }

                private void Effects (ref bool update)
                {
                        TMP_TextInfo textInfo = textMesh.textInfo;
                        if (textInfo == null || textInfo.lineInfo == null)
                                return;

                        for (int l = 0; l < textInfo.linkCount; l++)
                        {
                                update = true;
                                TMP_LinkInfo link = textInfo.linkInfo[l];
                                int startIndex = link.linkTextfirstCharacterIndex;
                                int range = startIndex + link.linkTextLength;
                                string linkID = link.GetLinkID();

                                if (linkID.Contains("jitter"))
                                {
                                        Jitter(startIndex, range, textInfo);
                                }
                                else if (linkID.Contains("wobble"))
                                {
                                        Wobble(startIndex, range, textInfo);
                                }
                                else if (linkID.Contains("distortion"))
                                {
                                        Distortion(startIndex, range, textInfo);
                                }
                                else if (linkID.Contains("waveX"))
                                {
                                        WaveX(startIndex, range, textInfo);
                                }
                                else if (linkID.Contains("wave"))
                                {
                                        Wave(startIndex, range, textInfo);
                                }
                                else if (!pauseDetected && (characterRange + 1) == startIndex && linkID.Contains("pause")) //+ 1 so that pause occurs before the first letter of the word
                                {
                                        // string[] time = linkID.Split (new string[] { "pause" }, System.StringSplitOptions.None);
                                        pauseTypeWriter = true;
                                        pauseDetected = true;
                                        pauseCounter = 0;
                                        string time = Regex.Replace(linkID, "[^0-9]", "");
                                        pauseLimit = float.Parse(time, CultureInfo.InvariantCulture);
                                }
                                else if (!pauseDetected && (characterRange + 1) == startIndex && linkID.Contains("speed"))
                                {
                                        string[] time = linkID.Split(new string[] { "speed" }, System.StringSplitOptions.None);
                                        typewriterSpeed = float.Parse(time[1], CultureInfo.InvariantCulture);
                                }
                                // if pause and speed are adjacent in the string message, then speed must come first, or else it will never be set
                        }
                }

                private void Wobble (int startIndex, int range, TMP_TextInfo textInfo) //(float time)
                {
                        for (int i = startIndex; i < range; i++)
                        {
                                if (ValidCharacter(i, textInfo, out TMP_CharacterInfo c))
                                {
                                        continue;
                                }
                                float t = Time.time + i;
                                Vector2 effect = new Vector2(Mathf.Sin(t * wobbleSpeed), Mathf.Cos(t * wobbleSpeed)) * wobble;
                                SetVertices(textInfo.meshInfo[c.materialReferenceIndex].vertices, c.vertexIndex, effect);
                        }
                }

                private void Wave (int startIndex, int range, TMP_TextInfo textInfo)
                {
                        for (int i = startIndex; i < range; i++)
                        {
                                if (ValidCharacter(i, textInfo, out TMP_CharacterInfo c))
                                {
                                        continue;
                                }
                                Vector2 effect = new Vector2(0, Mathf.Sin(Time.time * waveSpeed + oldVertices[c.vertexIndex].x * wavePhase) * waveStrength);
                                SetVertices(textInfo.meshInfo[c.materialReferenceIndex].vertices, c.vertexIndex, effect);
                        }
                }

                private void WaveX (int startIndex, int range, TMP_TextInfo textInfo)
                {
                        for (int i = startIndex; i < range; i++)
                        {
                                if (ValidCharacter(i, textInfo, out TMP_CharacterInfo c))
                                {
                                        continue;
                                }
                                Vector2 effect = new Vector2(Mathf.Sin(Time.time * waveSpeedX + oldVertices[c.vertexIndex].x * wavePhaseX) * waveStrengthX, 0);
                                SetVertices(textInfo.meshInfo[c.materialReferenceIndex].vertices, c.vertexIndex, effect);
                        }
                }

                private void Jitter (int startIndex, int range, TMP_TextInfo textInfo)
                {
                        if (!Clock.Timer(ref jitterCounter, jitterRate))
                                return;

                        for (int i = startIndex; i < range; i++)
                        {
                                if (ValidCharacter(i, textInfo, out TMP_CharacterInfo c))
                                {
                                        continue;
                                }
                                Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0);
                                SetVertices(textInfo.meshInfo[c.materialReferenceIndex].vertices, c.vertexIndex, offset * jitterStrength);
                        }
                }

                private void Distortion (int startIndex, int range, TMP_TextInfo textInfo)
                {
                        if (!Clock.Timer(ref distortionCounter, distortionRate))
                                return;

                        for (int i = startIndex; i < range; i++)
                        {
                                if (ValidCharacter(i, textInfo, out TMP_CharacterInfo c))
                                {
                                        continue;
                                }
                                Vector3[] vertices = textInfo.meshInfo[c.materialReferenceIndex].vertices;
                                int index = c.vertexIndex;
                                for (int j = 0; j < 4; j++)
                                {
                                        Vector3 offset = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0);
                                        vertices[index + j] = oldVertices[index + j] + offset * distortionStrength;
                                }
                        }
                }

                private bool ValidCharacter (int i, TMP_TextInfo textInfo, out TMP_CharacterInfo charInfo)
                {
                        charInfo = textInfo.characterInfo[i];
                        return charInfo.character == ' ' || !charInfo.isVisible;
                }

                private void SetVertices (Vector3[] vertices, int index, Vector3 offset)
                {
                        vertices[index + 0] = oldVertices[index + 0] + offset;
                        vertices[index + 1] = oldVertices[index + 1] + offset;
                        vertices[index + 2] = oldVertices[index + 2] + offset;
                        vertices[index + 3] = oldVertices[index + 3] + offset;
                }

                private void SetAlpha (Color32[] colors, int index, byte alpha)
                {
                        colors[index + 0].a = alpha;
                        colors[index + 1].a = alpha;
                        colors[index + 2].a = alpha;
                        colors[index + 3].a = alpha;
                }

                private void Complete ()
                {
                        typeWriterOn = false;
                        onComplete.Invoke();
                        if (dialogueRef != null)
                        {
                                dialogueRef.TypingComplete();
                        }
                }

                public void SetTypeWriterSpeed (float speed)
                {
                        typewriterSpeed = speed;
                }
        }

        public interface ITypeWriterComplete
        {
                public void TypingComplete ();
                public void TypingCommence ();
        }

        [Flags]
        public enum ShowType
        {
                Wobble = 1 << 0,
                Wave = 1 << 1,
                WaveX = 1 << 2,
                Jitter = 1 << 3,
                Distortion = 1 << 4
        }
}
