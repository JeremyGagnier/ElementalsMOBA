using UnityEngine;
using System.Collections;

public class RaycastExample : MonoBehaviour
{
	public GameObject terrain;
	private Blockmap tScript;
	public GameObject target;
	private LayerMask layerMask = (1 << 0);

	void Start ()
	{
		tScript = terrain.GetComponent ("PolygonGenerator") as Blockmap;
	}

	void Update ()
	{
		RaycastHit hit;

		float distance = Vector3.Distance (transform.position, target.transform.position);

		if (Physics.Raycast (transform.position, (target.transform.position - transform.position).normalized, out hit, distance, layerMask))
		{
			Debug.DrawLine (transform.position, hit.point, Color.red);

			Vector3 point = new Vector3(hit.point.x, hit.point.y, 10);
			point += (new Vector3 (hit.normal.x, hit.normal.y, 0)) * -0.5f;

			Debug.DrawLine (hit.point, point, Color.green);

			tScript.blocks[Mathf.RoundToInt(point.x - 0.5f), Mathf.RoundToInt(point.y + 0.5f)] = 0;
		}
		else
		{
			Debug.DrawLine (transform.position, target.transform.position, Color.blue);
		}
	}
}
