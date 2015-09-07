using UnityEngine;
using System.Collections;

public class FireForwardGround : Move {

    public override void Step(CombatManager mgr)
    {
        switch (currentFrame)
        {
            case 0:
                break;
            case 1:
                break;
        }
        currentFrame += 1;
    }

    public override bool Trigger(CombatManager mgr)
    {
        if (Input.GetAxis("") == 1 ||
            owner.blockingMove)
        {
            return false;
        }



        return true;
    }
}
