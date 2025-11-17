using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [System.Serializable]
        public class ExtraProperty
        {
                [SerializeField] public bool interpolate = false;
                [System.NonSerialized] public bool originalSaved = false;

                public virtual void SetState (int frameIndex, bool firstFrame = false) { }
                public virtual void Interpolate (int frameIndex, float duration, float timer) { }
                public virtual void ResetToOriginalState ( ) { }

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable 0414
                [SerializeField] public bool view;
                [SerializeField] public bool wasSet;
                [SerializeField] public bool canShow;
                [SerializeField] public bool delete;
                [SerializeField] public bool deleteAsk;
                [SerializeField] public bool alreadySaved;
                #pragma warning restore 0414
                #endif
                #endregion
        }
}