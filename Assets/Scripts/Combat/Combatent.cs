using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Combatent : MonoBehaviour
{
    // Combat Variables
    public int health = 0;
    public bool blockingMove = false;
    public bool facingLeft = false;

    // Physics variables
    public PhysicsMover phys = null;

    // Combat Management
    public CombatManager manager = null;
    public List<Move> moves = null;

    public bool Grounded
    {
        get { return phys.grounded; }
    }

    public bool AllowInput
    {
        get { return phys.allowInput; }
        set { phys.allowInput = value; }
    }

    public bool FacingRight
    {
        get { return phys.facingRight; }
    }

    public FInt Mass
    {
        get { return phys.mass; }
    }

    public FVector Position
    {
        get { return phys.position; }
    }

    void Start()
    {
        phys = GetComponent<PhysicsMover>();
    }

    public virtual void TriggerMoves()
    {
        foreach (Move move in moves)
        {
            move.Trigger(manager);
        }
    }
}
