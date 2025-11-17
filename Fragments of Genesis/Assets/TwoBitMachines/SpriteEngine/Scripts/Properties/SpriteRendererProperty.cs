using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class SpriteRendererProperty : ExtraProperty
        {
                public bool useAdaptiveMode = false;
                public bool useRenderPriority = false;
                public bool useSortingOrder = false;
                public bool useReceiveShadows = false;
                public bool useColor = false;
                public bool useSize = false;
                public bool useEnabled = false;
                public bool useFlipX = false;
                public bool useFlipY = false;

                public bool interpolateColor = false;
                public bool interpolateSize = false;

                public SpriteRenderer property;
                public Data original = new Data ( );
                public List<Data> data = new List<Data> ( );

                public override void SetState (int frameIndex, bool firstFrame = false)
                {
                        if (frameIndex >= data.Count) return;
                        if (firstFrame) SaveOriginalState ( );
                        Set (data[frameIndex]);
                }

                public override void ResetToOriginalState ( )
                {
                        if (originalSaved) Set (original);
                        originalSaved = false;
                }

                private void Set (Data d)
                {
                        if (property == null) return;
                        if (useColor) property.color = d.color;
                        if (useSize) property.size = d.size;
                        if (useRenderPriority) property.rendererPriority = d.renderPriority;
                        if (useSortingOrder) property.sortingOrder = d.sortingOrder;
                        if (useAdaptiveMode) property.adaptiveModeThreshold = d.adaptiveMode;
                        if (useReceiveShadows) property.receiveShadows = d.receiveShadows;
                        if (useEnabled) property.enabled = d.enabled;
                        if (useFlipX) property.flipX = d.flipX;
                        if (useFlipY) property.flipY = d.flipY;

                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.color = property.color;
                        original.size = property.size;
                        original.renderPriority = property.rendererPriority;
                        original.sortingOrder = property.sortingOrder;
                        original.receiveShadows = property.receiveShadows;
                        original.adaptiveMode = property.adaptiveModeThreshold;
                        original.enabled = property.enabled;
                        original.flipX = property.flipX;
                        original.flipY = property.flipY;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];

                        if (useColor && interpolateColor)
                                property.color = Color.Lerp (data1.color, data2.color, timer / duration);
                        if (useSize && interpolateSize)
                                property.size = Vector2.Lerp (data1.size, data2.size, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public Color color;
                        public Vector2 size;
                        public int renderPriority;
                        public int sortingOrder;
                        public float adaptiveMode;
                        public bool receiveShadows;
                        public bool enabled;
                        public bool flipX;
                        public bool flipY;
                }
        }
}