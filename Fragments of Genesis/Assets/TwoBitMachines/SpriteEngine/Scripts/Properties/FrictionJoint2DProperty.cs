using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class FrictionJoint2DProperty : ExtraProperty
        {
                public bool useAnchor = false;
                public bool useMaxForce = false;
                public bool useMaxTorque = false;
                public bool useConfigAnchor = false;
                public bool useEnableCollision = false;
                public bool useEnabled = false;

                public bool interpolateAnchor = false;
                public bool interpolateMaxForce = false;
                public bool interpolateMaxTorque = false;

                public FrictionJoint2D property;
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
                        if (useMaxForce) property.maxForce = d.maxForce;
                        if (useMaxTorque) property.maxTorque = d.maxTorque;
                        if (useConfigAnchor) property.autoConfigureConnectedAnchor = d.configAnchor;
                        if (useEnableCollision) property.enableCollision = d.enableCollision;
                        if (useEnabled) property.enabled = d.enabled;

                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.anchor = property.anchor;
                        original.maxForce = property.maxForce;
                        original.maxTorque = property.maxTorque;
                        original.configAnchor = property.autoConfigureConnectedAnchor;
                        original.enableCollision = property.enableCollision;
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
                        if (useMaxForce && interpolateMaxForce)
                                property.maxForce = Mathf.Lerp (data1.maxForce, data2.maxForce, timer / duration);
                        if (useMaxTorque && interpolateMaxTorque)
                                property.maxTorque = Mathf.Lerp (data1.maxTorque, data2.maxTorque, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public Vector2 anchor;
                        public float maxForce;
                        public float maxTorque;
                        public bool configAnchor;
                        public bool enableCollision;
                        public bool enabled;
                }
        }
}