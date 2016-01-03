using UnityEngine;
using System.Collections;

public class Collisions
{
    private static FInt DistSqr(FInt x1, FInt y1, FInt x2, FInt y2)
    {
        return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
    }

    public static bool CircleToBox(FInt cx, FInt cy, FInt rad, 
                                   FInt bx, FInt by, FInt bw, FInt bh) {
        // Two point to rect checks and four point to circle checks
        return (cx > bx && cx < (bx + bw) && (cy + rad) > by && (cy - rad) < (by + bh)) ||
               ((cx + rad) > bx && (cx - rad) < (bx + bw) && cy > by && cy < (by + bh)) ||
               DistSqr(cx, cy, bx, by) < (rad * rad) ||
               DistSqr(cx, cy, bx + bw, by) < (rad * rad) ||
               DistSqr(cx, cy, bx, by + bh) < (rad * rad) ||
               DistSqr(cx, cy, bx + bw, by + bh) < (rad * rad);
    }
}
