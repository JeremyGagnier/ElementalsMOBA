using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public List<Combatent> combatents = new List<Combatent>();
    // All moves in the moves list are currently being executed.
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
                Gizmos.DrawSphere(new Vector3((hitbox.pos.x + hitbox.sourcePlayer.Position.x).ToFloat(),
                                              (hitbox.pos.y + hitbox.sourcePlayer.Position.y).ToFloat()),
                                              hitbox.pos.r.ToFloat());
            }
        }

        foreach (Hurtbox hurtbox in hurtboxes)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
            Gizmos.DrawSphere(new Vector3((hurtbox.pos.x + hurtbox.player.Position.x).ToFloat(),
                                          (hurtbox.pos.y + hurtbox.player.Position.y).ToFloat()),
                                          hurtbox.pos.r.ToFloat());
        }
    }

    public void Advance(int frames)
    {
        for (int f = Game.frame; f < Game.frame + frames; ++f)
        {
            for (int i = 0; i < combatents.Count; ++i)
            {
                combatents[i].TriggerMoves();
            }

            // Update all moves
            for (int i = 0; i < moves.Count; ++i)
            {
                moves[i].Step(this);
            }

            // Calculate all hits

        }
    }

    internal void AddCombatent(Combatent combatent)
    {
        combatents.Add(combatent);
    }
}
