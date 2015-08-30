using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public List<Combatent> combatents;
    // All moves in the moves list are currently being executed.
    public List<Move> moves;
    public List<Hurtbox> hurtboxes;
    public List<Hitbox> hitboxes;

	// Use this for initialization
	void Start ()
    {
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
}
