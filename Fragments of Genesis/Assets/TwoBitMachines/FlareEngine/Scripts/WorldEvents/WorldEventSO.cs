using System.Collections.Generic;
using UnityEngine;

namespace TwoBitMachines.FlareEngine
{
        public class WorldEventSO : ScriptableObject
        {
                public string eventName = "";
                private List<WorldEventListener> listeners = new List<WorldEventListener>();

                public void TriggerEvent ()
                {
                        for (int i = listeners.Count - 1; i >= 0; i--)
                        {
                                if (listeners[i] == null)
                                {
                                        listeners.RemoveAt(i);
                                        continue;
                                }
                                listeners[i].EventTriggered();
                        }
                }

                public void ClearListeners ()
                {
                        if (listeners != null)
                        {
                                listeners.Clear();
                        }
                }

                public void RegisterListener (WorldEventListener listener)
                {
                        if (listeners != null)
                        {
                                listeners.Add(listener);
                        }
                }

                public void UnregisterListener (WorldEventListener listener)
                {
                        if (listeners != null)
                        {
                                listeners.Remove(listener);
                        }
                }
        }
}
