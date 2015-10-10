using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour {

    public static FInt timestep = new FInt(0.01666f);
    public static bool DEBUG = false;

    public World world;

    List<PhysicsMover> movers = new List<PhysicsMover>();

    public void AddMover(PhysicsMover mover)
    {
        movers.Add(mover);
        mover.pManager = this;
    }

    public void Advance(int frames)
    {
        for (int f = Game.frame; f < Game.frame + frames; ++f)
        {
            if (Game.frame > 10)         // Don't do anything until the game scene is loaded
            {
                /*
                foreach (PhysicsMover mover in movers)
                {
                    // This is obviously a hack, fix/remove this.
                    world.LightSourceAt(mover.position.x.ToInt() / world.chunkWidth,
                                        mover.position.y.ToInt() / world.chunkHeight).Clear();
                }*/
                foreach (PhysicsMover mover in movers)
                {
                    // This is obviously a hack, fix/remove this.
                    HashSet<Tuple> sources = world.LightSourceAt(mover.position.x.ToInt() / world.blockWidth,
                                                                 mover.position.y.ToInt() / world.blockHeight);
                    if (sources != null)
                    {
                        sources.Add(new Tuple(mover.position.x.ToInt() % world.blockWidth,
                                              mover.position.y.ToInt() % world.blockHeight));
                    }
                    //Debug.LogError((mover.position.x.ToInt() / world.blockWidth).ToString() + ", " +
                    //               (mover.position.y.ToInt() / world.blockHeight).ToString() + " : ");
                    MoveMover(mover);
                }
            }
        }
    }

    private void UpdateVelocity(PhysicsMover mover)
    {
        if (!mover.grounded)
        {
            if (mover.velocity.y > -mover.fallSpeed)
            {
                mover.velocity.y -= mover.fallAccel * timestep;
            }
            if (mover.velocity.y < -mover.fallSpeed && !mover.carried)
            {
                mover.velocity.y = -mover.fallSpeed;
            }
        }
        if (mover.allowInput)
        {
            mover.velocity = mover.ApplyInput(mover.velocity);
        }
    }

    private void MoveMover(PhysicsMover mover)
    {
        UpdateVelocity(mover);
        CollideWithWorld(mover);
        if (mover.grounded)
        {
            mover.grounded = IsGrounded(mover);
        }

        float px = mover.position.x.ToFloat();
        float py = mover.position.y.ToFloat();
        mover.transform.position = new Vector3(px, py, 0);
    }

    private void CollideWithWorld(PhysicsMover mover)
    {
        FInt step = new FInt(timestep);
        while (true)
        {
            bool xIsMin = true;
            FInt minTimestep = new FInt(step);
            foreach (PhysBox pos in mover.hitbox)
            {
                FInt xTime = FInt.Max();
                FInt yTime = FInt.Max();

                // Set the left and right x boundaries given the direction of motion.
                if (mover.velocity.x < FInt.Zero())
                {
                    int minX = (mover.position.x + pos.x).ToInt();
                    FInt minOffX = mover.position.x + pos.x - new FInt(minX);
                    xTime = minOffX / -mover.velocity.x;
                }
                else if (mover.velocity.x > FInt.Zero())
                {
                    int maxX = (mover.position.x + pos.x + pos.w).ToInt() + 1;
                    FInt maxOffX = new FInt(maxX) - (mover.position.x + pos.x + pos.w);
                    xTime = maxOffX / mover.velocity.x;
                }

                // If we're entering a new x unit without moving first do some stuff.
                if (xTime.rawValue == 0)
                {
                    // Find if we will be colliding with any blocks in the x direction. This should
                    // change our x velocity so that we won't collide again until the next frame.
                    List<Tuple> reCollide = CheckMoverCollision(mover, true);
                    if (reCollide.Count != 0)
                    {
                        Log("x re-collide");
                        mover.CollideWithBlocks(true, reCollide);
                        if (mover.velocity.x.rawValue == 0)
                        {
                            xTime = FInt.Max();
                        }
                    }
                    else
                    {
                        // If we're not going to collide with anything then update the position up to the
                        // next possible collision.
                        xTime = FInt.One() / mover.velocity.x.Abs();
                        Log("x force moved");
                    }
                }

                if (mover.velocity.y < FInt.Zero())
                {
                    int minY = (mover.position.y + pos.y).ToInt();
                    FInt minOffY = mover.position.y + pos.y - new FInt(minY);
                    yTime = minOffY / -mover.velocity.y;
                }
                else if (mover.velocity.y > FInt.Zero())
                {
                    int maxY = (mover.position.y + pos.y + pos.h).ToInt() + 1;
                    FInt maxOffY = new FInt(maxY) - (mover.position.y + pos.y + pos.h);
                    yTime = maxOffY / mover.velocity.y;
                }
                if (yTime.rawValue == 0)
                {
                    List<Tuple> reCollide = CheckMoverCollision(mover, false);
                    if (reCollide.Count != 0)
                    {
                        Log("y re-collide");
                        mover.CollideWithBlocks(false, reCollide);
                        if (mover.velocity.y.rawValue == 0)
                        {
                            yTime = FInt.Max();
                        }
                    }
                    else
                    {
                        yTime = FInt.One() / mover.velocity.y.Abs();
                        Log("y force moved");
                    }
                }

                if (xTime < minTimestep)
                {
                    minTimestep = xTime;
                    xIsMin = true;
                }

                if (yTime < minTimestep)
                {
                    minTimestep = yTime;
                    xIsMin = false;
                }
            }

            if (minTimestep.rawValue <= 0)
            {
                LogWarning("Stepped by " + minTimestep.rawValue.ToString() + ", exiting while loop!!!");
                break;
            }

            mover.position += mover.velocity * minTimestep;
            step -= minTimestep;

            if (step.rawValue == 0)
            {
                break;
            }

            List<Tuple> blocksHit = CheckMoverCollision(mover, xIsMin);
            if (blocksHit.Count != 0)
            {
                mover.CollideWithBlocks(xIsMin, blocksHit);
            }
            else
            {
                mover.EnterNewBlock(xIsMin);
            }

            if (mover.velocity.x.rawValue == 0 &&
                mover.velocity.y.rawValue == 0)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Check if there are blocks in the x or y direction that we're moving in.
    /// </summary>
    /// <param name="mover"></param>
    /// <param name="xIsMin"></param>
    /// <returns></returns>
    public List<Tuple> CheckMoverCollision(PhysicsMover mover, bool xIsMin)
    {
        List<Tuple> blocksHit = new List<Tuple>();
        foreach (PhysBox pos in mover.hitbox)
        {
            int xMin = (mover.position.x + pos.x).ToInt() - 1;
            int xMax = (mover.position.x + pos.x + pos.w).ToInt() + 1;
            int yMin = (mover.position.y + pos.y).ToInt() - 1;
            int yMax = (mover.position.y + pos.y + pos.h).ToInt() + 1;

            if (xIsMin)
            {
                int xPos = (mover.velocity.x.rawValue < 0) ? xMin : xMax;
                for (int y = yMin + 1; y < yMax; ++y)
                {
                    if (world.BlockAt(xPos, y) != 0)
                    {
                        blocksHit.Add(new Tuple(xPos, y));
                    }
                }
            }
            else
            {
                int yPos = (mover.velocity.y.rawValue < 0) ? yMin : yMax;
                for (int x = xMin + 1; x < xMax; ++x)
                {
                    if (world.BlockAt(x, yPos) != 0)
                    {
                        blocksHit.Add(new Tuple(x, yPos));
                    }
                }
            }
        }
        return blocksHit;
    }

    private bool IsGrounded(PhysicsMover mover)
    {
        foreach (PhysBox pos in mover.hitbox)
        {
            int yPos = (mover.position.y + pos.y).ToInt() - 1;
            int xMin = (mover.position.x + pos.x).ToInt() - 1;
            int xMax = (mover.position.x + pos.x + pos.w - FInt.RawFInt(1)).ToInt() + 1;
            for (int x = xMin + 1; x < xMax; ++x)
            {
                if (world.BlockAt(x, yPos) != 0)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void Log(string message)
    {
        if (DEBUG)
        {
            Debug.Log("PhysicsManager: " + message);
        }
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning("PhysicsManager: " + message);
    }

    public static void LogError(string message)
    {
        Debug.LogError("PhysicsManager: " + message);
    }
}
