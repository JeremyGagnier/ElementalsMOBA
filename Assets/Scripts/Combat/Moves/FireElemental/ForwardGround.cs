using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireForwardGround : Move {
    private Hitbox hitbox1;
    private bool facingRight = true;

    public FireForwardGround(Combatent combatent)
    {
        owner = combatent;
        duration = 15;
        hitbox1 = new Hitbox();
        hitbox1.sourcePlayer = owner;
        hitbox1.sourceMove = this;
        hitbox1.pos = new Circle();
        hitbox1.damage = 10;
        hitbox1.hitstun = 25;
        hitbox1.knockback = 10;
        hitbox1.pos.y = new FInt(0.3);
        hitbox1.pos.r = new FInt(0.5);
    }

    public override void Step(CombatManager mgr)
    {
        if (currentFrame == 0)
        {
            owner.blockingMove = true;
        }
        else if (currentFrame == 5)
        {
            mgr.hitboxes.Add(hitbox1);
            hitbox1.pos.x = new FInt(facingRight ? 1 : -1);
        }
        else if (currentFrame == 10)
        {
            mgr.hitboxes.Remove(hitbox1);
        }
        else if (currentFrame == 14)
        {
            owner.blockingMove = false;
        }
        base.Step(mgr);
    }

    public override void Trigger(CombatManager mgr)
    {
        if (!((owner.input.rightAttackJustPressed && owner.facingRight) ||
              (owner.input.leftAttackJustPressed && !owner.facingRight)) ||
            owner.blockingMove ||
            owner.state != PhysicsMover.State.GROUNDED)
        {
            return;
        }
        facingRight = owner.facingRight;
        hitbox1.damagedPlayers.Clear();
        hitbox1.damagedBlocks.Clear();
        base.Trigger(mgr);
        //Debug.Log("Forward Ground");
    }
}
