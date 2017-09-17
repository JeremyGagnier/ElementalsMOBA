using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Backmap : MonoBehaviour
{
	public byte[,] bgBlocks;

	private World world;
	public int numBlocksWide;
	public int numBlocksHigh;
	public int chunkPosX;
	public int chunkPosY;

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
    private Dictionary<Tuple, int> squares = new Dictionary<Tuple, int>();

    public bool update = false;
	public bool isActive = false;

	public void Setup (World world, int bWidth, int bHeight, int cx, int cy)
	{
		this.mesh = GetComponent<MeshFilter> ().mesh;

		this.world = world;
		this.numBlocksWide = bWidth;
		this.numBlocksHigh = bHeight;
		this.chunkPosX = cx;
		this.chunkPosY = cy;
		this.transform.position = new Vector3(bWidth*cx, bHeight*cy, 0);

		this.GenTerrain ();
	}

	public void Activate (float[,] brightness, byte[,] blockmap)
    {
        isActive = true;
        ClearMeshInfo();
        BuildMesh(brightness, blockmap);
        update = true;
	}

    void Update()
    {
        if (update)
        {
            UpdateMesh();
            update = false;
        }
    }

	void UpdateMesh ()
	{
		mesh.Clear ();
		mesh.vertices = newVertices.ToArray();
		mesh.triangles = newTriangles.ToArray();
		mesh.uv = newUV.ToArray();
		mesh.RecalculateNormals ();
	}

    void ClearMeshInfo()
    {
        squareCount = 0;
        squares.Clear();
        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();
    }

	void GenSquare(int x, int y)
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

        squares.Add(new Tuple(x, y), squareCount);
        squareCount += 1;

        Vector2 texture = GetTextureAt(x, y);
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
		bgBlocks = new byte[numBlocksWide, numBlocksHigh];

		for(int px = 0; px < numBlocksWide; px++)
		{
			int truepx = px + chunkPosX*numBlocksWide;

			int stone = Noise(truepx, 0, 80, 15, 1);
			stone += Noise (truepx, 0, 50, 30, 1);
			stone += Noise (truepx, 0, 10, 10, 1);
			stone += numBlocksHigh * world.numChunksHigh / 2;

			int dirt = Noise (truepx, 0, 100, 35, 1);
			dirt += Noise (truepx, 0, 50, 30, 1);
			dirt += numBlocksHigh * world.numChunksHigh / 2;

			for(int py = 0; py < numBlocksHigh; py++)
			{
				int truepy = py + chunkPosY*numBlocksHigh;
				if (truepy < stone)
				{
					if (Noise(truepx, truepy, 12, 16, 1) > 10)
					{
						bgBlocks[px, py] = 2;
					}
					else
					{
						bgBlocks[px, py] = 1;
					}
				}
				else if (truepy < dirt)
				{
					bgBlocks[px, py] = 2;
				}
			}
		}
	}

	void BuildMesh (float[,] brightness, byte[,] blockmap)
	{
		for(int px = 0; px < numBlocksWide; px++)
		{
			for(int py = 0; py < numBlocksHigh; py++)
			{
				if(bgBlocks[px, py] != 0 && blockmap[px,py] == 0 && brightness[px, py] != 0f)
				{
                    GenSquare(px, py);
				}
			}
		}
	}

	public int DestroyBack (int bx, int by)
	{
		int oldBlock = bgBlocks[bx, by];
		bgBlocks[bx, by] = 0;

		return oldBlock;
	}


    public Vector2 GetTextureAt(int x, int y)
    {
        if (bgBlocks[x, y] == 1)
        {
            return tStone;
        }
        else if (bgBlocks[x, y] == 2)
        {
            return tGrass;
        }
        else if (bgBlocks[x, y] == 3)
        {
            return tStoneWall;
        }
        return new Vector2();
    }

    public void Redraw(float light, byte block, int x, int y)
    {
        if (bgBlocks[x, y] != 0 && light != 0f && block == 0)
        {
            Tuple pos = new Tuple(x, y);
            if (squares.ContainsKey(pos))
            {
                Vector2 texture = GetTextureAt(x, y);
                newUV[squares[pos] * 4] = new Vector2(texture.x / tTexDim + 1 / tPixDim, texture.y / tTexDim + 1 / tPixDim);
                newUV[squares[pos] * 4 + 1] = new Vector2(texture.x / tTexDim + 1 / tPixDim, texture.y / tTexDim + 1 / tPixDim + tUnit);
                newUV[squares[pos] * 4 + 2] = new Vector2(texture.x / tTexDim + 1 / tPixDim + tUnit, texture.y / tTexDim + 1 / tPixDim + tUnit);
                newUV[squares[pos] * 4 + 3] = new Vector2(texture.x / tTexDim + 1 / tPixDim + tUnit, texture.y / tTexDim + 1 / tPixDim);
            }
            else
            {
                GenSquare(x, y);
            }
        }
        update = true;
    }

}
