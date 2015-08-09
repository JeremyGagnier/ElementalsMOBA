using UnityEngine;
using System;
using System.Collections;

public struct Sphere
{
    int cx;
    int cy;
    FInt x;
    FInt y;
    FInt r;
};


public class Move
{
    public delegate bool trigger();
    public MoveState[] move = null;


    public Move()
    {

    }
}

public class MoveState
{

    int startFrame;
    int frameLength;

    public MoveState(int start, int duration)
    {
        startFrame = start;
        frameLength = duration;
    }

    /*
    public Sphere[] GetHitSpheres()
    {
        return null;
    }*/


}
