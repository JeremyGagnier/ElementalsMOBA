using UnityEngine;
using System.Collections;

public class FireElemental : Combatent
{
    public Move jump = null;
    public Move fground = null;
    public Move bground = null;
    public Move dground = null;
    public Move uground = null;
    public Move fair = null;
    public Move bair = null;
    public Move dair = null;
    public Move uair = null;
    public Move f1 = null;
    public Move b1 = null;
    public Move d1 = null;
    public Move u1 = null;
    public Move f2 = null;
    public Move b2 = null;
    public Move d2 = null;
    public Move u2 = null;
    public Move f3 = null;
    public Move b3 = null;
    public Move d3 = null;
    public Move u3 = null;
    public Move f4 = null;
    public Move b4 = null;
    public Move d4 = null;
    public Move u4 = null;

    void Start()
    {
        // Add all the exposed moves to the moves list for
        // easier management.
        moves.Add(fair);
        moves.Add(bair);
        moves.Add(dair);
        moves.Add(uair);
        moves.Add(fground);
        moves.Add(bground);
        moves.Add(dground);
        moves.Add(uground);
        moves.Add(f1);
        moves.Add(b1);
        moves.Add(d1);
        moves.Add(u1);
        moves.Add(f2);
        moves.Add(b2);
        moves.Add(d2);
        moves.Add(u2);
        moves.Add(f3);
        moves.Add(b3);
        moves.Add(d3);
        moves.Add(u3);
        moves.Add(f4);
        moves.Add(b4);
        moves.Add(d4);
        moves.Add(u4);
    }
}
