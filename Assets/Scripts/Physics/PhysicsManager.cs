using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour {

    public static FInt timestep = new FInt(0.01666f);
    public static bool DEBUG = true;

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
                foreach (PhysicsMover mover in movers)
                {
                    
                    HashSet<Tuple> sources = world.LightSourcesAt(mover.position.x.ToInt() / world.numBlocksWide,
                                                                 mover.position.y.ToInt() / world.numBlocksHigh);
                    if (sources != null)
                    {
                        sources.Add(new Tuple(mover.position.x.ToInt() % world.numBlocksWide,
                                              mover.position.y.ToInt() % world.numBlocksHigh));
                    }
                    MoveMover(mover);
                }
            }
        }
    }

    private void MoveMover(PhysicsMover mover)
    {
        mover.velocity = mover.UpdateVelocity(mover.velocity);
        CollideWithWorld(mover);

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
                    FInt minOffX = FInt.RawFInt((mover.position.x + pos.x).FractionalBits());
                    xTime = minOffX / -mover.velocity.x;
                }
                else if (mover.velocity.x > FInt.Zero())
                {
                    FInt maxOffX = FInt.RawFInt((mover.position.x + pos.x + pos.w).FractionalBits());
                    xTime = (new FInt(1) - maxOffX) / mover.velocity.x;
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
                    }
                }

                if (mover.velocity.y < FInt.Zero())
                {
                    FInt minOffY = FInt.RawFInt((mover.position.y + pos.y).FractionalBits());
                    yTime = minOffY / -mover.velocity.y;
                }
                else if (mover.velocity.y > FInt.Zero())
                {
                    FInt maxOffY = FInt.RawFInt((mover.position.y + pos.y + pos.h).FractionalBits());
                    yTime = (new FInt(1) - maxOffY) / mover.velocity.y;
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
