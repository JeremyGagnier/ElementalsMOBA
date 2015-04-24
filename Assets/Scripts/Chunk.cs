using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Chunk : MonoBehaviour
{
	public byte[,] blocks;

	private World world;
	public int blockWidth;
	public int blockHeight;
	public int chunkx;
	public int chunky;

    private const float tTexDim = 4.0f;
    private const float tPixDim = 72.0f;
    private const float tUnit = 16.0f / tPixDim;

    private Vector2 tGrass = new Vector2(0, 0);
    private Vector2 tStone = new Vector2(1, 0);
    private Vector2 tStoneWall = new Vector2(2, 0);

	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();
	public List<Vector2> newUV = new List<Vector2>();

	private Mesh mesh;
	
	private int squareCount;

	public bool update = false;
	public bool isActive = false;

	public GameObject spotlightPrefab;

	public void Setup (World world, int bWidth, int bHeight, int cx, int cy)
	{
		this.mesh = GetComponent<MeshFilter> ().mesh;

		this.world = world;
		this.blockWidth = bWidth;
		this.blockHeight = bHeight;
		this.chunkx = cx;
		this.chunky = cy;
		this.transform.position = new Vector3(bWidth*cx, bHeight*cy, 0);

		this.GenTerrain ();
	}

	public void Activate ()
	{
		BuildMesh ();
		UpdateMesh ();
		isActive = true;
	}

	void Awake ()
	{
	}

	void Start ()
	{
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (update)
		{
			BuildMesh ();
			UpdateMesh ();
			update = false;
		}
	}

	void UpdateMesh ()
	{
		mesh.Clear ();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.Optimize ();
		mesh.RecalculateNormals ();

		squareCount = 0;
		newVertices.Clear ();
		newTriangles.Clear ();
		newUV.Clear ();
	}

	void GenSquare(int x, int y, Vector2 texture)
	{
        newVertices.Add(new Vector3(x, y, 0));
        newVertices.Add(new Vector3(x, y + 1, 0));
        newVertices.Add(new Vector3(x + 1, y + 1, 0));
        newVertices.Add(new Vector3(x + 1, y, 0));

        newTriangles.Add(squareCount * 4);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 3);
        newTriangles.Add((squareCount * 4) + 1);
        newTriangles.Add((squareCount * 4) + 2);
        newTriangles.Add((squareCount * 4) + 3);

        squareCount++;

        newUV.Add(new Vector2(texture.x / tTexDim + 1 / tPixDim, texture.y / tTexDim + 1 / tPixDim));
        newUV.Add(new Vector2(texture.x / tTexDim + 1 / tPixDim, texture.y / tTexDim + 1 / tPixDim + tUnit));
        newUV.Add(new Vector2(texture.x / tTexDim + 1 / tPixDim + tUnit, texture.y / tTexDim + 1 / tPixDim + tUnit));
        newUV.Add(new Vector2(texture.x / tTexDim + 1 / tPixDim + tUnit, texture.y / tTexDim + 1 / tPixDim));
	}

	// Scale defines smoothness, mag is magnitude and exp is exponent.
	int Noise (int x, int y, float scale, float mag, float exp)
	{
		return (int)(Mathf.Pow ((Mathf.PerlinNoise (x/scale, y/scale)*mag), exp));
	}

	void GenTerrain()
	{
		blocks = new byte[blockWidth, blockHeight];

		for(int px = 0; px < blockWidth; px++)
		{
			int truepx = px + chunkx*blockWidth;

			int stone = Noise(truepx, 0, 80, 15, 1);
			stone += Noise (truepx, 0, 50, 30, 1);
			stone += Noise (truepx, 0, 10, 10, 1);
			stone += blockHeight * world.chunkHeight / 2;

			int dirt = Noise (truepx, 0, 100, 35, 1);
			dirt += Noise (truepx, 0, 50, 30, 1);
			dirt += blockHeight * world.chunkHeight / 2;

			for(int py = 0; py < blockHeight; py++)
			{
				int truepy = py + chunky*blockHeight;
				if (truepy < stone)
				{
					if (Noise(truepx, truepy*2, 16, 14, 1) > 10)
					{
						blocks[px, py] = 0;
					}
					else if (Noise(truepx, truepy, 12, 16, 1) > 10)
					{
						blocks[px, py] = 2;
					}
					else
					{
						blocks[px, py] = 1;
					}
				}
				else if (truepy < dirt)
				{
					blocks[px,py] = 2;
				}
			}
		}
	}

	void BuildMesh ()
	{
		for(int px = 0; px < blockWidth; px++)
		{
			for(int py = 0; py < blockHeight; py++)
			{
				if(blocks[px,py] != 0)
				{
					if (blocks[px,py] == 1)
					{
						GenSquare (px, py, tStoneWall);
					}
					else if (blocks[px,py] == 2)
					{
						GenSquare (px, py, tGrass);
					}
				}
			}
		}
	}

	byte Block (int x, int y)
	{
		int cx = chunkx;
		int cy = chunky;
		int bx = x;
		int by = y;
		bool changed = false;
		if (x == -1)
		{
			cx -= 1;
			bx = blockWidth - 1;
			changed = true;
		}
		if (x == blockWidth)
		{
			cx += 1;
			bx = 0;
			changed = true;
		}
		if (y == -1)
		{
			cy -= 1;
			by = blockHeight - 1;
			changed = true;
		}
		if (y == blockHeight)
		{
			cy += 1;
			by = 0;
			changed = true;
		}
		
		if(changed)
		{
			return world.BlockAt (cx, cy, bx, by);
		}
		return blocks[x,y];
	}

	public int DestroyBlock (int bx, int by)
	{
		int oldBlock = blocks[bx, by];
		blocks[bx, by] = 0;

		update = true;

		return oldBlock;
	}
}
