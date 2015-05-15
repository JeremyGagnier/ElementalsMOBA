using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : PhysicsMover {
	public const int MAX_FALL_SPEED = 30;

	public float moveSpeed = 10;

	void Start ()
    {
        mass = new FInt(mass_e);
        verticalAirSpeed = new FInt(verticalAirSpeed_e);
        horizontalAirSpeed = new FInt(horizontalAirSpeed_e);
        fallSpeed = new FInt(fallSpeed_e);
        fallAccel = new FInt(fallAccel_e);
        runSpeed = new FInt(runSpeed_e);
        position = new FVector(new FInt(transform.position.x), new FInt(transform.position.y));

        transform.position = new Vector3(position.x.ToFloat(), position.y.ToFloat(), 0);
	}

	void Update ()
	{

	}

    public override FVector ApplyInput(FVector vel)
    {
        float xInput = Input.GetAxis("Horizontal");
        if (grounded)
        {
            if (xInput > 0)
            {
                vel.x = runSpeed;
            }
            else if (xInput < 0)
            {
                vel.x = -runSpeed;
            }
            else
            {
                vel.x = FInt.Zero();
            }
        }
        else
        {
            if (carried)
            {
                if (xInput > 0)
                {
                    vel.x += horizontalAirSpeed * PhysicsManager.timestep;
                }
                else if (xInput < 0)
                {
                    vel.x -= horizontalAirSpeed * PhysicsManager.timestep;
                }
            }
            else
            {
                if (xInput > 0)
                {
                    vel.x = horizontalAirSpeed;
                }
                else if (xInput < 0)
                {
                    vel.x = -horizontalAirSpeed;
                }
                else
                {
                    vel.x = FInt.Zero();
                }
            }
        }
        return vel;
    }

    public override void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
        if (xIsMin)
        {
        }
        else
        {
            grounded = true;
        }
        carried = false;
        velocity = new FVector(FInt.Zero(), FInt.Zero());
    }
}
