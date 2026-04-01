using UnityEngine;
using TwoBitMachines.FlareEngine.ThePlayer;

public class PlayerLadderAbility : MonoBehaviour
{
    public float climbSpeed = 5f;

    private Player player;
    private bool isInLadder = false;
    private bool isClimbing = false;

    void Awake()
    {
        player = GetComponent<Player>();
    }

    public void EnterLadder()
    {
        isInLadder = true;
    }

    public void ExitLadder()
    {
        isInLadder = false;
        isClimbing = false;
    }

    void Update()
    {
        if (!isInLadder)
            return;

        float vertical = player.inputs.vertical;

        if (Mathf.Abs(vertical) > 0.1f)
        {
            isClimbing = true;
        }

        // 점프로 탈출
        if (isClimbing && player.inputs.jumpPressed)
        {
            isClimbing = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (!isClimbing)
            return;

        float vertical = player.inputs.vertical;

        // 🔥 핵심: Ability 시스템 우회해서 직접 이동 제어
        Vector2 climbVelocity = new Vector2(0, vertical * climbSpeed);

        player.Control(climbVelocity.x, false, true);
    }

    public bool IsClimbing()
    {
        return isClimbing;
    }
}