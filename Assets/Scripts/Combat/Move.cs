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

public struct Sphere
{
    public int cx;
    public int cy;
    public FInt x;
    public FInt y;
    public FInt r;
};

public struct Hurtbox
{
    public Sphere pos;
    public int armor;
    public int player;
};

public struct Hitbox
{
    public Sphere pos;
    public int damage;
    public int knockback;
    public int hitstun;
    public int sourcePlayer;
    public List<int> damagedPlayers;
    public Move sourceMove;
};

public class Move
{
    public int duration;
    public int currentFrame = 0;
    public int player;

    public virtual void Step(CombatManager mgr)
    {
        currentFrame += 1;
        if (currentFrame == duration)
        {
            mgr.moves.Remove(this);
            currentFrame = 0;
        }
    }

    public virtual bool Trigger()
    {
        return false;
    }
}
