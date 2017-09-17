using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public World world = null;

    public List<Combatent> combatents = new List<Combatent>();
    public List<Move> moves = new List<Move>();
    public List<Hurtbox> hurtboxes = new List<Hurtbox>();
    public List<Hitbox> hitboxes = new List<Hitbox>();

    void OnDrawGizmos()
    {
        foreach (Hitbox hitbox in hitboxes)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            if (hitbox.pos.x != null &&
                hitbox.pos.y != null &&
                hitbox.pos.r != null)
            {
                Gizmos.DrawSphere(new Vector3((hitbox.pos.x + hitbox.sourcePlayer.position.x).ToFloat(),
                                              (hitbox.pos.y + hitbox.sourcePlayer.position.y).ToFloat()),
                                              hitbox.pos.r.ToFloat());
            }
        }

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(new Vector3((hurtbox.pos.x + hurtbox.player.position.x).ToFloat(),
                                          (hurtbox.pos.y + hurtbox.player.position.y).ToFloat()),
                                          hurtbox.pos.r.ToFloat());
        }
    }

    public void Advance(int frames)
    {
        for (int f = Game.frame; f < Game.frame + frames; ++f)
        {
            foreach (Combatent combatent in combatents)
            {
                combatent.TriggerMoves();
            }

            // Update all moves
            for (int i = 0; i < moves.Count; ++i)
            {
                moves[i].Step(this);
                // This must be done here to prevent skipping the next active move.
                if (moves[i].currentFrame == moves[i].duration)
                {
                    moves.RemoveAt(i);
                    i -= 1;
                }
            }

            // Check collision with blocks
            foreach (Hitbox hitbox in hitboxes)
            {
                FInt hit_x = hitbox.pos.x + hitbox.sourcePlayer.position.x;
                FInt hit_y = hitbox.pos.y + hitbox.sourcePlayer.position.y;

                foreach (Hurtbox hurtbox in hurtboxes)
                {
                    FInt hurt_x = hurtbox.pos.x + hurtbox.player.position.x;
                    FInt hurt_y = hurtbox.pos.y + hurtbox.player.position.y;
                    if (Collisions.DistSqr(hit_x, hit_y, hurt_x, hurt_y) < (hitbox.pos.r + hurtbox.pos.r) * (hitbox.pos.r + hurtbox.pos.r))
                    {
                        CalculateHit(hitbox, hurtbox);
                        // player was hit!
                    }
                }
            }

            // Check collisions with hurtboxes
            foreach (Hitbox hitbox in hitboxes)
            {
                FInt real_x = hitbox.pos.x + hitbox.sourcePlayer.position.x;
                FInt real_y = hitbox.pos.y + hitbox.sourcePlayer.position.y;

                int xMin = (real_x - hitbox.pos.r).ToInt();
                int xMax = (real_x + hitbox.pos.r).ToInt() + 1;
                int yMin = (real_y - hitbox.pos.r).ToInt();
                int yMax = (real_y + hitbox.pos.r).ToInt() + 1;
                for (int x = xMin; x <= xMax; ++x)
                {
                    for (int y = yMin; y <= yMax; ++y)
                    {
                        // Do a check to make sure that this block is actually colliding
                        if (!Collisions.CircleToBox(real_x, real_y, hitbox.pos.r,
                                                    new FInt(x), new FInt(y), FInt.One(), FInt.One()))
                        {
                            continue;
                        }

                        Tuple pos = new Tuple(x, y);
                        if (world.BlockAt(x, y) != 0 &&
                            !hitbox.damagedBlocks.Contains(pos))
                        {
                            world.DamageBlock(x, y, hitbox.damage);
                            hitbox.damagedBlocks.Add(pos);
                        }
                    }
                }
            }
        }
    }

    internal void AddCombatent(Combatent combatent)
    {
        combatents.Add(combatent);
    }

    private void CalculateHit(Hitbox hitbox, Hurtbox hurtbox)
    {
        if (hurtbox.armor == 1)
        {
            return;
        }
        int direction = (hitbox.pos.x < hurtbox.pos.x) ? 1 : -1;

        hurtbox.player.health -= hitbox.damage;
        if (hurtbox.player.health <= 0)
        {
            //world.HandleDeath(hurtbox.player);
        }


    }
}
