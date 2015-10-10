using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

public class Lightmap : MonoBehaviour
{
    public const int ITERATIONS = 16;
    public const byte LIGHT_UNIT = 256 / ITERATIONS;
    public const byte DIAGONAL_UNIT = (byte)((float)LIGHT_UNIT * 1.414f);
    public const byte BLOCK_REDUCTION = 3;

	public byte[,] brightness;
    public HashSet<Tuple> lightSources = new HashSet<Tuple>();

	private World world;
	public int numBlocksWide;
	public int numBlocksHigh;
	public int chunkPosX;
	public int chunkPosY;

    private const float tTexDim = 256.0f;
    private const float tPixDim = 256.0f;
    private const float tUnit = 1.0f / tPixDim;

	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();
	public List<Vector2> newUV = new List<Vector2>();

	private Mesh mesh;
	private int squareCount;

	public bool isActive = false;
    public bool update = false;

    private Thread lightThread;

	public void Setup(World world, int bWidth, int bHeight, int cx, int cy)
	{
		this.mesh = GetComponent<MeshFilter>().mesh;

		this.world = world;
		this.numBlocksWide = bWidth;
		this.numBlocksHigh = bHeight;
		this.chunkPosX = cx;
		this.chunkPosY = cy;
        this.transform.position = new Vector3(bWidth * cx, bHeight * cy, 0);
	}

    void Update()
    {
        if (update)
        {
            update = false;
            BuildMesh();
            UpdateMesh();
        }
    }

	public void Activate(Blockmap bmap, Backmap bkmap)
    {
        isActive = true;

        Dictionary<string, object> data = new Dictionary<string,object>();
        byte[][,] block = new byte[9][,];
        block[0] = world.BlockmapAt(chunkPosX - 1, chunkPosY - 1);
        block[1] = world.BlockmapAt(chunkPosX    , chunkPosY - 1);
        block[2] = world.BlockmapAt(chunkPosX + 1, chunkPosY - 1);
        block[3] = world.BlockmapAt(chunkPosX - 1, chunkPosY    );
        block[4] = world.BlockmapAt(chunkPosX    , chunkPosY    );
        block[5] = world.BlockmapAt(chunkPosX + 1, chunkPosY    );
        block[6] = world.BlockmapAt(chunkPosX - 1, chunkPosY + 1);
        block[7] = world.BlockmapAt(chunkPosX    , chunkPosY + 1);
        block[8] = world.BlockmapAt(chunkPosX + 1, chunkPosY + 1);
        data["block"] = block;
        byte[][,] back = new byte[9][,];
        back[0] = world.BackmapAt(chunkPosX - 1, chunkPosY - 1);
        back[1] = world.BackmapAt(chunkPosX    , chunkPosY - 1);
        back[2] = world.BackmapAt(chunkPosX + 1, chunkPosY - 1);
        back[3] = world.BackmapAt(chunkPosX - 1, chunkPosY    );
        back[4] = world.BackmapAt(chunkPosX    , chunkPosY    );
        back[5] = world.BackmapAt(chunkPosX + 1, chunkPosY    );
        back[6] = world.BackmapAt(chunkPosX - 1, chunkPosY + 1);
        back[7] = world.BackmapAt(chunkPosX    , chunkPosY + 1);
        back[8] = world.BackmapAt(chunkPosX + 1, chunkPosY + 1);
        data["back"] = back;
        HashSet<Tuple>[] light = new HashSet<Tuple>[9];
        light[0] = world.LightSourceAt(chunkPosX - 1, chunkPosY - 1);
        light[1] = world.LightSourceAt(chunkPosX    , chunkPosY - 1);
        light[2] = world.LightSourceAt(chunkPosX + 1, chunkPosY - 1);
        light[3] = world.LightSourceAt(chunkPosX - 1, chunkPosY    );
        light[4] = this.lightSources;
        light[5] = world.LightSourceAt(chunkPosX + 1, chunkPosY    );
        light[6] = world.LightSourceAt(chunkPosX - 1, chunkPosY + 1);
        light[7] = world.LightSourceAt(chunkPosX    , chunkPosY + 1);
        light[8] = world.LightSourceAt(chunkPosX + 1, chunkPosY + 1);
        data["light"] = light;

        /*
        lightThread = new Thread((bInfo) =>
        {
            CalculateLight(bInfo);
            bmap.Activate(brightness);
            bkmap.Activate(brightness, bmap.blocks);
        });
        lightThread.Start(data);
        */

        CalculateLight(data);
        bmap.Activate(brightness);
        bkmap.Activate(brightness, bmap.blocks);
    }

    void BuildMesh()
    {
        for (int px = 0; px < numBlocksWide; px++)
        {
            for (int py = 0; py < numBlocksHigh; py++)
            {
                int lightLevel = 255 - (int)brightness[px, py];
                if (lightLevel != 0)
                {
                    GenSquare(px, py, new Vector2((float)lightLevel, 0));
                }
            }
        }
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

    void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = newVertices.ToArray();
        mesh.triangles = newTriangles.ToArray();
        mesh.uv = newUV.ToArray();
        mesh.Optimize();
        mesh.RecalculateNormals();

        squareCount = 0;
        newVertices.Clear();
        newTriangles.Clear();
        newUV.Clear();
    }

	void CalculateLight(object data)
	{
        brightness = new byte[numBlocksWide + 2 * ITERATIONS + 2, numBlocksHigh + 2 * ITERATIONS + 2];

        Dictionary<string, object> info = (Dictionary<string, object>)data;
        byte[][][,] blockData = new byte[2][][,];
        blockData[0] = (byte[][,])info["block"];
        blockData[1] = (byte[][,])info["back"];
        HashSet<Tuple>[] lightData = (HashSet<Tuple>[])info["light"];

        byte[,] blocks = new byte[numBlocksWide + 2 * ITERATIONS + 2, numBlocksHigh + 2 * ITERATIONS + 2];
        byte[,] bgBlocks = new byte[numBlocksWide + 2 * ITERATIONS + 2, numBlocksHigh + 2 * ITERATIONS + 2];

        int xIndex;
        int yIndex;

        // Parse blocks0 segment
        xIndex = 0;
        for (int x = numBlocksWide - ITERATIONS - 1; x < numBlocksWide; ++x)
        {
            yIndex = 0;
            for (int y = numBlocksHigh - ITERATIONS - 1; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][0][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][0][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks1 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < numBlocksWide; ++x)
        {
            yIndex = 0;
            for (int y = numBlocksHigh - ITERATIONS - 1; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][1][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][1][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks2 segment
        xIndex = numBlocksWide + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = 0;
            for (int y = numBlocksHigh - ITERATIONS - 1; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][2][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][2][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks3 segment
        xIndex = 0;
        for (int x = numBlocksWide - ITERATIONS - 1; x < numBlocksWide; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][3][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][3][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks4 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < numBlocksWide; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][4][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][4][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks5 segment
        xIndex = numBlocksWide + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = ITERATIONS + 1;
            for (int y = 0; y < numBlocksHigh; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][5][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][5][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks6 segment
        xIndex = 0;
        for (int x = numBlocksWide - ITERATIONS - 1; x < numBlocksWide; ++x)
        {
            yIndex = numBlocksHigh + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][6][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][6][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks7 segment
        xIndex = ITERATIONS + 1;
        for (int x = 0; x < numBlocksWide; ++x)
        {
            yIndex = numBlocksHigh + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][7][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][7][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

        // Parse blocks8 segment
        xIndex = numBlocksWide + ITERATIONS + 1;
        for (int x = 0; x < ITERATIONS + 1; ++x)
        {
            yIndex = numBlocksHigh + ITERATIONS + 1;
            for (int y = 0; y < ITERATIONS + 1; ++y)
            {
                blocks[xIndex, yIndex] = blockData[0][8][x, y];
                bgBlocks[xIndex, yIndex] = blockData[1][8][x, y];
                ++yIndex;
            }
            ++xIndex;
        }

		var watch = Stopwatch.StartNew();

		/*
		 * Algorithm #1
		 * 
		 * This algorithm goes through every block ITERATIONS times.
		 * 
		 * Worse Case:
		 * Array index count: O(ITERATIONS * (2 * ITERATIONS + blockWidth) * (2 * ITERATIONS + blockHeight) * 9)
		 * Arithmetic operations count: O(ITERATIONS * (2 * ITERATIONS + blockWidth) * (2 * ITERATIONS + blockHeight) * 18)
		 * 
		 * Best Case:
		 * Array index count: O(ITERATIONS * (2 * ITERATIONS + blockWidth) * (2 * ITERATIONS + blockHeight))
		 * Arithmetic operations count: O(1)
		 */
		/*
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
		*/

		/*
		 * Algorithm #2
		 * 
		 * This algorithm does one pass over every block and then only goes through blocks
		 * that could potentially change their light value.
		 * 
		 * Testing has shown Algorithm #2 to be approximately twice as fast as Algorithm #1
		 * 
		 * Best Case:
		 * 
		 * Worse Case:
		 * 
		 */

		// Pick up initial set of changed blocks
		HashSet<Tuple> changed = new HashSet<Tuple>();
		for (int x = 1; x < numBlocksWide + 2 * ITERATIONS + 1; ++x)
		{
			for (int y = 1; y < numBlocksHigh + 2 * ITERATIONS + 1; ++y)
			{
				if (  blocks[x, y] == 0 && 
                    bgBlocks[x, y] == 0)
                {
					brightness[x, y] = 255;
					if (blocks[x - 1, y - 1] != 0 || bgBlocks[x - 1, y - 1] != 0) changed.Add(new Tuple(x - 1, y - 1));
                    if (blocks[x - 1, y    ] != 0 || bgBlocks[x - 1, y    ] != 0) changed.Add(new Tuple(x - 1, y    ));
                    if (blocks[x - 1, y + 1] != 0 || bgBlocks[x - 1, y + 1] != 0) changed.Add(new Tuple(x - 1, y + 1));
                    if (blocks[x    , y - 1] != 0 || bgBlocks[x    , y - 1] != 0) changed.Add(new Tuple(x    , y - 1));
                    if (blocks[x    , y + 1] != 0 || bgBlocks[x    , y + 1] != 0) changed.Add(new Tuple(x    , y + 1));
                    if (blocks[x + 1, y - 1] != 0 || bgBlocks[x + 1, y - 1] != 0) changed.Add(new Tuple(x + 1, y - 1));
                    if (blocks[x + 1, y    ] != 0 || bgBlocks[x + 1, y    ] != 0) changed.Add(new Tuple(x + 1, y    ));
                    if (blocks[x + 1, y + 1] != 0 || bgBlocks[x + 1, y + 1] != 0) changed.Add(new Tuple(x + 1, y + 1));
				}
			}
		}

        // Go through light sources
        foreach (HashSet<Tuple> chunkLight in lightData)
        {
            foreach (Tuple pos in chunkLight)
            {
                int x = pos.x + ITERATIONS + 1;
                int y = pos.y + ITERATIONS + 1;
                brightness[x, y] = 255;
                if (blocks[x - 1, y - 1] != 0 || bgBlocks[x - 1, y - 1] != 0) changed.Add(new Tuple(x - 1, y - 1));
                if (blocks[x - 1, y    ] != 0 || bgBlocks[x - 1, y    ] != 0) changed.Add(new Tuple(x - 1, y    ));
                if (blocks[x - 1, y + 1] != 0 || bgBlocks[x - 1, y + 1] != 0) changed.Add(new Tuple(x - 1, y + 1));
                if (blocks[x    , y - 1] != 0 || bgBlocks[x    , y - 1] != 0) changed.Add(new Tuple(x    , y - 1));
                if (blocks[x    , y + 1] != 0 || bgBlocks[x    , y + 1] != 0) changed.Add(new Tuple(x    , y + 1));
                if (blocks[x + 1, y - 1] != 0 || bgBlocks[x + 1, y - 1] != 0) changed.Add(new Tuple(x + 1, y - 1));
                if (blocks[x + 1, y    ] != 0 || bgBlocks[x + 1, y    ] != 0) changed.Add(new Tuple(x + 1, y    ));
                if (blocks[x + 1, y + 1] != 0 || bgBlocks[x + 1, y + 1] != 0) changed.Add(new Tuple(x + 1, y + 1));
            }
        }

		byte[,] oldBrightness = brightness;
		HashSet<Tuple> oldChanged;
		for (int i = 0; i < ITERATIONS; ++i)
		{
			oldBrightness = brightness;

			oldChanged = changed;
			changed = new HashSet<Tuple>();

			foreach (Tuple pos in oldChanged)
			{
				// Check to confirm that the position being updated is within bounds
				// It's technically faster to check before adding the position to the set
				if (pos.x <= 0 || pos.x > numBlocksWide + 2 * ITERATIONS ||
				    pos.y <= 0 || pos.y > numBlocksHigh + 2 * ITERATIONS)
				{
					continue;
				}

				// Determine our maximal light value at this point in time.
                byte lightValue;
                if (  blocks[pos.x, pos.y] == 0 &&
                    bgBlocks[pos.x, pos.y] != 0)
                {
                    lightValue = (byte)Mathf.Max(0,
                                                 oldBrightness[pos.x    , pos.y + 1] - LIGHT_UNIT,
                                                 oldBrightness[pos.x    , pos.y - 1] - LIGHT_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y    ] - LIGHT_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y - 1] - DIAGONAL_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y + 1] - DIAGONAL_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y    ] - LIGHT_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y - 1] - DIAGONAL_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y + 1] - DIAGONAL_UNIT);
                }
                else if (blocks[pos.x, pos.y] != 0)
                {
                    lightValue = (byte)Mathf.Max(0,
                                                 oldBrightness[pos.x    , pos.y + 1] - BLOCK_REDUCTION * LIGHT_UNIT,
                                                 oldBrightness[pos.x    , pos.y - 1] - BLOCK_REDUCTION * LIGHT_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y    ] - BLOCK_REDUCTION * LIGHT_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y - 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                                                 oldBrightness[pos.x + 1, pos.y + 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y    ] - BLOCK_REDUCTION * LIGHT_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y - 1] - BLOCK_REDUCTION * DIAGONAL_UNIT,
                                                 oldBrightness[pos.x - 1, pos.y + 1] - BLOCK_REDUCTION * DIAGONAL_UNIT);
                }
                else
                {
                    lightValue = 255;
                }
				brightness[pos.x, pos.y] = lightValue;

				// Check our neighbors to see if they should be updated.
				if (oldBrightness[pos.x - 1, pos.y - 1] < lightValue - DIAGONAL_UNIT * ((blocks[pos.x - 1, pos.y - 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x - 1, pos.y - 1));
                if (oldBrightness[pos.x - 1, pos.y    ] < lightValue - LIGHT_UNIT    * ((blocks[pos.x - 1, pos.y    ] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x - 1, pos.y    ));
                if (oldBrightness[pos.x - 1, pos.y + 1] < lightValue - DIAGONAL_UNIT * ((blocks[pos.x - 1, pos.y + 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x - 1, pos.y + 1));
                if (oldBrightness[pos.x    , pos.y - 1] < lightValue - LIGHT_UNIT    * ((blocks[pos.x    , pos.y - 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x    , pos.y - 1));
                if (oldBrightness[pos.x    , pos.y + 1] < lightValue - LIGHT_UNIT    * ((blocks[pos.x    , pos.y + 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x    , pos.y + 1));
                if (oldBrightness[pos.x + 1, pos.y - 1] < lightValue - DIAGONAL_UNIT * ((blocks[pos.x + 1, pos.y - 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x + 1, pos.y - 1));
                if (oldBrightness[pos.x + 1, pos.y    ] < lightValue - LIGHT_UNIT    * ((blocks[pos.x + 1, pos.y    ] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x + 1, pos.y    ));
                if (oldBrightness[pos.x + 1, pos.y + 1] < lightValue - DIAGONAL_UNIT * ((blocks[pos.x + 1, pos.y + 1] == 0) ? (byte)1 : BLOCK_REDUCTION)) changed.Add(new Tuple(pos.x + 1, pos.y + 1));
			}
		}

		/*
		 * Algorithm #3 (not implemented)
		 * 
		 * This algorighm uses a lot of memory to keep track of every light within range for
		 * every block. Then you just calculate the closest light for every relevant block.
		 * 
		 * Best Case:
		 * 
		 * Worst Case:
		 * 
		 */

		
		watch.Stop ();
		world.RecordLightingTime (watch.ElapsedMilliseconds);
		/*
		 * Get the brightness variable back into a size of (blockWidth, blockHeight)
		 */
		oldBrightness = brightness;
		
		brightness = new byte[numBlocksWide, numBlocksHigh];
		
		for (int x = 0; x < numBlocksWide; ++x)
		{
			for (int y = 0; y < numBlocksHigh; ++y)
			{
				brightness[x, y] = oldBrightness[x + ITERATIONS + 1, y + ITERATIONS + 1];
			}
		}
	}
}
