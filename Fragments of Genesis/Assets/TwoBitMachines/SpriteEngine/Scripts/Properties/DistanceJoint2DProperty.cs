using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class DistanceJoint2DProperty : ExtraProperty
        {

                public bool useAnchor = false;
                public bool useDistance = false;
                public bool useConfigAnchor = false;
                public bool useConfigDistance = false;
                public bool useEnableCollision = false;
                public bool useMaxDistanceOnly = false;
                public bool useEnabled = false;

                public bool interpolateAnchor = false;
                public bool interpolateDistance = false;

                public DistanceJoint2D property;
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
                        if (useAnchor) property.anchor = d.anchor;
                        if (useDistance) property.distance = d.distance;
                        if (useConfigAnchor) property.autoConfigureConnectedAnchor = d.configAnchor;
                        if (useConfigDistance) property.autoConfigureDistance = d.configDistance;
                        if (useEnableCollision) property.enableCollision = d.enableCollision;
                        if (useMaxDistanceOnly) property.maxDistanceOnly = d.maxDistanceOnly;
                        if (useEnabled) property.enabled = d.enabled;

                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.anchor = property.anchor;
                        original.distance = property.distance;
                        original.configAnchor = property.autoConfigureConnectedAnchor;
                        original.configDistance = property.autoConfigureDistance;
                        original.enableCollision = property.enableCollision;
                        original.maxDistanceOnly = property.maxDistanceOnly;
                        original.enabled = property.enabled;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];

                        if (useAnchor && interpolateAnchor)
                                property.anchor = Vector2.Lerp (data1.anchor, data2.anchor, timer / duration);
                        if (useDistance && interpolateDistance)
                                property.distance = Mathf.Lerp (data1.distance, data2.distance, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public Vector2 anchor;
                        public float distance;
                        public bool configAnchor;
                        public bool configDistance;
                        public bool enableCollision;
                        public bool maxDistanceOnly;
                        public bool enabled;
                }
        }
}