using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        [AddComponentMenu ("Flare Engine/一Saving/TransformTracker")]
        public class TransformTracker : MonoBehaviour
        {
                [SerializeField] public string key = "name";
                [SerializeField] private SaveTransformList saveTransforms = new SaveTransformList ( );
                [System.NonSerialized] public static List<TransformTracker> tracker = new List<TransformTracker> ( );

                public static TransformTracker get;

                private void Start ( )
                {
                        get = this;
                        Restore ( );
                }

                private void OnEnable ( )
                {
                        if (!tracker.Contains (this))
                        {
                                tracker.Add (this);
                        }
                }

                private void OnDisable ( )
                {
                        if (tracker.Contains (this))
                        {
                                tracker.Remove (this);
                        }
                        get = null;
                }

                public static void Reset ( )
                {
                        for (int i = 0; i < tracker.Count; i++)
                        {
                                if (tracker[i] == null) continue;
                                tracker[i].Restore ( );
                        }
                }

                public bool Contains (Transform transform)
                {
                        return saveTransforms.Contains (transform);
                }

                public void AddToList (Transform transform)
                {
                        saveTransforms.AddToList (transform);
                }

                public void AddToList (ImpactPacket packet)
                {
                        saveTransforms.AddToList (packet.transform);
                }

                public void Save ( )
                {
                        Storage.Save (saveTransforms, WorldManager.saveFolder, key);
                }

                private void Restore ( )
                {
                        saveTransforms.ClearAll ( );
                        saveTransforms = Storage.Load<SaveTransformList> (saveTransforms, WorldManager.saveFolder, key);
                }

        }

        [System.Serializable]
        public class SaveTransformList
        {
                public List<Transform> list = new List<Transform> ( );

                public void ClearAll ( )
                {
                        list.Clear ( );
                }

                public bool Contains (Transform transform)
                {
                        return list.Contains (transform);
                }

                public void AddToList (Transform transform)
                {
                        if (!list.Contains (transform))
                        {
                                list.Add (transform);
                        }
                }
        }
}