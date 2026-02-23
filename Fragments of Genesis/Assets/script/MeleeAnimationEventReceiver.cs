using TwoBitMachines.FlareEngine.ThePlayer;
using UnityEngine;

/// <summary>
/// 공격 애니메이션에서 Animation Event로 호출하는 리시버.
/// 이 프로젝트는 한 가지 공격 모션만 사용하므로,
/// 애니 끝에서 콤보를 강제로 완료시켜 공격 상태를 벗어나게 만든다.
/// </summary>
public class MeleeAnimationEventReceiver : MonoBehaviour
{
    [Tooltip("플레이어에 붙어있는 ThePlayer.Melee Ability (비워두면 자동 검색)")]
    public Melee meleeAbility;

    void Awake()
    {
        // 인스펙터에서 안 넣어주면, 같은 오브젝트에서 자동으로 찾기
        if (meleeAbility == null)
            meleeAbility = GetComponent<Melee>();
    }

    /// <summary>
    /// 공격 애니메이션의 '마지막 프레임'에 Animation Event로 호출.
    /// </summary>
    public void OnAttackAnimationEnd()
    {
        if (meleeAbility == null)
            return;

        // 현재 활성화된 무기의 공격을 완료 처리 → 콤보/공격 상태 종료
        meleeAbility.CompleteAttack();
    }
}