﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
	private Chunk[,] chunks;

	public Dictionary<Tuple, GameObject> generatedChunks;
    public List<Tuple> generationOrder;
	public GameObject localPlayer;
	private int playerCX = -1;
	private int playerCY = -1;

	void Awake ()
	{
        generatedChunks = new Dictionary<Tuple, GameObject>();
        generationOrder = new List<Tuple>();
	}

	void Start ()
	{
		localPlayer.transform.position = new Vector3(blockWidth * chunkWidth / 2, (blockHeight + 10) * chunkHeight / 2);
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
						GameObject chunk = (GameObject)Instantiate (chunkPrefab);
						generatedChunks[pos] = chunk;
						generationOrder.Add (pos);
						chunk.name = "Chunk(" + x.ToString () + "," + y.ToString () + ")";
						chunk.transform.parent = this.transform;
						chunk.GetComponent<Chunk> ().Setup (this, blockWidth, blockHeight, x, y);
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
					if (!generatedChunks[pos].GetComponent<Chunk> ().isActive)
					{
						generatedChunks[pos].GetComponent<Chunk> ().Activate ();
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
                generatedChunks[new Tuple(cx, cy)].GetComponent<Chunk>().DestroyBlock(bx, by);
			}
		}
	}

	public byte BlockAt(int cx, int cy, int bx, int by)
	{
		if (cx < 0 || cx >= chunkWidth || cy < 0 || cy >= chunkHeight)
		{
			return (byte)1;
		}
        return generatedChunks[new Tuple(cx, cy)].GetComponent<Chunk>().blocks[bx, by];
	}
}