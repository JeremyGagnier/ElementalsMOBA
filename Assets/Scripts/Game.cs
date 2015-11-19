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
    private GameObject physicsPrefab;
    private GameObject combatPrefab;
    private GameObject worldPrefab;
    private GameObject fireElemental;

    private PhysicsManager physics = null;
    private CombatManager combat = null;
    private World world = null;

    public static int frame = 0;

    void Awake()
    {
        physicsPrefab = Resources.Load("Prefabs/Physics") as GameObject;
        combatPrefab = Resources.Load("Prefabs/Combat") as GameObject;
        worldPrefab = Resources.Load("Prefabs/World") as GameObject;
        fireElemental = Resources.Load("Prefabs/Characters/FireElemental") as GameObject;
    }

	void Start ()
    {
        // Create the fundamental classes and set them up
        GameObject physicsObject = Instantiate(physicsPrefab);
        physicsObject.name = "Physics Manager";
        physics = physicsObject.GetComponent<PhysicsManager>();

        GameObject combatObject = Instantiate(combatPrefab);
        combatObject.name = "Combat Manager";
        combat = combatObject.GetComponent<CombatManager>();

        GameObject worldObject = Instantiate(worldPrefab);
        worldObject.name = "World";
        world = worldObject.GetComponent<World>();

        physics.world = world;

        CreatePlayer(fireElemental);
    }

    private void CreatePlayer(GameObject elemental)
    {
        GameObject player = Instantiate(elemental);
        world.localPlayer = player;

        Combatent combatent = player.GetComponent<Combatent>();
        combat.AddCombatent(combatent);
        combatent.manager = combat;

        PhysicsMover mover = combatent as PhysicsMover;
        physics.AddMover(mover);

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
