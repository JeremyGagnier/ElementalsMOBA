using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
	public const int MAX_FALL_SPEED = 30;

	public float moveSpeed = 10;
	public float height = 1;
	public float width = 1;
	private float heightOffset = 0;

	private bool grounded = false;

	private LayerMask layerMask = (1 << 0);

	// Use this for initialization
	void Start ()
	{
		//CapsuleCollider capsule = GetComponent<CapsuleCollider> ();
		//height = capsule.bounds.extents.y;
		//heightOffset = -0.04f * 10;
		//width = capsule.bounds.extents.x;
	}

	// Update is called once per frame
	void Update ()
	{
        /*
		if (this.GetComponent<Rigidbody>().velocity.sqrMagnitude > MAX_FALL_SPEED*MAX_FALL_SPEED)
		{
			this.GetComponent<Rigidbody>().velocity = MAX_FALL_SPEED * this.GetComponent<Rigidbody>().velocity.normalized;
		}
         */
		RaycastHit hit;
		if (Physics.Raycast (transform.position, new Vector3(0,-1,0), out hit, 1 + height - heightOffset, layerMask))
		{
			grounded = true;
		}
		else
		{
			grounded = false;
		}
		
		if (Input.GetKey (KeyCode.LeftArrow))
		{
			if (!Physics.Raycast (transform.position + Vector3.up * (height - heightOffset), new Vector3(-1,0,0), out hit, width, layerMask))
			{
				transform.position -= new Vector3(moveSpeed * Time.deltaTime, 0, 0);
			}
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (!Physics.Raycast(transform.position + Vector3.up * (height - heightOffset), new Vector3(1, 0, 0), out hit, width, layerMask))
            {
                transform.position += new Vector3(moveSpeed * Time.deltaTime, 0, 0);
            }
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (!Physics.Raycast(transform.position + Vector3.up * (height - heightOffset), new Vector3(1, 0, 0), out hit, width, layerMask))
            {
                transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (!Physics.Raycast(transform.position + Vector3.up * (height - heightOffset), new Vector3(1, 0, 0), out hit, width, layerMask))
            {
                transform.position -= new Vector3(0, moveSpeed * Time.deltaTime, 0);
            }
        }


		if (grounded && Input.GetKeyDown (KeyCode.Space))
		{
			//GetComponent<Rigidbody>().AddForce(new Vector3(0, 40, 0), ForceMode.Impulse);
			grounded = false;
		}
	}
}
