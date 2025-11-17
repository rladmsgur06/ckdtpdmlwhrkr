using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class AreaEffector2DProperty : ExtraProperty
        {
                public bool useAngularDrag;
                public bool useDrag;
                public bool useForceAngle;
                public bool useForceMagnitude;
                public bool useForceVariation;
                public bool useColliderMask;
                public bool useGlobalAngle;
                public bool useEnabled;

                public bool interpolateAngularDrag;
                public bool interpolateDrag;
                public bool interpolateForceAngle;
                public bool interpolateForceMagnitude;
                public bool interpolateForceVariation;

                public AreaEffector2D property;
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
                        if (useAngularDrag) property.angularDrag = d.angularDrag;
                        if (useDrag) property.drag = d.drag;
                        if (useForceAngle) property.forceAngle = d.forceAngle;
                        if (useForceMagnitude) property.forceMagnitude = d.forceMagnitude;
                        if (useForceVariation) property.forceVariation = d.forceVariation;
                        if (useColliderMask) property.useColliderMask = d.useColliderMask;
                        if (useGlobalAngle) property.useGlobalAngle = d.useGlobalAngle;
                        if (useEnabled) property.enabled = d.enabled;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.angularDrag = property.angularDrag;
                        original.drag = property.drag;
                        original.forceAngle = property.forceAngle;
                        original.forceMagnitude = property.forceMagnitude;
                        original.forceVariation = property.forceVariation;
                        original.useColliderMask = property.useColliderMask;
                        original.useGlobalAngle = property.useGlobalAngle;
                        original.enabled = property.enabled;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];

                        if (useAngularDrag && interpolateAngularDrag)
                                property.angularDrag = Mathf.Lerp (data1.angularDrag, data2.angularDrag, timer / duration);
                        if (useDrag && interpolateDrag)
                                property.drag = Mathf.Lerp (data1.drag, data2.drag, timer / duration);
                        if (useForceAngle && interpolateForceAngle)
                                property.forceAngle = Mathf.Lerp (data1.forceAngle, data2.forceAngle, timer / duration);
                        if (useForceMagnitude && interpolateForceMagnitude)
                                property.forceMagnitude = Mathf.Lerp (data1.forceMagnitude, data2.forceMagnitude, timer / duration);
                        if (useForceVariation && interpolateForceVariation)
                                property.forceVariation = Mathf.Lerp (data1.forceVariation, data2.forceVariation, timer / duration);
                }

                [System.Serializable]
                public class Data
                {
                        public float angularDrag;
                        public float drag;
                        public float forceAngle;
                        public float forceMagnitude;
                        public float forceVariation;
                        public bool useColliderMask;
                        public bool useGlobalAngle;
                        public bool enabled;
                }
        }
}