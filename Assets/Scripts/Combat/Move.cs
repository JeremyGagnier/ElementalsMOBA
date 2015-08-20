using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct Sphere
{
    public int cx;
    public int cy;
    public FInt x;
    public FInt y;
    public FInt r;
};

[Serializable]
public class Move
{
    public delegate bool trigger();
    public List<MoveState> moveStates = null;
    public int currentFrame;
    public int player;

    public void Update()
    {
    }
}

[Serializable]
public class MoveState
{
    public int startFrame;
    public int frameLength;
}
