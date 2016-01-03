using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager
{
    public bool isLocalPlayer = false;

    private List<int> jump = new List<int>();
    private int jumpCtr = 0;

    private List<int> dodge = new List<int>();
    private int dodgeCtr = 0;

    private List<int> right = new List<int>();
    private int rightCtr = 0;

    private List<int> left = new List<int>();
    private int leftCtr = 0;

    private List<int> down = new List<int>();
    private int downCtr = 0;

    private List<int> up = new List<int>();
    private int upCtr = 0;

    private List<int> rightAttack = new List<int>();
    private int rightAttackCtr = 0;

    private List<int> leftAttack = new List<int>();
    private int leftAttackCtr = 0;

    private List<int> downAttack = new List<int>();
    private int downAttackCtr = 0;

    private List<int> upAttack = new List<int>();
    private int upAttackCtr = 0;

    private List<int> b1 = new List<int>();
    private int b1Ctr = 0;

    private List<int> b2 = new List<int>();
    private int b2Ctr = 0;

    private List<int> b3 = new List<int>();
    private int b3Ctr = 0;

    private List<int> b4 = new List<int>();
    private int b4Ctr = 0;

    // The following are the variables that can be accessed
    // by other classes.
    // The Pressed boolean values are determined by checking
    // if that input has changed an odd number of times.
    // The JustPressed boolean values are only on for the
    // frame in which they changed.
    public bool jumpJustPressed;
    public bool jumpPressed
    {
        get { return (jump.Count % 2 == 1); }
    }

    public bool dodgeJustPressed;
    public bool dodgePressed
    {
        get { return (dodge.Count % 2 == 1); }
    }

    public bool rightJustPressed;
    public bool rightPressed
    {
        get { return (right.Count % 2 == 1); }
    }

    public bool leftJustPressed;
    public bool leftPressed
    {
        get { return (left.Count % 2 == 1); }
    }

    public bool downJustPressed;
    public bool downPressed
    {
        get { return (down.Count % 2 == 1); }
    }

    public bool upJustPressed;
    public bool upPressed
    {
        get { return (up.Count % 2 == 1); }
    }

    public bool rightAttackJustPressed;
    public bool rightAttackPressed
    {
        get { return (rightAttack.Count % 2 == 1); }
    }

    public bool leftAttackJustPressed;
    public bool leftAttackPressed
    {
        get { return (leftAttack.Count % 2 == 1); }
    }

    public bool downAttackJustPressed;
    public bool downAttackPressed
    {
        get { return (downAttack.Count % 2 == 1); }
    }

    public bool upAttackJustPressed;
    public bool upAttackPressed
    {
        get { return (upAttack.Count % 2 == 1); }
    }

    public bool b1JustPressed;
    public bool b1Pressed
    {
        get { return (b1.Count % 2 == 1); }
    }

    public bool b2JustPressed;
    public bool b2Pressed
    {
        get { return (b2.Count % 2 == 1); }
    }

    public bool b3JustPressed;
    public bool b3Pressed
    {
        get { return (b3.Count % 2 == 1); }
    }

    public bool b4JustPressed;
    public bool b4Pressed
    {
        get { return (b4.Count % 2 == 1); }
    }

    public void Advance(int frames)
    {
        float jumpInput = 0f;
        float dodgeInput = 0f;
        float horizontal = 0f;
        float vertical = 0f;
        float horizontalAttack = 0f;
        float verticalAttack = 0f;
        float b1Input = 0f;
        float b2Input = 0f;
        float b3Input = 0f;
        float b4Input = 0f;
        if (isLocalPlayer)
        {
            jumpInput = Input.GetAxis("Jump");
            dodgeInput = Input.GetAxis("Dodge");
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
            horizontalAttack = Input.GetAxis("HorizontalAttack");
            verticalAttack = Input.GetAxis("VerticalAttack");
            b1Input = Input.GetAxis("1");
            b2Input = Input.GetAxis("2");
            b3Input = Input.GetAxis("3");
            b4Input = Input.GetAxis("4");
        }
        else
        {
            // Pull input info from another source...
        }

        for (int i = 0; i < frames; ++i)
        {
            jumpCtr += 1;
            dodgeCtr += 1;
            rightCtr += 1;
            leftCtr += 1;
            downCtr += 1;
            upCtr += 1;
            rightAttackCtr += 1;
            leftAttackCtr += 1;
            downAttackCtr += 1;
            upAttackCtr += 1;
            b1Ctr += 1;
            b2Ctr += 1;
            b3Ctr += 1;
            b4Ctr += 1;

            if ((jump.Count % 2 == 0 && jumpInput == 1) ||
                (jump.Count % 2 == 1 && jumpInput != 1))
            {
                jump.Add(jumpCtr);
                jumpCtr = 0;
                jumpJustPressed = (jump.Count % 2 == 1);
            }
            else
            {
                jumpJustPressed = false;
            }

            if ((dodge.Count % 2 == 0 && dodgeInput == 1) ||
                (dodge.Count % 2 == 1 && dodgeInput != 1))
            {
                dodge.Add(dodgeCtr);
                dodgeCtr = 0;
                dodgeJustPressed = (dodge.Count % 2 == 1);
            }
            else
            {
                dodgeJustPressed = false;
            }

            if ((right.Count % 2 == 0 && horizontal == 1) ||
                (right.Count % 2 == 1 && horizontal != 1))
            {
                right.Add(rightCtr);
                rightCtr = 0;
                rightJustPressed = (right.Count % 2 == 1);
            }
            else
            {
                rightJustPressed = false;
            }
            if ((left.Count % 2 == 0 && horizontal == -1) ||
                (left.Count % 2 == 1 && horizontal != -1))
            {
                left.Add(leftCtr);
                leftCtr = 0;
                leftJustPressed = (left.Count % 2 == 1);
            }
            else
            {
                leftJustPressed = false;
            }

            if ((up.Count % 2 == 0 && vertical == 1) ||
                (up.Count % 2 == 1 && vertical != 1))
            {
                up.Add(upCtr);
                upCtr = 0;
                upJustPressed = (up.Count % 2 == 1);
            }
            else
            {
                upJustPressed = false;
            }
            if ((down.Count % 2 == 0 && vertical == -1) ||
                (down.Count % 2 == 1 && vertical != -1))
            {
                down.Add(downCtr);
                downCtr = 0;
                downJustPressed = (down.Count % 2 == 1);
            }
            else
            {
                downJustPressed = false;
            }

            if ((rightAttack.Count % 2 == 0 && horizontalAttack == 1) ||
                (rightAttack.Count % 2 == 1 && horizontalAttack != 1))
            {
                rightAttack.Add(rightAttackCtr);
                rightAttackCtr = 0;
                rightAttackJustPressed = (rightAttack.Count % 2 == 1);
            }
            else
            {
                rightAttackJustPressed = false;
            }
            if ((leftAttack.Count % 2 == 0 && horizontalAttack == -1) ||
                (leftAttack.Count % 2 == 1 && horizontalAttack != -1))
            {
                leftAttack.Add(leftAttackCtr);
                leftAttackCtr = 0;
                leftAttackJustPressed = (leftAttack.Count % 2 == 1);
            }
            else
            {
                leftAttackJustPressed = false;
            }

            if ((upAttack.Count % 2 == 0 && verticalAttack == 1) ||
                (upAttack.Count % 2 == 1 && verticalAttack != 1))
            {
                upAttack.Add(upAttackCtr);
                upAttackCtr = 0;
                upAttackJustPressed = (upAttack.Count % 2 == 1);
            }
            else
            {
                upAttackJustPressed = false;
            }
            if ((downAttack.Count % 2 == 0 && verticalAttack == -1) ||
                (downAttack.Count % 2 == 1 && verticalAttack != -1))
            {
                downAttack.Add(downAttackCtr);
                downAttackCtr = 0;
                downAttackJustPressed = (downAttack.Count % 2 == 1);
            }
            else
            {
                downAttackJustPressed = false;
            }

            if ((b1.Count % 2 == 0 && b1Input == 1) ||
                (b1.Count % 2 == 1 && b1Input != 1))
            {
                b1.Add(b1Ctr);
                b1Ctr = 0;
                b1JustPressed = (b1.Count % 2 == 1);
            }
            else
            {
                b1JustPressed = false;
            }

            if ((b2.Count % 2 == 0 && b2Input == 1) ||
                (b2.Count % 2 == 1 && b2Input != 1))
            {
                b2.Add(b2Ctr);
                b2Ctr = 0;
                b2JustPressed = (b2.Count % 2 == 1);
            }
            else
            {
                b2JustPressed = false;
            }

            if ((b3.Count % 2 == 0 && b3Input == 1) ||
                (b3.Count % 2 == 1 && b3Input != 1))
            {
                b3.Add(b3Ctr);
                b3Ctr = 0;
                b3JustPressed = (b3.Count % 2 == 1);
            }
            else
            {
                b3JustPressed = false;
            }

            if ((b4.Count % 2 == 0 && b4Input == 1) ||
                (b4.Count % 2 == 1 && b4Input != 1))
            {
                b4.Add(b4Ctr);
                b4Ctr = 0;
                b4JustPressed = (b4.Count % 2 == 1);
            }
            else
            {
                b4JustPressed = false;
            }
        }
    }
}
