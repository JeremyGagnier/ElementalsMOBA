using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jump : Move
{
    private enum Type
    {
        GROUNDED,
        AERIAL
    }

    private Type jumpType;
    private bool stillHolding = false;

    public Jump(Combatent combatent)
    {
        owner = combatent;
        duration = 16;
    }

    public override void Step(CombatManager mgr)
    {
        if (jumpType == Type.GROUNDED)
        {
            if (currentFrame == 4)
            {
                owner.state = PhysicsMover.State.AIRBORNE;
                owner.velocity.y = owner.groundedJumpSpeed;
            }
            else if (currentFrame >= 5 && currentFrame < 6)
            {
                owner.velocity.y = owner.groundedJumpSpeed;
            }
            else if (currentFrame == 6)
            {
                if (owner.input.jumpPressed)
                {
                    stillHolding = true;
                    owner.velocity.y = owner.groundedJumpSpeed;
                }
            }
            else if (currentFrame >= 7 && currentFrame < 16)
            {
                if (stillHolding)
                {
                    owner.velocity.y = owner.groundedJumpSpeed;
                }
            }
        }
        else if (jumpType == Type.AERIAL)
        {
            if (currentFrame == 2)
            {
                owner.velocity.y = owner.aerialJumpSpeed;
            }
        }
        base.Step(mgr);
    }

    public override void Trigger(CombatManager mgr)
    {
        if (!owner.input.jumpJustPressed ||
            owner.blockingMove)
        {
            return;
        }

        if (owner.state == PhysicsMover.State.GROUNDED)
        {
            jumpType = Type.GROUNDED;
        }
        else if (owner.state == PhysicsMover.State.AIRBORNE)
        {
            if (owner.jumps == 0)
            {
                return;
            }
            else
            {
                owner.jumps -= 1;
                jumpType = Type.AERIAL;
            }
        }
        else
        {
            return;
        }
        stillHolding = false;
        base.Trigger(mgr);
        //Debug.Log("Jump");
    }
}
