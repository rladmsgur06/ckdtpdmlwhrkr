using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class Rigidbody2DProperty : ExtraProperty
        {
                public bool useAngularDrag;
                public bool useGravityScale;
                public bool useLinearDrag;
                public bool useMass;
                public bool useAutoMass;
                public bool useSimulated;
                public bool useKinematicContacts;

                public bool interpolateAngularDrag;
                public bool interpolateGravityScale;
                public bool interpolateLinearDrag;
                public bool interpolateMass;

                public Rigidbody2D property;
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
                        if (useGravityScale) property.gravityScale = d.gravityScale;
                        if (useLinearDrag) property.drag = d.linearDrag;
                        if (useMass) property.mass = d.mass;
                        if (useAutoMass) property.useAutoMass = d.useAutoMass;
                        if (useSimulated) property.simulated = d.simulated;
                        if (useKinematicContacts) property.useFullKinematicContacts = d.kinematicContacts;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.angularDrag = property.angularDrag;
                        original.gravityScale = property.gravityScale;
                        original.linearDrag = property.drag;
                        original.mass = property.mass;
                        original.useAutoMass = property.useAutoMass;
                        original.simulated = property.simulated;
                        original.kinematicContacts = property.useFullKinematicContacts;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];
                        if (useAngularDrag && interpolateAngularDrag)
                                property.angularDrag = Mathf.Lerp (data1.angularDrag, data2.angularDrag, timer / duration);
                        if (useGravityScale && interpolateGravityScale)
                                property.gravityScale = Mathf.Lerp (data1.gravityScale, data2.gravityScale, timer / duration);
                        if (useLinearDrag && interpolateLinearDrag)
                                property.drag = Mathf.Lerp (data1.linearDrag, data2.linearDrag, timer / duration);
                        if (useMass && interpolateMass)
                                property.mass = Mathf.Lerp (data1.mass, data2.mass, timer / duration);

                }

                [System.Serializable]
                public class Data
                {
                        public float angularDrag;
                        public float gravityScale;
                        public float linearDrag;
                        public float mass;
                        public bool useAutoMass;
                        public bool simulated;
                        public bool kinematicContacts;
                }

        }
}