using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class ConstantForce2DProperty : ExtraProperty
        {
                public bool useForce = false;
                public bool useRelativeForce = false;
                public bool useTorque = false;
                public bool useEnabled = false;

                public bool interpolateForce = false;
                public bool interpolateRelativeForce = false;
                public bool interpolateTorque = false;

                public ConstantForce2D property;
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
                        if (useForce) property.force = d.force;
                        if (useRelativeForce) property.relativeForce = d.relativeForce;
                        if (useTorque) property.torque = d.torque;
                        if (useEnabled) property.enabled = d.enabled;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.force = property.force;
                        original.relativeForce = property.relativeForce;
                        original.torque = property.torque;
                        original.enabled = property.enabled;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];

                        if (useForce && interpolateForce)
                                property.force = Vector2.Lerp (data1.force, data2.force, timer / duration);
                        if (useRelativeForce && interpolateRelativeForce)
                                property.relativeForce = Vector2.Lerp (data1.relativeForce, data2.relativeForce, timer / duration);
                        if (useTorque && interpolateTorque)
                                property.torque = Mathf.Lerp (data1.torque, data2.torque, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public Vector2 force;
                        public Vector2 relativeForce;
                        public float torque;
                        public bool enabled;
                }
        }
}