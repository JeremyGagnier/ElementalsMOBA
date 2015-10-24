using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Tuple
{
    public int x;
    public int y;

    public Tuple(int first, int second)
    {
        x = first;
        y = second;
    }
}

public class Game : MonoBehaviour
{
    public GameObject physicsPrefab;
    public GameObject combatPrefab;
    public GameObject worldPrefab;
    public GameObject fireElemental;

    private PhysicsManager physics = null;
    private CombatManager combat = null;
    private World world = null;

    public static int frame = 0;

	void Start ()
    {
        // Create the fundamental classes and set them up
        physics = Instantiate(physicsPrefab).GetComponent<PhysicsManager>();
        combat = Instantiate(combatPrefab).GetComponent<CombatManager>();
        world = Instantiate(worldPrefab).GetComponent<World>();
        physics.world = world;

        CreatePlayer(fireElemental);
    }

    void CreatePlayer(GameObject elemental)
    {
        GameObject player = Instantiate(elemental);
        world.localPlayer = player;
        PhysPlayer mover = player.GetComponent<PhysPlayer>();
        physics.AddMover(mover);
        Combatent combatent = player.GetComponent<Combatent>();
        combat.AddCombatent(combatent);
        combatent.phys = (PhysicsMover)mover;
        combatent.manager = combat;

    }

	void FixedUpdate()
    {
        // An interesting point here is that while combat and physics are co-dependant they can be stepped independantly.
        // In the general sense physics handles motion and position while combat handles moves and damage.
        // However moves are dependant on position and position and motion are dependant on moves.
        // For example a move will put a hitboxes relative to the players position and will periodically
        // change the location of the character as well as change where the hurtboxes are from animations.
        InputManager.Advance(1);
        physics.Advance(1);
        combat.Advance(1);
        frame += 1;
	}
}
