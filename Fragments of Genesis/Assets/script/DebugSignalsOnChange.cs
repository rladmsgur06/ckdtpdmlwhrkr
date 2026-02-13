using System.Collections.Generic;
using TwoBitMachines.FlareEngine;
using UnityEngine;

public class DebugSignalsOnChange : MonoBehaviour
{
    public Character character;

    Dictionary<string, bool> signals;
    Dictionary<string, bool> prev = new Dictionary<string, bool>();

    void Awake()
    {
        if (character == null)
            character = GetComponent<Character>();

        signals = character.signals.signals;
    }

    void LateUpdate()
    {
        foreach (var kv in signals)
        {
            prev.TryGetValue(kv.Key, out bool p);

            if (p != kv.Value)
            {
                Debug.Log($"[SIGNAL {(kv.Value ? "ON " : "OFF")}] {kv.Key}");
            }

            prev[kv.Key] = kv.Value;
        }
    }
}
