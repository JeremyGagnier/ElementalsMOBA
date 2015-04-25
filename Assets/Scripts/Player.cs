using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public const int MAX_FALL_SPEED = 30;

	public float moveSpeed = 10;
	public float height = 1;
	public float width = 1;

	void Start ()
	{
	}

	void Update ()
	{
        if (Input.GetAxis("Horizontal") != 0)
		{
			transform.position += new Vector3(moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal"), 0, 0);
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.position += new Vector3(0, moveSpeed * Time.deltaTime * Input.GetAxis("Vertical"), 0);
        }

        //Debug.Log(Input.GetAxis("Horizontal").ToString("n3") + ", " + Input.GetAxis("Vertical").ToString("n3"));
	}
}
