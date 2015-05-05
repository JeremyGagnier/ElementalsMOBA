﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsMover : MonoBehaviour {

    [SerializeField]
    public List<Vector4> hitbox;

    // Measured with block of dirt = 1
    public float mass_e;
    public FInt mass;

    // Measured in blocks per second
    public float verticalAirSpeed_e;
    public FInt verticalAirSpeed;

    // Measured in blocks per second
    public float horizontalAirSpeed_e;
    public FInt horizontalAirSpeed;

    // Measure in blocks per second
    public float fallSpeed_e;
    public FInt fallSpeed;

    // Measured in blocks per second squared
    public float fallAccel_e;
    public FInt fallAccel;

    public bool allowInput = true;
    public bool carried = false;
    public bool grounded = false;

    //public Tuple position_e = new Tuple(0, 0);
    public FVector position = new FVector(FInt.Zero(), FInt.Zero());
    public FVector velocity = new FVector(FInt.Zero(), FInt.Zero());

    void OnDrawGizmos()
    {
        foreach (Vector4 pos in hitbox)
        {
            int xMin = (new FInt(transform.position.x) + pos.x).ToInt() - 1;
            int xMax = (new FInt(transform.position.x) + pos.x + pos.z - FInt.RawFInt(256)).ToInt() + 1;
            int yMin = (new FInt(transform.position.y) + pos.y).ToInt() - 1;
            int yMax = (new FInt(transform.position.y) + pos.y + pos.w - FInt.RawFInt(256)).ToInt() + 1;

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
        }
    }

    void Start()
    {
        mass = new FInt(mass_e);
        verticalAirSpeed = new FInt(verticalAirSpeed_e);
        horizontalAirSpeed = new FInt(horizontalAirSpeed_e);
        fallSpeed = new FInt(fallSpeed_e);
        fallAccel = new FInt(fallAccel_e);
        //position = new FVector(new FInt(position_e.x), new FInt(position_e.y));
    }

    public virtual FVector ApplyInput(FVector vel)
    {
        return vel;
    }

    public virtual void CollideWithBlocks(List<Tuple> blocks)
    {
    }
}