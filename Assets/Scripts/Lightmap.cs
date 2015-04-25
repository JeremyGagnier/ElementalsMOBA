using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class Lightmap : MonoBehaviour
{
    public const int ITERATIONS = 16;
    public const byte LIGHT_UNIT = 256 / ITERATIONS;
    public const byte DIAGONAL_UNIT = (byte)((float)LIGHT_UNIT * 1.414f);
    public const byte BLOCK_REDUCTION = 3;

	public byte[,] brightness;

	private World world;
	public int blockWidth;
	public int blockHeight;
	public int chunkx;
	public int chunky;

    private const float tTexDim = 16.0f;
    private const float tPixDim = 80.0f;
    private const float tUnit = 1.0f / tPixDim;

	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();
	public List<Vector2> newUV = new List<Vector2>();

	private Mesh mesh;
	private int squareCount;

	public bool isActive = false;
    public bool update = false;

    public GameObject spotlightPrefab;

    private Thread lightThread;

	public void Setup (World world, int bWidth, int bHeight, int cx, int cy)
	{
		this.mesh = GetComponent<MeshFilter> ().mesh;

		this.world = world;
		this.blockWidth = bWidth;
		this.blockHeight = bHeight;
		this.chunkx = cx;
		this.chunky = cy;
        this.transform.position = new Vector3(bWidth * cx, bHeight * cy, 0);
	}

    void Update()
    {
        if (update)
        {
            UpdateMesh();
            update = false;
        }
    }

	public void Activate (Blockmap bmap)
    {
        isActive = true;

        byte [][,] data = new byte [9][,];
        data[0] = world.ChunkAt(chunkx - 1, chunky - 1);
        data[1] = world.ChunkAt(chunkx, chunky - 1);
        data[2] = world.ChunkAt(chunkx + 1, chunky - 1);
        data[3] = world.ChunkAt(chunkx - 1, chunky);
        data[4] = world.ChunkAt(chunkx, chunky);
        data[5] = world.ChunkAt(chunkx + 1, chunky);
        data[6] = world.ChunkAt(chunkx - 1, chunky + 1);
        data[7] = world.ChunkAt(chunkx, chunky + 1);
        data[8] = world.ChunkAt(chunkx + 1, chunky + 1);

        lightThread = new Thread((bInfo) =>
        {
            CalculateLight(bInfo);
            BuildMesh();
            bmap.Activate(brightness);
            update = true;
        });
        lightThread.Start(data);
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

	void CalculateLight(object data)
	{
        brightness = new byte[blockWidth + 2 * ITERATIONS + 2, blockHeight + 2 * ITERATIONS + 2];

        byte[][,] blockData = (byte[][,])data;

        byte[,] blocks = new byte[blockWidth + 2 * ITERATIONS + 2, blockHeight + 2 * ITERATIONS + 2];

        int xIndex;
        int yIndex;

        // Parse blocks0 segment
        xIndex = 0;
        for (int x = blockWidth - ITERATIONS - 1; x < blockWidth; ++x)
        {
            yIndex = 0;
            for (int y = blockHeight - ITERATIONS - 1; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks1 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < blockWidth; ++x)
        {
            yIndex = 0;
            for (int y = blockHeight - ITERATIONS - 1; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[1][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks2 segment
        xIndex = blockWidth + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = 0;
            for (int y = blockHeight - ITERATIONS - 1; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[2][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks3 segment
        xIndex = 0;
        for (int x = blockWidth - ITERATIONS - 1; x < blockWidth; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[3][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks4 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < blockWidth; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[4][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks5 segment
        xIndex = blockWidth + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < blockHeight; ++y)
            {
                blocks[xIndex, yIndex] = blockData[5][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks6 segment
        xIndex = 0;
        for (int x = blockWidth - ITERATIONS - 1; x < blockWidth; ++x)
        {
            yIndex = blockHeight + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[6][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks7 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < blockWidth; ++x)
        {
            yIndex = blockHeight + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[7][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks8 segment
        xIndex = blockWidth + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = blockHeight + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[8][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        byte[,] oldBrightness;
        for (int i = 0; i < ITERATIONS; ++i)
        {
            oldBrightness = brightness;
		    brightness = new byte[blockWidth + 2 * ITERATIONS + 2, blockHeight + 2 * ITERATIONS + 2];

            for (int x = 1; x < blockWidth + 2 * ITERATIONS + 1; ++x)
            {
                for (int y = 1; y < blockHeight + 2 * ITERATIONS + 1; ++y)
                {
                    if (blocks[x, y] == 0)
                    {
                        brightness[x, y] = 255;
                    }
                    else
                    {
                        brightness[x, y] = (byte)Mathf.Max(0,
                            oldBrightness[x, y + 1] - BLOCK_REDUCTION * LIGHT_UNIT,
                            oldBrightness[x, y - 1] - BLOCK_REDUCTION * LIGHT_UNIT,
                            oldBrightness[x + 1, y] - BLOCK_REDUCTION * LIGHT_UNIT,
                            oldBrightness[x + 1, y - 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                            oldBrightness[x + 1, y + 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                            oldBrightness[x - 1, y] - BLOCK_REDUCTION * LIGHT_UNIT,
                            oldBrightness[x - 1, y - 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                            oldBrightness[x - 1, y + 1] - BLOCK_REDUCTION * DIAGONAL_UNIT);
                    }
                }
            }
        }

        oldBrightness = brightness;

        brightness = new byte[blockWidth, blockHeight];

        for (int x = 0; x < blockWidth; ++x)
        {
            for (int y = 0; y < blockHeight; ++y)
            {
                brightness[x, y] = oldBrightness[x + ITERATIONS + 1, y + ITERATIONS + 1];
            }
        }
	}

    void BuildMesh()
	{
		for(int px = 0; px < blockWidth; px++)
		{
			for(int py = 0; py < blockHeight; py++)
			{
                int lightLevel = ITERATIONS - ((int)brightness[px, py] + 1) / (int)LIGHT_UNIT;
                if (lightLevel != 0)
                {
                    GenSquare(px, py, new Vector2((float)lightLevel - 1, 0));
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
		return brightness[x,y];
	}

	public int DestroyBlock (int bx, int by)
	{
		int oldBlock = brightness[bx, by];
		brightness[bx, by] = 0;

		return oldBlock;
	}
}
