using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct Tuple
{
    public int x;
    public int y;

    public Tuple(int first, int second)
    {
        x = first;
        y = second;
    }
}

public class World : MonoBehaviour
{
	public const int MAX_LOADED_CHUNKS = 50;

	public int blockWidth = 50;
	public int blockHeight = 50;
	public int chunkWidth = 1;
    public int chunkHeight = 1;
    public GameObject chunkPrefab;
    public GameObject lightmapPrefab;

	public Dictionary<Tuple, GameObject> generatedChunks = new Dictionary<Tuple, GameObject>();
    public Dictionary<Tuple, GameObject> generatedLightmaps = new Dictionary<Tuple, GameObject>();
    public List<Tuple> generationOrder = new List<Tuple>();
	public GameObject localPlayer;
	private int playerCX = -1;
	private int playerCY = -1;

    private byte[,] emptyChunk;

	private long lightingPassesDone = 0;
	private long lightingTimeTaken = 0;
    
	void Start ()
	{
        emptyChunk = new byte[blockWidth, blockHeight];
        for (int x = 0; x < blockWidth; ++x)
        {
            for (int y = 0; y < blockHeight; ++y)
            {
                emptyChunk[x, y] = 0;
            }
        }
		//localPlayer.transform.position = new Vector3(blockWidth * chunkWidth / 2, (blockHeight + 10) * chunkHeight / 2);
	}

	void Update ()
	{
		int pcx = (int)(localPlayer.transform.position.x) / blockWidth;
		int pcy = (int)localPlayer.transform.position.y / blockHeight;

		if (pcx != playerCX || pcy != playerCY)
		{
			// Generate terrain for all chunks in a 5x5 box around your character
			for (int x = pcx - 2; x < pcx + 3; ++x)
			{
				for (int y = pcy - 2; y < pcy + 3; ++y)
				{
					if (x < 0 || y < 0 || x >= chunkWidth || y >= chunkWidth)
					{
						continue;
					}
                    Tuple pos = new Tuple(x, y);
					if (!generatedChunks.ContainsKey(pos))
					{
						GameObject blockmap = (GameObject)Instantiate (chunkPrefab);
						generatedChunks[pos] = blockmap;
						generationOrder.Add (pos);
						blockmap.name = "Blockmap(" + x.ToString () + "," + y.ToString () + ")";
						blockmap.transform.parent = this.transform;
						blockmap.GetComponent<Blockmap> ().Setup (this, blockWidth, blockHeight, x, y);

                        GameObject lightmap = (GameObject)Instantiate(lightmapPrefab);
                        generatedLightmaps[pos] = lightmap;
                        lightmap.name = "Lightmap(" + x.ToString() + "," + y.ToString() + ")";
                        lightmap.transform.parent = this.transform;
                        lightmap.GetComponent<Lightmap>().Setup(this, blockWidth, blockHeight, x, y);
					}
				}
			}

			// Create meshes and collision meshes in a 3x3 box around your character
			for (int x = pcx - 1; x < pcx + 2; ++x)
			{
				for (int y = pcy - 1; y < pcy + 2; ++y)
				{
					if (x < 0 || y < 0 || x >= chunkWidth || y >= chunkWidth)
					{
						continue;
					}
                    Tuple pos = new Tuple(x, y);

                    Lightmap lmap = generatedLightmaps[pos].GetComponent<Lightmap>();
                    Blockmap bmap = generatedChunks[pos].GetComponent<Blockmap>();
                    if (!lmap.isActive)
                    {
                        lmap.Activate(bmap);
                    }
				}
			}
			playerCX = pcx;
			playerCY = pcy;
            
			while (generationOrder.Count >= MAX_LOADED_CHUNKS)
			{
                for (int i = 0; i < generationOrder.Count; ++i)
				{
                    if (pcx + 3 < generationOrder[i].x || pcx - 3 > generationOrder[i].x ||
                        pcy + 3 < generationOrder[i].y || pcy - 3 > generationOrder[i].y)
                    {
                        DestroyObject(generatedChunks[generationOrder[i]]);
                        generatedChunks.Remove(generationOrder[i]);

                        DestroyObject(generatedLightmaps[generationOrder[i]]);
                        generatedLightmaps.Remove(generationOrder[i]);

                        generationOrder.RemoveAt(i);
						break;
					}
				}
			}
		}
		if (Input.GetMouseButtonDown (1))
		{
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int blockHoverX = Mathf.RoundToInt(mousePos.x - 0.5f);
			int blockHoverY = Mathf.RoundToInt(mousePos.y - 0.5f);
			int cx = blockHoverX / blockWidth;
			int cy = blockHoverY / blockHeight;
			int bx = blockHoverX % blockWidth;
			int by = blockHoverY % blockHeight;
			if (BlockAt (cx, cy, bx, by) != 0)
		    {
                generatedChunks[new Tuple(cx, cy)].GetComponent<Blockmap>().DestroyBlock(bx, by);
			}
		}
	}

	public byte BlockAt(int cx, int cy, int bx, int by)
	{
		if (cx < 0 || cx >= chunkWidth || cy < 0 || cy >= chunkHeight)
		{
			return (byte)1;
		}
        return generatedChunks[new Tuple(cx, cy)].GetComponent<Blockmap>().blocks[bx, by];
	}

    public byte BlockAt(int x, int y)
    {
        return BlockAt(x / blockWidth, y / blockHeight, x % blockWidth, y % blockHeight);
    }

    public byte[,] ChunkAt(int cx, int cy)
    {
        if (cx < 0 || cx >= chunkWidth || cy < 0 || cy >= chunkHeight)
        {
            return emptyChunk;
        }
        return generatedChunks[new Tuple(cx, cy)].GetComponent<Blockmap>().blocks;
    }

	public void RecordLightingTime(long time)
	{
		lightingTimeTaken += time;
		lightingPassesDone += 1;
		//Debug.Log ("LIGHTING: Last: " +(((float)time) / 1000f).ToString("n3") + ", Average: " + ((float)(lightingTimeTaken / lightingPassesDone) / 1000f).ToString("n3"));
	}
}
