using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : PhysicsMover {

    public FInt feetPos;
    public float feetPos_e;

	new void Start ()
    {
        base.Start();
        feetPos = new FInt(feetPos_e);
        position = new FVector(new FInt(transform.position.x), new FInt(transform.position.y));
        transform.position = new Vector3(position.x.ToFloat(), position.y.ToFloat(), 0);
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
            bool climb = false;
            float xInput = Input.GetAxis("Horizontal");
            if (xInput * velocity.x.ToFloat() > 0)
            {
                if (blocks.Count != 0)
                {
                    int highestBlock = blocks[0].y;
                    foreach (Tuple pos in blocks)
                    {
                        if (pos.y > highestBlock)
                        {
                            highestBlock = pos.y;
                        }
                    }
                    float smallestBox = hitbox[0].y;
                    foreach (Vector4 box in hitbox)
                    {
                        if (box.y < smallestBox)
                        {
                            smallestBox = box.y;
                        }
                    }
                    FInt boxY = new FInt(smallestBox) + position.y;
                    FInt blockY = new FInt(highestBlock);
                    if (blockY < boxY + FInt.One())
                    {
                        position.y += FInt.One();
                        position.y -= FInt.RawFInt((position.y + FInt.RawFInt(feetPos.FractionalBits())).FractionalBits());
                        climb = true;
                    }
                }
            }
            if (!climb)
            {
                velocity.x = FInt.Zero();
            }
        }
        else
        {
            velocity.y = FInt.Zero();
            grounded = true;
        }
        carried = false;
    }
}
