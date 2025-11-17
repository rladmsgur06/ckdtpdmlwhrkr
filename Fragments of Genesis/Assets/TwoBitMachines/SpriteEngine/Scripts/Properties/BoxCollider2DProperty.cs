using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class BoxCollider2DProperty : ExtraProperty
        {
                public bool useEnabled = false;
                public bool useDensity = false;
                public bool useIsTrigger = false;
                public bool useOffsetX = false;
                public bool useOffsetY = false;
                public bool useSizeX = false;
                public bool useSizeY = false;
                public bool useEffector = false;
                public bool useEdgeRadius = false;

                public bool interpolateOffset = false;
                public bool interpolateSize = false;
                public bool interpolateEdgeRadius = false;
                public bool interpolateDensity = false;

                public BoxCollider2D property;
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
                        property.size = PropertyTool.SetV2 (property.size, useSizeX, useSizeY, d.sizeX, d.sizeY);
                        if (useDensity) property.density = d.density;
                        if (useEdgeRadius) property.edgeRadius = d.edgeRadius;
                        if (useIsTrigger) property.isTrigger = d.isTrigger;
                        if (useEffector) property.usedByEffector = d.usedByEffector;
                        if (useEnabled) property.enabled = d.enabled;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.offsetX = property.offset.x;
                        original.offsetY = property.offset.y;
                        original.sizeX = property.size.x;
                        original.sizeY = property.size.y;
                        original.density = property.density;
                        original.edgeRadius = property.edgeRadius;
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
                        if (interpolateSize)
                        {
                                Vector2 value = property.size;
                                if (useSizeX) value.x = Mathf.Lerp (data1.sizeX, data2.sizeX, timer / duration);
                                if (useSizeY) value.y = Mathf.Lerp (data1.sizeY, data2.sizeY, timer / duration);
                                property.size = value;
                        }
                        if (useEdgeRadius && interpolateEdgeRadius)
                                property.edgeRadius = Mathf.Lerp (data1.edgeRadius, data2.edgeRadius, timer / duration);
                        if (useDensity && interpolateDensity)
                                property.density = Mathf.Lerp (data1.density, data2.density, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public float sizeX;
                        public float sizeY;
                        public float offsetX;

                        public float offsetY;
                        public float density;
                        public float edgeRadius;
                        public bool enabled = false;
                        public bool isTrigger = false;
                        public bool usedByEffector = false;
                }
        }

        public static class PropertyTool
        {

                public static Vector2 SetV2 (Vector2 property, bool useX, bool useY, float x, float y)
                {
                        Vector2 value = property;
                        if (useX) value.x = x;
                        if (useY) value.y = y;
                        return value;
                }

                public static Vector3 SetV3 (Vector2 property, bool useX, bool useY, bool useZ, float x, float y, float z)
                {
                        Vector3 value = property;
                        if (useX) value.x = x;
                        if (useY) value.y = y;
                        if (useZ) value.z = z;
                        return value;
                }

        }

}