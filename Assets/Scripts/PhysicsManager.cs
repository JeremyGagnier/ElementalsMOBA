using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsManager : MonoBehaviour {

    public World world;

    List<PhysicsMover> movers = new List<PhysicsMover>();

	// Use this for initialization
	void Start () {
        movers = new List<PhysicsMover>(FindObjectsOfType<PhysicsMover>());
	}
	
	// Update is called once per frame
	void LateUpdate () {
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
            foreach (PhysicsMover mover in movers)
            {
                UpdateVelocity(mover);
                UpdatePosition(mover);
            }
        }
    }

    private void UpdateVelocity(PhysicsMover mover)
    {
        Tuple velocity = mover.velocity;
        if (velocity.y > -mover.fallSpeed)
        {
            velocity.y -= mover.fallAccel;
        }
        if (velocity.y < -mover.fallSpeed && !mover.carried)
        {
            velocity.y = -mover.fallSpeed;
        }
        if (mover.allowInput)
        {
            velocity = mover.ApplyInput(velocity);
        }
        mover.velocity = velocity;
    }

    private void UpdatePosition(PhysicsMover mover)
    {
        foreach (Vector4 pos in mover.hitbox)
        {
            int minBlockx = (mover.position.x + (int)(pos.x * (1 << 16))) >> 16;
            int minBlocky = (mover.position.y + (int)(pos.y * (1 << 16))) >> 16;
            int fractionx = (mover.position.x + (int)(pos.x * (1 << 16))) % (1 << 16);
            int fractiony = (mover.position.y + (int)(pos.y * (1 << 16))) % (1 << 16);

            int width = Mathf.CeilToInt(pos.z) + 1;
            if (fractionx == 0)
            {
                width -= 1;
            }

            int height = Mathf.CeilToInt(pos.w) + 1;
            if (fractiony == 0)
            {
                height -= 1;
            }

            for (int x = minBlockx; x < minBlockx + width; ++x)
            {
                for (int y = minBlocky; y < minBlocky + height; ++y)
                {
                    // If you start a frame inside a block then you are stuck.
                    if (world.BlockAt(x, y) != 0)
                    {
                        mover.velocity.x = 0;
                        mover.velocity.y = 0;
                        return;
                    }
                }
            }
        }

        mover.position.x += mover.velocity.x;
        mover.position.y += mover.velocity.y;

        float px = (float)(mover.position.x >> 16) + (float)(mover.position.x % (1 << 16)) / (float)(1 << 16);
        float py = (float)(mover.position.y >> 16) + (float)(mover.position.y % (1 << 16)) / (float)(1 << 16);
        mover.transform.position = new Vector3(px, py, 0);
    }
}
