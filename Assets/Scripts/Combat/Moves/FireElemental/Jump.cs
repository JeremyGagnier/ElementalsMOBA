using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Jump : Move {
    private enum Type
    {
        GROUNDED,
        DOUBLE
    }

    public Jump(Combatent combatent)
    {
        owner = combatent;
        duration = 10;
    }

    public override void Step(CombatManager mgr)
    {
        switch (currentFrame)
        {
            case 0:
                owner.velocity.y = owner.jumpSpeed;
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                break;
            case 7:
                break;
            case 8:
                break;
            case 9:
                break;
            case 10:
                break;
            case 11:
                break;
            case 12:
                break;
            case 13:
                break;
            case 14:
                break;
        }
        base.Step(mgr);
    }

    public override void Trigger(CombatManager mgr)
    {
        if (!InputManager.jumpJustPressed ||
            owner.blockingMove)
        {
            return;
        }

        if (owner.state == PhysicsMover.State.GROUNDED)
        {
            owner.state = PhysicsMover.State.AIRBORNE;
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
            }
        }
        else
        {
            return;
        }

        base.Trigger(mgr);
        //Debug.Log("Jump");
    }
}
