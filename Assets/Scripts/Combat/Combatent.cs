using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combatent : MonoBehaviour
{
    public CombatManager manager = null;
    public List<Move> moves = null;

    public virtual void TriggerMoves()
    {
        foreach (Move move in moves)
        {
            if (move.trigger())
            {
                manager.moves.Add(move);
            }
        }
    }
}
