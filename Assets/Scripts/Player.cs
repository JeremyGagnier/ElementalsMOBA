using UnityEngine;
using System.Collections;

public class Player : PhysicsMover {
	public const int MAX_FALL_SPEED = 30;

	public float moveSpeed = 10;

	void Start ()
    {
        mass = (int)(weight * (1 << 16));
        fallSpeed = (int)(fallingSpeed * (1 << 16));
        fallAccel = (int)(fallingAcceleration * 0.01666f * (1 << 16));

        position.x = position.x << 16;
        position.y = position.y << 16;

        float px = (float)(position.x >> 16) + (float)(position.x % (1 << 16)) / (float)(1 << 16);
        float py = (float)(position.y >> 16) + (float)(position.y % (1 << 16)) / (float)(1 << 16);
        transform.position = new Vector3(px, py, 0);
	}

	void Update ()
	{
        /*
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.position += new Vector3(0, moveSpeed * Time.deltaTime * Input.GetAxis("Vertical"), 0);
        }*/

        //Debug.Log(Input.GetAxis("Horizontal").ToString("n3") + ", " + Input.GetAxis("Vertical").ToString("n3"));
	}

    public override Tuple ApplyInput(Tuple vel)
    {
        float xInput = Input.GetAxis("Horizontal");
        if (carried)
        {
            vel.x += (int)(xInput * horizontalAirSpeed * (1 << 16));
        }
        else
        {
                vel.x = (int)(xInput * horizontalAirSpeed * (1 << 16));
        }
        return vel;
    }
}
