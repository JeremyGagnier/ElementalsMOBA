using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysPlayer : PhysicsMover {

    public FInt feetPos;
    public Tuple lastChunkPos;
    public Tuple lastBlockPos;

    public override FVector ApplyInput(FVector vel)
    {
        // Determine velocity changes based on input (DI and walking/running)
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
        
        // Handle jumping (change this to a move instead)
        bool jump = InputManager.jumpPressed;
        if (grounded && InputManager.jumpJustPressed)
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

        // Check to change light source
        if (lastChunkPos.x != chunkPos.x || lastChunkPos.y != chunkPos.y ||
            lastBlockPos.x != blockPos.x || lastBlockPos.y != blockPos.y)
        {
            HashSet<Tuple> lastLights = pManager.world.LightSourceAt(lastChunkPos.x, lastChunkPos.y);
            if (lastLights != null)
            {
                lastLights.Remove(lastBlockPos);
                pManager.world.HackRelight(lastChunkPos.x, lastChunkPos.y);
            }
            HashSet<Tuple> lights = pManager.world.LightSourceAt(chunkPos.x, chunkPos.y);
            if (lights != null)
            {
                lights.Add(blockPos);
                pManager.world.HackRelight(chunkPos.x, chunkPos.y);
            }
            lastChunkPos = chunkPos;
            lastBlockPos = blockPos;
        }

        return vel;
    }

    public override void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
        if (xIsMin)
        {
            // Check if we should move the player up one block (climbing)
            bool climb = false;
            if ((InputManager.forwardPressed && velocity.x > FInt.Zero()) ||
                (InputManager.backwardPressed && velocity.x < FInt.Zero()))
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
                    FInt smallestBox = hitbox[0].y;
                    foreach (PhysBox box in hitbox)
                    {
                        if (box.y < smallestBox)
                        {
                            smallestBox = box.y;
                        }
                    }
                    FInt boxY = smallestBox + position.y;
                    FInt blockY = new FInt(highestBlock);

                    FInt vTemp = velocity.y;    // Force the velocity positive so that check mover collision works
                    velocity.y = new FInt(1);
                    if (blockY < boxY + FInt.One() && pManager.CheckMoverCollision(this, false).Count == 0)
                    {
                        position.y += FInt.One();
                        position.y -= FInt.RawFInt((position.y + FInt.RawFInt(feetPos.FractionalBits())).FractionalBits());
                        climb = true;
                    }
                    velocity.y = vTemp;
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
