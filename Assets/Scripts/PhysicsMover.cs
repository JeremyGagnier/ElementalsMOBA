using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PhysicsMover : MonoBehaviour {

    [SerializeField]
    public List<Vector4> hitbox;

    public float weight;
    [HideInInspector]
    public int mass;

    public float verticalAirSpeed;
    public float horizontalAirSpeed;

    public float fallingSpeed;
    public float fallingAcceleration;
    [HideInInspector]
    public int fallSpeed;
    [HideInInspector]
    public int fallAccel;

    public bool allowInput = true;
    public bool carried = false;

    public Tuple position = new Tuple(0, 0);
    public Tuple velocity = new Tuple(0, 0);

    void OnDrawGizmos()
    {
        foreach (Vector4 pos in hitbox)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawCube(transform.position + new Vector3(pos.x + pos.z / 2, pos.y + pos.w / 2, 0), 
                            new Vector3(pos.z, pos.w, 1));

            int minBlockx = (position.x + (int)(pos.x * (1 << 16))) >> 16;
            int minBlocky = (position.y + (int)(pos.y * (1 << 16))) >> 16;
            int fractionx = (position.x + (int)(pos.x * (1 << 16))) % (1 << 16);
            int fractiony = (position.y + (int)(pos.y * (1 << 16))) % (1 << 16);

            int width = Mathf.CeilToInt(pos.z) + 1;
            if (fractionx == 0)
            {
                width -= 1;
            }

            int height = Mathf.CeilToInt(pos.w) + 1;
            if (fractiony == 0)
            {
                height -= 1;
            }

            Gizmos.color = new Color(0, 0, 1, 0.2f);
            for (int x = minBlockx; x < minBlockx + width; ++x)
            {
                for (int y = minBlocky; y < minBlocky + height; ++y)
                {
                    Gizmos.DrawCube(new Vector3(x + 0.5f, y + 0.5f, 0), new Vector3(1, 1, 1));
                }
            }
        }
    }

    void Start()
    {
        mass = (int)(weight * (1 << 16));
        fallSpeed = (int)(fallingSpeed * (1 << 16));
        fallAccel = (int)(fallingAcceleration * 0.01666f * (1 << 16));
    }

    public virtual Tuple ApplyInput(Tuple vel)
    {
        return vel;
    }
}
