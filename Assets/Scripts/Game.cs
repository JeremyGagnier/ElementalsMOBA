using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
    public GameObject physicsPrefab;
    public GameObject worldPrefab;
    public GameObject fireElemental;

    private PhysicsManager physics = null;
    private World world = null;

	void Start ()
    {
        // Create the fundamental classes and set them up
        physics = Instantiate(physicsPrefab).GetComponent<PhysicsManager>();
        world = Instantiate(worldPrefab).GetComponent<World>();
        physics.world = world;

        CreatePlayer(fireElemental);
    }

    void CreatePlayer(GameObject elemental)
    {
        GameObject player = Instantiate(elemental);
        world.localPlayer = player;
        physics.AddMover(player.GetComponent<Player>());
    }

	void Update ()
    {
	
	}
}
