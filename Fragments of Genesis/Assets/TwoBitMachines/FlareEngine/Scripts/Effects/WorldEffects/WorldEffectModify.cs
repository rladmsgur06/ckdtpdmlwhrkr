using TMPro;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [System.Serializable]
        public class WorldEffectModify
        {
                [SerializeField] public WorldEffectType type;
                [SerializeField] public EffectPosition position;
                [SerializeField] public float yOffset = 0f;
                [SerializeField] public bool useRandomX;
                [SerializeField] public bool useRandomY;
                [SerializeField] public bool useRandomRotation;
                [SerializeField] public bool flipX;
                [SerializeField] public float randomRotationMin = 0f;
                [SerializeField] public float randomRotationMax = 0f;
                [SerializeField] public float randomXOffsetMin = 0f;
                [SerializeField] public float randomXOffsetMax = 0f;
                [SerializeField] public float randomYOffsetMin = 0f;
                [SerializeField] public float randomYOffsetMax = 0f;
                [SerializeField] public bool checkForWalls = false;

                public void Activate (GameObject gameObject, ImpactPacket impact)
                {
                        Transform transform = gameObject.transform;
                        if (type == WorldEffectType.TextMeshPro)
                        {
                                TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
                                if (text != null)
                                        text.SetText(impact.damageValue.ToString());
                        }
                        else if (type == WorldEffectType.TextMeshProNoSign)
                        {
                                TextMeshPro text = gameObject.GetComponent<TextMeshPro>();
                                if (text != null)
                                        text.SetText(Mathf.Abs(impact.damageValue).ToString());
                        }
                        else if (type == WorldEffectType.LetsWiggle)
                        {
                                LetsWiggle wiggle = gameObject.GetComponent<LetsWiggle>();
                                if (wiggle != null)
                                        wiggle.Activate(impact);
                        }

                        // position
                        if (position == EffectPosition.Bottom)
                        {
                                transform.position = impact.bottomPosition;
                        }
                        else if (position == EffectPosition.Center)
                        {
                                transform.position = impact.Center();
                        }
                        else
                        {
                                transform.position = impact.Top();
                        }
                        if (yOffset != 0)
                        {
                                transform.position += Vector3.up * yOffset;
                        }
                        if (useRandomX)
                        {
                                float xDirection = impact.direction.x == 0 ? 1f : impact.direction.x;
                                transform.position += Vector3.right * xDirection * Random.Range(randomXOffsetMin, randomXOffsetMax);
                        }
                        if (useRandomY)
                        {
                                transform.position += Vector3.up * Random.Range(randomYOffsetMin, randomYOffsetMax);

                        }
                        if (useRandomRotation)
                        {
                                Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(randomRotationMin, randomRotationMax));
                                transform.rotation *= randomRotation;
                        }
                        if (flipX)
                        {
                                transform.localEulerAngles = new Vector3(0, impact.directionX > 0 ? 0 : 180f, 0);
                        }
                        if (checkForWalls)
                        {
                                if (Physics2D.OverlapPoint(transform.position, WorldManager.collisionMask))
                                {
                                        transform.position = impact.Center();
                                }
                        }
                }
        }

        public enum EffectPosition
        {
                Bottom,
                Center,
                Top
        }

        public enum WorldEffectType
        {
                Normal,
                TextMeshPro,
                TextMeshProNoSign,
                LetsWiggle
        }
}
