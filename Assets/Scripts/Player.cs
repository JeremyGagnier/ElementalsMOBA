using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : PhysicsMover {

	void Start ()
    {
        base.Start();
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

        bool jump = Input.GetButton("Jump");
        if (grounded && jump)
        {
            grounded = false;
            jumping = true;
            currentJumpDuration = new FInt(jumpDuration);
            vel.y = new FInt(jumpSpeed);
        }
        else if (jumping && jump)
        {
            currentJumpDuration -= PhysicsManager.timestep;
            if (currentJumpDuration.rawValue <= 0)
            {
                jump = false;
            }
            else
            {
                vel.y = new FInt(jumpSpeed);
            }
        }
        
        if (jumping && !jump)
        {
            jumping = false;
            vel.y = new FInt(jumpEndSpeed);
        }


        return vel;
    }

    public override void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
        if (xIsMin)
        {
            velocity.x = FInt.Zero();
        }
        else
        {
            velocity.y = FInt.Zero();
            grounded = true;
        }
        carried = false;
    }
}
