using UnityEngine;

public class Ladder : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        var player = col.GetComponent<TwoBitMachines.FlareEngine.ThePlayer.Player>();
        if (player != null)
        {
            player.GetComponent<PlayerLadderAbility>()?.EnterLadder();
        }
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        var player = col.GetComponent<TwoBitMachines.FlareEngine.ThePlayer.Player>();
        if (player != null)
        {
            player.GetComponent<PlayerLadderAbility>()?.ExitLadder();
        }
    }
}