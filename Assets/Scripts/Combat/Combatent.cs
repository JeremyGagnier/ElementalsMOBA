using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combatent : PhysicsMover
{
    // Combat Variables
    public int health;
    public int maxJumps;
    public FInt jumpSpeed;
    [HideInInspector] public int jumps = 0;
    [HideInInspector] public bool blockingMove = false;

    // Combat Management
    [HideInInspector] public CombatManager manager = null;
    [HideInInspector] public List<Move> moves = null;

    public virtual void TriggerMoves()
    {
        foreach (Move move in moves)
        {
            move.Trigger(manager);
        }
    }
}
