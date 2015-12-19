using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    public Hurtbox head = null;
    public Hurtbox leftShoulder = null;
    public Hurtbox rightShoulder = null;
    public Hurtbox leftElbow = null;
    public Hurtbox rightElbow = null;
    public Hurtbox leftHand = null;
    public Hurtbox rightHand = null;
    public Hurtbox body = null;
    public Hurtbox leftHip = null;
    public Hurtbox rightHip = null;
    public Hurtbox leftKnee = null;
    public Hurtbox rightKnee = null;
    public Hurtbox leftfoot = null;
    public Hurtbox rightfoot = null;

    void Awake()
    {
        onGrounded += () => { jumps = maxJumps; };
        moves = new List<Move>();

        // Add all the moves to the moves list for easier management.
        jump = new Jump(this);
        moves.Add(jump);
        fground = new FireForwardGround(this);
        moves.Add(fground);
        //moves.Add(bground);
        //moves.Add(dground);
        //moves.Add(uground);
        //moves.Add(fair);
        //moves.Add(bair);
        //moves.Add(dair);
        //moves.Add(uair);
        //moves.Add(f1);
        //moves.Add(b1);
        //moves.Add(d1);
        //moves.Add(u1);
        //moves.Add(f2);
        //moves.Add(b2);
        //moves.Add(d2);
        //moves.Add(u2);
        //moves.Add(f3);
        //moves.Add(b3);
        //moves.Add(d3);
        //moves.Add(u3);
        //moves.Add(f4);
        //moves.Add(b4);
        //moves.Add(d4);
        //moves.Add(u4);

        head = new Hurtbox();
        head.pos.x = new FInt(0);
        head.pos.y = new FInt(1);
        head.pos.r = new FInt(0.6);
        head.player = this;
        head.armor = 0;
        leftShoulder = new Hurtbox();
        leftShoulder.pos.x = new FInt(-0.5);
        leftShoulder.pos.y = new FInt(0.4);
        leftShoulder.pos.r = new FInt(0.3);
        leftShoulder.player = this;
        leftShoulder.armor = 0;
        rightShoulder = new Hurtbox();
        rightShoulder.pos.x = new FInt(0.5);
        rightShoulder.pos.y = new FInt(0.4);
        rightShoulder.pos.r = new FInt(0.3);
        rightShoulder.player = this;
        rightShoulder.armor = 0;
        body = new Hurtbox();
        body.pos.x = new FInt(0);
        body.pos.y = new FInt(0);
        body.pos.r = new FInt(0.5);
        body.player = this;
        body.armor = 0;
        leftKnee = new Hurtbox();
        leftKnee.pos.x = new FInt(-0.3);
        leftKnee.pos.y = new FInt(-0.7);
        leftKnee.pos.r = new FInt(0.3);
        leftKnee.player = this;
        leftKnee.armor = 0;
        rightKnee = new Hurtbox();
        rightKnee.pos.x = new FInt(0.3);
        rightKnee.pos.y = new FInt(-0.7);
        rightKnee.pos.r = new FInt(0.3);
        rightKnee.player = this;
        rightKnee.armor = 0;
        leftfoot = new Hurtbox();
        leftfoot.pos.x = new FInt(-0.3);
        leftfoot.pos.y = new FInt(-1.2);
        leftfoot.pos.r = new FInt(0.3);
        leftfoot.player = this;
        leftfoot.armor = 0;
        rightfoot = new Hurtbox();
        rightfoot.pos.x = new FInt(0.3);
        rightfoot.pos.y = new FInt(-1.2);
        rightfoot.pos.r = new FInt(0.3);
        rightfoot.player = this;
        rightfoot.armor = 0;
    }

    void Start()
    {
        manager.hurtboxes.Add(head);
        manager.hurtboxes.Add(leftShoulder);
        manager.hurtboxes.Add(rightShoulder);
        //manager.hurtboxes.Add(leftElbow);
        //manager.hurtboxes.Add(rightElbow);
        //manager.hurtboxes.Add(leftHand);
        //manager.hurtboxes.Add(rightHand);
        manager.hurtboxes.Add(body);
        //manager.hurtboxes.Add(leftHip);
        //manager.hurtboxes.Add(rightHip);
        manager.hurtboxes.Add(leftKnee);
        manager.hurtboxes.Add(rightKnee);
        manager.hurtboxes.Add(leftfoot);
        manager.hurtboxes.Add(rightfoot);
    }
}
