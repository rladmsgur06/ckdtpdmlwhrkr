using System.Collections.Generic;
using System.Text;
using TwoBitMachines.FlareEngine;
using UnityEngine;

public class DebugAllSignals : MonoBehaviour
{
    public Character character;      // Player에 붙이면 자동으로 찾아오도록 처리

    Dictionary<string, bool> signals;

    void Awake()
    {
        if (character == null)
            character = GetComponent<Character>();

        if (character == null)
        {
            Debug.LogError("[DebugAllSignals] Character 를 찾지 못했습니다.");
            enabled = false;
            return;
        }

        signals = character.signals.signals;
    }

    void LateUpdate()
    {
        if (signals == null || signals.Count == 0)
            return;

        var on  = new StringBuilder();
        var off = new StringBuilder();

        foreach (var kv in signals)
        {
            if (kv.Value)
                on.Append(kv.Key).Append(", ");
            else
                off.Append(kv.Key).Append(", ");
        }

        Debug.Log(
            $"[FRAME {Time.frameCount}] " +
            $"ON: {on.ToString()}  |  OFF: {off.ToString()}"
        );
    }
}