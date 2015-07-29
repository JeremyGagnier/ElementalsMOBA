using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct PhysBox
{
    public FInt x;
    public FInt y;
    public FInt w;
    public FInt h;
};

public class PhysicsMover : MonoBehaviour {

    [SerializeField]
    public List<PhysBox> hitbox;

    // Measured with block of dirt = 1
    public float mass_e;
    public FInt mass;

    public float runSpeed_e;
    public FInt runSpeed;

    /*
     * Air movement variables
     */
    public float verticalAirSpeed_e;
    public FInt verticalAirSpeed;

    public float horizontalAirSpeed_e;
    public FInt horizontalAirSpeed;

    public float fallSpeed_e;
    public FInt fallSpeed;

    public float fallAccel_e;
    public FInt fallAccel;

    /*
     * Jump variables
     */
    public float jumpSpeed_e;
    public FInt jumpSpeed;

    public float jumpDuration_e;
    public FInt jumpDuration;

    public float jumpEndSpeed_e;
    public FInt jumpEndSpeed;

    public FInt currentJumpDuration = FInt.Zero();

    /*
     * Important state variables
     */
    public bool allowInput = true;
    public bool carried = false;
    public bool grounded = false;
    public bool jumping = false;
    public bool facingRight = true;

    //public Tuple position_e = new Tuple(0, 0);
    public FVector position = new FVector(FInt.Zero(), FInt.Zero());
    public FVector velocity = new FVector(FInt.Zero(), FInt.Zero());

    public PhysicsManager pManager;

    void OnDrawGizmos()
    {
        foreach (PhysBox pos in hitbox)
        {
            int xMin = (new FInt(transform.position.x) + pos.x).ToInt() - 1;
            int xMax = (new FInt(transform.position.x) + pos.x + pos.w - FInt.RawFInt(256)).ToInt() + 1;
            int yMin = (new FInt(transform.position.y) + pos.y).ToInt() - 1;
            int yMax = (new FInt(transform.position.y) + pos.y + pos.h - FInt.RawFInt(256)).ToInt() + 1;

            Gizmos.color = new Color(0, 0, 1, 0.35f);
            for (int x = xMin + 1; x < xMax; ++x)
            {
                for (int y = yMin + 1; y < yMax; ++y)
                {
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), new Vector3(1, 1, 1));
                }
            }

            Gizmos.color = new Color(1, 0, 1, 0.5f);
            for (int y = yMin + 1; y < yMax; ++y)
            {
                Gizmos.DrawCube(new Vector3(xMin + 0.5f, y + 0.5f, 0), new Vector3(1, 1, 1));
                Gizmos.DrawCube(new Vector3(xMax + 0.5f, y + 0.5f, 0), new Vector3(1, 1, 1));
            }
            for (int x = xMin + 1; x < xMax; ++x)
            {
                Gizmos.DrawCube(new Vector3(x + 0.5f, yMin + 0.5f, 0), new Vector3(1, 1, 1));
                Gizmos.DrawCube(new Vector3(x + 0.5f, yMax + 0.5f, 0), new Vector3(1, 1, 1));
            }
        }
    }

    public void Start()
    {
        mass = new FInt(mass_e);
        verticalAirSpeed = new FInt(verticalAirSpeed_e);
        horizontalAirSpeed = new FInt(horizontalAirSpeed_e);
        fallSpeed = new FInt(fallSpeed_e);
        fallAccel = new FInt(fallAccel_e);
        runSpeed = new FInt(runSpeed_e);
        jumpSpeed = new FInt(jumpSpeed_e);
        jumpDuration = new FInt(jumpDuration_e);
        jumpEndSpeed = new FInt(jumpEndSpeed_e);

    }

    public virtual FVector ApplyInput(FVector vel)
    {
        return vel;
    }

    public virtual void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
    }
}
