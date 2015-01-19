using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class World : MonoBehaviour
{
	public int blockWidth = 50;
	public int blockHeight = 50;
	public int chunkWidth = 1;
	public int chunkHeight = 1;
	public GameObject chunkPrefab;
	private Chunk[,] chunks;

	void Awake ()
	{
		chunks = new Chunk[chunkWidth, chunkHeight];
	}

	void Start ()
	{
		for (int x = 0; x < chunkWidth; ++x)
		{
			for (int y = 0; y < chunkHeight; ++y)
			{
				GameObject chunk = (GameObject)Instantiate (chunkPrefab);
				chunk.transform.SetParent (this.transform);
				chunks[x,y] = chunk.GetComponent<Chunk> ();
				chunks[x,y].Setup (this, blockWidth, blockHeight, x, y);
			}
		}
	}

	void Update ()
	{
	
	}

	public byte BlockAt(int cx, int cy, int bx, int by)
	{
		if (cx < 0 || cx >= chunkWidth || cy < 0 || cy >= chunkHeight)
		{
			return (byte)1;
		}
		return chunks[cx, cy].blocks[bx, by];
	}
}
