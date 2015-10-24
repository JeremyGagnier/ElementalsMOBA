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
    public FInt mass;

    public FInt runSpeed;

    /*
     * Air movement variables
     */
    public FInt verticalAirSpeed;

    public FInt horizontalAirSpeed;

    public FInt fallSpeed;

    public FInt fallAccel;

    /*
     * Jump variables
     */
    public FInt jumpSpeed;

    public FInt jumpDuration;

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

    public Tuple chunkPos
    {
        get
        {
            return new Tuple(position.x.ToInt() / pManager.world.numBlocksWide,
                             position.y.ToInt() / pManager.world.numBlocksHigh);
        }
    }
    public Tuple blockPos
    {
        get
        {
            return new Tuple(position.x.ToInt() % pManager.world.numBlocksWide,
                             position.y.ToInt() % pManager.world.numBlocksHigh);
        }
    }

    public PhysicsManager pManager;

    void OnDrawGizmos()
    {
        if (!PhysicsManager.DEBUG)
        {
            return;
        }

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

            Gizmos.color = new Color(1, 1, 0, 0.4f);
            Gizmos.DrawCube(new Vector3(transform.position.x,
                                        transform.position.y, 0),
                            new Vector3(pos.w.ToFloat(), pos.h.ToFloat(), 1));
        }
    }

    public virtual FVector ApplyInput(FVector vel)
    {
        return vel;
    }

    public virtual void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
    }

    public virtual void EnterNewBlock(bool xIsMin)
    {
    }

    public void ApplyForce(FVector force)
    {
        velocity += force / mass;
    }

    public void SetVelocity(FVector vel)
    {
        // Need to make a new FVector since ours will change and the one we
        // were provided probably needs to stay constant.
        velocity = new FVector(vel);
    }
}
