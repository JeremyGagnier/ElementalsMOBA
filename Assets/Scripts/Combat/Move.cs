using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 *  Properties of a move: 
 *  - Damage
 *  - Hitstun
 *  - Hitboxes
 *  - Duration
 *  - Armor (per frame)
 *  - Power level (determines clashes or hit through armor)
 *  - Knockback
 *  
 *  Most of these are properties of a hitbox
 */

public struct Circle
{
    public int cx;
    public int cy;
    public FInt x;
    public FInt y;
    public FInt r;
};

public class Hurtbox
{
    public Circle pos;
    public int armor = 0;
    public Combatent player = null;
};

public class Hitbox
{
    public Circle pos;
    public int damage = 0;
    public int knockback = 0;
    public int hitstun = 0;
    public Combatent sourcePlayer = null;
    public List<Combatent> damagedPlayers = new List<Combatent>();
    public HashSet<Tuple> damagedBlocks = new HashSet<Tuple>();
    public Move sourceMove = null;
};

public class Move
{
    public Combatent owner = null;
    public int duration = 0;
    public int currentFrame = 0;

    public virtual void Step(CombatManager mgr)
    {
        currentFrame += 1;
    }

    public virtual void Trigger(CombatManager mgr)
    {
        currentFrame = 0;
        mgr.moves.Add(this);
    }
}
