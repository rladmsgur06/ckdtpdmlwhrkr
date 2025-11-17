using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class CircleCollider2DProperty : ExtraProperty
        {
                public bool useEnabled = false;
                public bool useDensity = false;
                public bool useIsTrigger = false;
                public bool useOffsetX = false;
                public bool useOffsetY = false;

                public bool useRadius = false;
                public bool useEffector = false;

                public bool interpolateOffset = false;
                public bool interpolateRadius = false;
                public bool interpolateDensity = false;

                public CircleCollider2D property;
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
                        property.offset = PropertyTool.SetV2 (property.offset, useOffsetX, useOffsetY, d.offsetX, d.offsetY);

                        if (useRadius) property.radius = d.radius;
                        if (useDensity) property.density = d.density;
                        if (useIsTrigger) property.isTrigger = d.isTrigger;
                        if (useEffector) property.usedByEffector = d.usedByEffector;
                        if (useEnabled) property.enabled = d.enabled;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.offsetX = property.offset.x;
                        original.offsetY = property.offset.y;
                        original.radius = property.radius;
                        original.density = property.density;
                        original.isTrigger = property.isTrigger;
                        original.usedByEffector = property.usedByEffector;
                        original.enabled = property.enabled;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];
                        if (interpolateOffset)
                        {
                                Vector2 value = property.offset;
                                if (useOffsetX) value.x = Mathf.Lerp (data1.offsetX, data2.offsetX, timer / duration);
                                if (useOffsetY) value.y = Mathf.Lerp (data1.offsetY, data2.offsetY, timer / duration);
                                property.offset = value;
                        }
                        if (useRadius && interpolateRadius)
                                property.radius = Mathf.Lerp (data1.radius, data2.radius, timer / duration);
                        if (useDensity && interpolateDensity)
                                property.density = Mathf.Lerp (data1.density, data2.density, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public float offsetX;
                        public float offsetY;
                        public float radius;
                        public float density;
                        public bool enabled = false;
                        public bool isTrigger = false;
                        public bool usedByEffector = false;

                }

        }
}