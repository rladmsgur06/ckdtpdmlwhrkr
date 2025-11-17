using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class TransformProperty : ExtraProperty
        {
                public bool usePosition = false;
                public bool useScale = false;
                public bool useRotation = false;

                public bool usepX = false;
                public bool usepY = false;
                public bool usepZ = false;

                public bool usesX = false;
                public bool usesY = false;
                public bool usesZ = false;

                public bool useeX = false;
                public bool useeY = false;
                public bool useeZ = false;

                public bool interpolatePosition = false;
                public bool interpolateScale = false;
                public bool interpolateRotation = false;

                public Transform property;
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
                        Vector3 p = property.localPosition;
                        Vector3 s = property.localScale;
                        Vector3 e = property.localEulerAngles;

                        if (usepX) p.x = d.pX;
                        if (usepY) p.y = d.pY;
                        if (usepZ) p.z = d.pZ;
                        property.localPosition = p;

                        if (usesX) s.x = d.sX;
                        if (usesY) s.y = d.sY;
                        if (usesZ) s.z = d.sZ;
                        property.localScale = s;

                        if (useeX) e.x = d.eX;
                        if (useeY) e.y = d.eY;
                        if (useeZ) e.z = d.eZ;
                        property.localEulerAngles = e;
                }

                public void SaveOriginalState ( )
                {
                        if (property == null) return;
                        original.pX = property.localPosition.x;
                        original.pY = property.localPosition.y;
                        original.pZ = property.localPosition.z;

                        original.sX = property.localScale.x;
                        original.sY = property.localScale.y;
                        original.sZ = property.localScale.z;

                        original.eX = property.localEulerAngles.x;
                        original.eY = property.localEulerAngles.y;
                        original.eZ = property.localEulerAngles.z;
                        originalSaved = true;
                }

                public override void Interpolate (int frameIndex, float duration, float timer)
                {
                        if (property == null || frameIndex >= data.Count || duration == 0) return;

                        Data data1 = data[frameIndex];
                        Data data2 = data[frameIndex + 1 >= data.Count ? 0 : frameIndex + 1];

                        if (interpolatePosition)
                        {
                                Vector3 p = property.localPosition;
                                if (usepX) p.x = Mathf.Lerp (data1.pX, data2.pX, timer / duration);
                                if (usepY) p.y = Mathf.Lerp (data1.pY, data2.pY, timer / duration);
                                if (usepZ) p.z = Mathf.Lerp (data1.pZ, data2.pZ, timer / duration);
                                property.localPosition = p;
                        }

                        if (interpolatePosition)
                        {
                                Vector3 s = property.localScale;
                                if (usesX) s.x = Mathf.Lerp (data1.sX, data2.sX, timer / duration);
                                if (usesY) s.y = Mathf.Lerp (data1.sY, data2.sY, timer / duration);
                                if (usesZ) s.z = Mathf.Lerp (data1.sZ, data2.sZ, timer / duration);
                                property.localScale = s;
                        }

                        if (interpolateRotation)
                        {
                                Vector3 e = property.localEulerAngles;
                                if (useeX) e.x = Mathf.Lerp (data1.eX, data2.eX, timer / duration);
                                if (useeY) e.y = Mathf.Lerp (data1.eY, data2.eY, timer / duration);
                                if (useeZ) e.z = Mathf.Lerp (data1.eZ, data2.eZ, timer / duration);
                                property.localEulerAngles = e;
                        }
                }

                [System.Serializable]
                public class Data
                {
                        public float pX = 0;
                        public float pY = 0;
                        public float pZ = 0;

                        public float sX = 0;
                        public float sY = 0;
                        public float sZ = 0;

                        public float eX = 0;
                        public float eY = 0;
                        public float eZ = 0;
                }
        }
}