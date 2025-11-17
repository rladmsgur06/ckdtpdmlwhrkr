using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        //Set sprite Manager to near last script
        public class SpriteManager : MonoBehaviour
        {
                private static SpriteManager instance;
                public static List<SpriteEngineBase> engineList = new List<SpriteEngineBase>();

                public static SpriteManager get
                {
                        get
                        {
                                if (instance == null)
                                {
                                        engineList.Clear();
                                        instance = new GameObject("SpriteManagerSE").AddComponent<SpriteManager>(); // each new scene will start fresh with a new SpriteManager since it's destroyed at the end of each scene
                                        instance.gameObject.hideFlags = HideFlags.HideInHierarchy;
                                }
                                return instance;
                        }
                }

                private void Start ()
                {
                        if (instance != null && instance != this)
                        {
                                Destroy(this.gameObject);
                        }
                }

                public void Register (SpriteEngineBase engine)
                {
                        if (!engineList.Contains(engine))
                        {
                                engineList.Add(engine);
                        }
                }

                private void LateUpdate ()
                {
                        for (int i = engineList.Count - 1; i >= 0; i--)
                        {
                                if (engineList[i] == null)
                                {
                                        engineList.RemoveAt(i);
                                }
                                else
                                {
                                        engineList[i].SetAlwaysSignals();
                                        engineList[i].Play();
                                }
                        }
                }

        }
}
