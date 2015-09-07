using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combatent : MonoBehaviour
{
    // Combat Variables
    public int health = 0;
    public bool blockingMove = false;

    // Combat Management
    public CombatManager manager = null;
    public List<Move> moves = null;

    public virtual void TriggerMoves()
    {
        foreach (Move move in moves)
        {
            if (move.Trigger(manager))
            {
                manager.moves.Add(move);
            }
        }
    }
}
