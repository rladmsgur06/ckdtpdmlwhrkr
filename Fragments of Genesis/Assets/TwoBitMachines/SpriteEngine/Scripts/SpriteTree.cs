using System;
using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.TwoBitSprite
{
        [Serializable]
        public class SpriteTree
        {
                [SerializeField] public List<Branch> spriteFlip = new List<Branch>();
                [SerializeField] public List<Branch> branch = new List<Branch>();
                [SerializeField] public List<string> signals = new List<string>();

                [NonSerialized] private string tempSprite;
                [NonSerialized] private Transform transform;
                [NonSerialized] public SpriteEngineBase engine;
                [NonSerialized] public Dictionary<string, bool> signal = new Dictionary<string, bool>(50);

                public static string[] spriteDirection = new string[] { "flipLeft", "flipRight", "flipUp", "flipDown" };

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
                #if UNITY_EDITOR
                #pragma warning disable format, 0414
                [SerializeField] private string search;
                [SerializeField] private bool active;
                [SerializeField] private bool createState;
                [SerializeField] private bool stateFoldOut;
                [SerializeField] private bool signalFoldOut;
                [SerializeField] private int signalIndex;
                [SerializeField] private int scrollIndex;
                [SerializeField] private StateSignals signalRef = new StateSignals();
                #pragma warning restore format, 0414
                #endif
                #endregion

                public void Initialize (SpriteEngineBase engine)
                {
                        this.engine = engine;
                }

                public void Initialize (SpriteEngineBase engine, Transform transform)
                {
                        this.transform = transform;
                        this.engine = engine;
                }

                public void Reset ()
                {
                        for (int i = 0; i < signals.Count; ++i)
                                signal[signals[i]] = false;
                }

                public void Set (string signal, bool value)
                {
                        this.signal[signal] = value;
                }

                public void SetSignalTrue (string signal)
                {
                        this.signal[signal] = true;
                }

                public void SetSignalFalse (string signal)
                {
                        this.signal[signal] = false;
                }

                public void FindNextAnimation ()
                {
                        if (engine == null)
                                return;

                        SearchTree(branch, true); // passing in delegates creates garbage, change to bool
                        SearchTree(spriteFlip, false);
                        engine.FinalizeAnimation(tempSprite);
                }

                public void ClearSignals ()
                {
                        signal.Clear();
                }

                #region Search Tree ... Since Unity has depth limit ... this is what we get
                private bool SearchTree (List<Branch> branch, bool setAnimation)
                {
                        for (int i = 0; i < branch.Count; i++)
                        {
                                Branch node = branch[i];
                                if (node.Empty())
                                {
                                        if (SetResult(node.signal, node.sprite, setAnimation))
                                        {
                                                return true;
                                        }
                                }
                                else if (SignalTrue(node.signal) && SearchTreeB(node.nodes, setAnimation)) //         If signal is true, check children nodes
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                private bool SearchTreeB (List<BranchB> branch, bool setAnimation)
                {
                        for (int i = 0; i < branch.Count; i++)
                        {
                                BranchB node = branch[i];
                                if (node.Empty())
                                {
                                        if (SetResult(node.signal, node.sprite, setAnimation))
                                        {
                                                return true;
                                        }
                                }
                                else if (SignalTrue(node.signal) && SearchTreeC(node.nodes, setAnimation)) //         If signal is true, check children nodes
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                private bool SearchTreeC (List<BranchC> branch, bool setAnimation)
                {
                        for (int i = 0; i < branch.Count; i++)
                        {
                                BranchC node = branch[i];
                                if (node.Empty())
                                {
                                        if (SetResult(node.signal, node.sprite, setAnimation))
                                        {
                                                return true;
                                        }
                                }
                                else if (SignalTrue(node.signal) && SearchTreeD(node.nodes, setAnimation)) //         If signal is true, check children nodes
                                {
                                        return true;
                                }
                        }
                        return false;
                }

                private bool SearchTreeD (List<BranchD> branch, bool setAnimation)
                {
                        for (int i = 0; i < branch.Count; i++)
                        {
                                BranchD node = branch[i];
                                if (SetResult(node.signal, node.sprite, setAnimation))
                                {
                                        return true;
                                }
                        }
                        return false;
                }
                #endregion

                private bool SetResult (string signalName, string spriteName, bool setAnimation)
                {
                        if (setAnimation)
                        {
                                return SetAnimation(signalName, spriteName);
                        }
                        else
                        {
                                return engine.FlipAnimation(signal, signalName, spriteName);
                        }
                }

                private bool SetAnimation (string signalName, string animationName)
                {
                        if (signal.TryGetValue(signalName, out bool value) && value)
                        {
                                tempSprite = animationName; //This is a place holder.
                        }
                        return value;
                }

                public bool SignalTrue (string signalName)
                {
                        return signal.TryGetValue(signalName, out bool value) && value;
                }

        }

        [System.Serializable]
        public class Branch
        {
                [SerializeField] public List<BranchB> nodes = new List<BranchB>();
                [SerializeField] public string sprite = "";
                [SerializeField] public string signal = "";
                public bool Empty () => nodes.Count == 0;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool add;
                [SerializeField] private bool active;
                [SerializeField] private bool delete;
                [SerializeField] private bool foldOut;
                [SerializeField] private int signalIndex;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class BranchB
        {
                [SerializeField] public List<BranchC> nodes = new List<BranchC>();
                [SerializeField] public string sprite = "";
                [SerializeField] public string signal = "";
                public bool Empty () => nodes.Count == 0;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool add;
                [SerializeField] private bool active;
                [SerializeField] private bool delete;
                [SerializeField] private bool foldOut;
                [SerializeField] private int signalIndex;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class BranchC
        {
                [SerializeField] public List<BranchD> nodes = new List<BranchD>();
                [SerializeField] public string sprite = "";
                [SerializeField] public string signal = "";
                public bool Empty () => nodes.Count == 0;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool add;
                [SerializeField] private bool active;
                [SerializeField] private bool delete;
                [SerializeField] private bool foldOut;
                [SerializeField] private int signalIndex;
#pragma warning restore 0414
#endif
                #endregion
        }

        [System.Serializable]
        public class BranchD
        {
                [SerializeField] public string sprite = "";
                [SerializeField] public string signal = "";
                public bool Empty () => true;

                #region ▀▄▀▄▀▄ Editor Variables ▄▀▄▀▄▀
#if UNITY_EDITOR
#pragma warning disable 0414
                [SerializeField] private bool add;
                [SerializeField] private bool active;
                [SerializeField] private bool delete;
                [SerializeField] private bool foldOut;
                [SerializeField] private int signalIndex;
#pragma warning restore 0414
#endif
                #endregion
        }
}
