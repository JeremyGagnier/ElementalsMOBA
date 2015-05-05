using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour {

    public static FInt timestep = new FInt(0.01666f);

    public World world;

    public static int frame = 0;

    List<PhysicsMover> movers = new List<PhysicsMover>();

	// Use this for initialization
	void Start () {
        movers = new List<PhysicsMover>(FindObjectsOfType<PhysicsMover>());
	}
	
	void FixedUpdate () {
        Advance(1);
	}

    public void AddMover(PhysicsMover mover)
    {
        movers.Add(mover);
    }

    private void Advance(int frames)
    {
        for (int i = 0; i < frames; ++i)
        {
            if (frame > 10)         // Don't do anything until the game scene is loaded
            {
                foreach (PhysicsMover mover in movers)
                {
                    MoveMover(mover);
                }
            }
            ++frame;
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
            foreach (Vector4 pos in mover.hitbox)
            {
                FInt xTime = FInt.Max();
                FInt yTime = FInt.Max();

                if (mover.velocity.x < FInt.Zero())
                {
                    int minX = (mover.position.x + pos.x).ToInt();
                    FInt minOffX = mover.position.x + pos.x - new FInt(minX);
                    xTime = minOffX / -mover.velocity.x;
                    if (xTime.rawValue == 0)
                    {
                        xTime = FInt.One() / -mover.velocity.x;
                    }
                }
                else if (mover.velocity.x > FInt.Zero())
                {
                    int maxX = (mover.position.x + pos.x + pos.z - FInt.RawFInt(1)).ToInt() + 1;
                    FInt maxOffX = new FInt(maxX) - (mover.position.x + pos.x + pos.z);
                    xTime = maxOffX / mover.velocity.x;
                    if (xTime.rawValue == 0)
                    {
                        xTime = FInt.One() / mover.velocity.x;
                    }
                }

                if (mover.velocity.y < FInt.Zero())
                {
                    int minY = (mover.position.y + pos.y).ToInt();
                    FInt minOffY = mover.position.y + pos.y - new FInt(minY);
                    yTime = minOffY / -mover.velocity.y;
                    if (yTime.rawValue == 0)
                    {
                        yTime = FInt.One() / -mover.velocity.y;
                    }
                }
                else if (mover.velocity.y > FInt.Zero())
                {
                    int maxY = (mover.position.y + pos.y + pos.w - FInt.RawFInt(1)).ToInt() + 1;
                    FInt maxOffY = new FInt(maxY) - (mover.position.y + pos.y + pos.w);
                    yTime = maxOffY / mover.velocity.y;
                    if (yTime.rawValue == 0)
                    {
                        yTime = FInt.One() / mover.velocity.y;
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

            mover.position += mover.velocity * minTimestep;
            step -= minTimestep;

            if (step.rawValue == 0)
            {
                break;
            }

            List<Tuple> blocksHit = new List<Tuple>();
            foreach (Vector4 pos in mover.hitbox)
            {
                int xMin = (mover.position.x + pos.x).ToInt() - 1;
                int xMax = (mover.position.x + pos.x + pos.z - FInt.RawFInt(1)).ToInt() + 1;
                int yMin = (mover.position.y + pos.y).ToInt() - 1;
                int yMax = (mover.position.y + pos.y + pos.w - FInt.RawFInt(1)).ToInt() + 1;

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
            if (blocksHit.Count != 0)
            {
                mover.CollideWithBlocks(blocksHit);
            }

            if (mover.velocity.x.rawValue == 0 &&
                mover.velocity.y.rawValue == 0)
            {
                break;
            }
        }
    }

    private bool IsGrounded(PhysicsMover mover)
    {
        foreach (Vector4 pos in mover.hitbox)
        {
            int yPos = (mover.position.y + pos.y).ToInt() - 1;
            int xMin = (mover.position.x + pos.x).ToInt() - 1;
            int xMax = (mover.position.x + pos.x + pos.z - FInt.RawFInt(1)).ToInt() + 1;
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

}
