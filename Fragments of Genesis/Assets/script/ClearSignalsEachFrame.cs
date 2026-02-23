using TwoBitMachines.FlareEngine;
using UnityEngine;

public class ClearSignalsEachFrame : MonoBehaviour
{
    public Character character;

    void Awake()
    {
        if (character == null)
            character = GetComponent<Character>();

        if (character == null)
        {
            Debug.LogError("[ClearSignalsEachFrame] Character 를 찾지 못했습니다.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (character == null)
            return;

        // 다른 프로젝트에서 SpriteEngine.Play() → tree.ClearSignals() 로 하던 일을 대신 수행
        character.signals.ClearSignals();
    }
}