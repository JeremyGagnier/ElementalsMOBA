using UnityEngine;
using System;
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

    /*
     * State Properties:
     *  - GROUNDED: In control, uses walking/running for movement
     *  - AIRBORNE: In control, uses air movement
     *  - LAUNCHED: Only DI control, keeps velocity with a fixed deceleration
     *  - STUNNED: No control, cannot move whatsoever
     *  - LAGGED: No control, can be force moved
     * 
     * Common Transitions:
     *  - GROUNDED -> AIRBORNE: From Jumping or falling
     *  - AIRBORNE -> GROUNDED: Landing on the ground
     *  - any state -> LAGGED: Getting hit by a move
     *  - LAGGED -> LAUNCHED: After the move resolves
     *  - LAUNCHED -> AIRBORNE: After hitstun wears off
     *  - LAUNCHED -> GROUNDED: After performing a breakfall
     */
    public enum State {
        GROUNDED,
        AIRBORNE,
        LAUNCHED,
        STUNNED,
        LAGGED
    }

    [SerializeField]
    public List<PhysBox> hitbox;

    // Measured with block of dirt = 1
    public FInt mass;
    public FInt runSpeed;

    /*
     * Air movement variables
     */
    public FInt upAirAccel;
    public FInt downAirAccel;
    public FInt horizontalAirAccel;
    public FInt horizontalAirSpeed;
    public FInt fallAccel;
    public FInt fallSpeed;

    /*
     * Other movement
     */
    public int turnaroundDuration;
    [HideInInspector] public int turning = 0;

    /*
     * Important state variables
     */
    public Action onGrounded;
    public Action onAirborne;
    public Action onLaunched;
    public Action onStunned;
    public Action onLagged;
    private State _state = State.AIRBORNE;
    public State state 
    {
        get
        {
            return _state;
        }
        set
        {
            if (state != value)
            {
                _state = value;
                switch (value)
                {
                    case State.GROUNDED:
                        if (onGrounded != null)
                        {
                            onGrounded();
                        }
                        break;
                    case State.AIRBORNE:
                        if (onAirborne != null)
                        {
                            onAirborne();
                        }
                        break;
                    case State.LAUNCHED:
                        if (onLaunched != null)
                        {
                            onLaunched();
                        }
                        break;
                    case State.STUNNED:
                        if (onStunned != null)
                        {
                            onStunned();
                        }
                        break;
                    case State.LAGGED:
                        if (onLagged != null)
                        {
                            onLagged();
                        }
                        break;
                }
            }
        }
    }

    [HideInInspector] public bool facingRight = true;

    public FVector position = new FVector(FInt.Zero(), FInt.Zero());
    [HideInInspector] public FVector velocity = new FVector(FInt.Zero(), FInt.Zero());

    public FInt feetPos;
    [HideInInspector] public Tuple lastChunkPos;
    [HideInInspector] public Tuple lastBlockPos;

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

    [HideInInspector] public PhysicsManager pManager = null;
    [HideInInspector] public InputManager input = null;

    void OnDrawGizmos()
    {
        if (!PhysicsManager.DEBUG)
        {
            return;
        }

        foreach (PhysBox pos in hitbox)
        {
            int xMin = (new FInt(transform.position.x) + pos.x).ToInt() - 1;
            int xMax = (new FInt(transform.position.x) + pos.x + pos.w).ToInt() + 1;
            int yMin = (new FInt(transform.position.y) + pos.y).ToInt() - 1;
            int yMax = (new FInt(transform.position.y) + pos.y + pos.h).ToInt() + 1;

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

    private FVector ApplyRunning(FVector vel)
    {
        if (turning == 0)
        {
            if (input.forwardPressed)
            {
                if (facingRight)
                {
                    vel.x = runSpeed;
                }
                else
                {
                    facingRight = true;
                    // This is pretty hacky
                    this.transform.localScale = new Vector3(2f, 2f, 1f);
                    turning = turnaroundDuration;
                    vel.x = FInt.Zero();
                }
            }
            else if (input.backwardPressed)
            {
                if (facingRight)
                {
                    facingRight = false;
                    // This is pretty hacky
                    this.transform.localScale = new Vector3(-2f, 2f, 1f);
                    turning = turnaroundDuration;
                    vel.x = FInt.Zero();
                }
                else
                {
                    vel.x = -runSpeed;
                }
            }
            else
            {
                vel.x = FInt.Zero();
            }
        }
        else
        {
            vel.x = FInt.Zero();
            turning -= 1;
        }
        return vel;
    }

    public FVector UpdateVelocity(FVector vel)
    {
        // Determine velocity changes based on input (DI and walking/running)

        switch (state)
        {
            case State.GROUNDED:
                vel.y = FInt.Zero();
                vel = ApplyRunning(vel);
                break;

            case State.AIRBORNE:
                if (turning != 0)
                {
                    turning = 0;
                    facingRight = !facingRight;
                    this.transform.localScale = new Vector3(facingRight ? 2f : -2f, 2f, 1f);
                }

                if (vel.x < -horizontalAirSpeed)
                {
                    if (input.forwardPressed)
                    {
                        vel.x += horizontalAirAccel * PhysicsManager.timestep * new FInt(2);
                    }
                    else if (input.backwardPressed)
                    {
                        vel.x += horizontalAirAccel * PhysicsManager.timestep * new FInt(0.33);
                    }
                    else
                    {
                        vel.x += horizontalAirAccel * PhysicsManager.timestep;
                    }
                }
                else if (vel.x <= horizontalAirSpeed)
                {
                    if (input.forwardPressed)
                    {
                        vel.x += horizontalAirAccel * PhysicsManager.timestep;
                        if (vel.x > horizontalAirSpeed)
                        {
                            vel.x = horizontalAirSpeed;
                        }
                    }
                    else if (input.backwardPressed)
                    {
                        vel.x -= horizontalAirAccel * PhysicsManager.timestep;
                        if (vel.x < -horizontalAirSpeed)
                        {
                            vel.x = -horizontalAirSpeed;
                        }
                    }
                }
                else
                {
                    if (input.backwardPressed)
                    {
                        vel.x -= horizontalAirAccel * PhysicsManager.timestep * new FInt(2);
                    }
                    else if (input.forwardPressed)
                    {
                        vel.x -= horizontalAirAccel * PhysicsManager.timestep * new FInt(0.33);
                    }
                    else
                    {
                        vel.x -= horizontalAirAccel * PhysicsManager.timestep;
                    }
                }
                /*
                if (input.upPressed)
                {
                    vel.y += upAirAccel * PhysicsManager.timestep;
                }
                else if (input.downPressed)
                {
                    vel.y -= downAirAccel * PhysicsManager.timestep;
                }*/
                vel.y -= fallAccel * PhysicsManager.timestep;
                if (vel.y < -fallSpeed)
                {
                    vel.y = -fallSpeed;
                }
                break;

            case State.LAUNCHED:
                break;
        }

        // Check to change light source
        if (lastChunkPos.x != chunkPos.x || lastChunkPos.y != chunkPos.y ||
            lastBlockPos.x != blockPos.x || lastBlockPos.y != blockPos.y)
        {
            HashSet<Tuple> lastLights = pManager.world.LightSourcesAt(lastChunkPos.x, lastChunkPos.y);
            if (lastLights != null)
            {
                lastLights.Remove(lastBlockPos);
                pManager.world.RefreshBlock(lastChunkPos.x, lastChunkPos.y, lastBlockPos.x, lastBlockPos.y);
            }
            HashSet<Tuple> lights = pManager.world.LightSourcesAt(chunkPos.x, chunkPos.y);
            if (lights != null)
            {
                lights.Add(blockPos);
                pManager.world.RefreshBlock(chunkPos.x, chunkPos.y, blockPos.x, blockPos.y);
            }
            lastChunkPos = chunkPos;
            lastBlockPos = blockPos;
        }

        return vel;
    }

    public void CollideWithBlocks(bool xIsMin, List<Tuple> blocks)
    {
        if (xIsMin)
        {
            // Check if we should move the player up one block (climbing)
            bool climb = false;
            if ((input.forwardPressed && velocity.x > FInt.Zero()) ||
                (input.backwardPressed && velocity.x < FInt.Zero()))
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

                        position.y -= FInt.RawFInt((position.y - FInt.RawFInt(feetPos.FractionalBits())).FractionalBits());
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
            if (state == PhysicsMover.State.AIRBORNE)
            {
                state = PhysicsMover.State.GROUNDED;
            }
            velocity.y = FInt.Zero();
        }
    }

    public void EnterNewBlock(bool xIsMin)
    {
        if (state == PhysicsMover.State.GROUNDED && !IsGrounded())
        {
            state = PhysicsMover.State.AIRBORNE;
        }
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

    private bool IsGrounded()
    {
        foreach (PhysBox pos in hitbox)
        {
            int yPos = (position.y + pos.y).ToInt() - 1;
            int xMin = (position.x + pos.x).ToInt() - 1;
            int xMax = (position.x + pos.x + pos.w).ToInt() + 1;
            for (int x = xMin + 1; x < xMax; ++x)
            {
                if (pManager.world.BlockAt(x, yPos) != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }
}
