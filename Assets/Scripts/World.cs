using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct IntVector2 {
	public int x;
	public int y;

	public IntVector2 (int x, int y)
	{
		this.x = x;
		this.y = y;
	}

	int sqrMagnitude
	{
		get { return x * x + y * y; }
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

	public Dictionary<IntVector2, GameObject> generatedChunks;
	public List<IntVector2> generationOrder;
	public GameObject localPlayer;
	private int playerCX = -1;
	private int playerCY = -1;

	void Awake ()
	{
		//chunks = new Chunk[chunkWidth, chunkHeight];
		generatedChunks = new Dictionary<IntVector2, GameObject> ();
		generationOrder = new List<IntVector2> ();
	}

	void Start ()
	{
		/*
		for (int x = 0; x < chunkWidth; ++x)
		{
			for (int y = 0; y < chunkHeight; ++y)
			{
				GameObject chunk = (GameObject)Instantiate (chunkPrefab);
				chunk.transform.SetParent (this.transform);
				generatedChunks[x + cw * y] = chunk.GetComponent<Chunk> ();
				generatedChunks[x + cw * y].Setup (this, blockWidth, blockHeight, x, y);

			}
		}*/
		localPlayer.transform.position = new Vector3(blockWidth * chunkWidth / 2, (blockHeight + 10) * chunkHeight / 2);
	}

	void Update ()
	{
		int pcx = (int)(localPlayer.transform.position.x) / blockWidth;
		int pcy = (int)localPlayer.transform.position.y / blockHeight;

		if (pcx != playerCX || pcy != playerCY)
		{
			Debug.Log (pcx.ToString () + ", " + pcy.ToString ());

			// Generate terrain for all chunks in a 5x5 box around your character
			for (int x = pcx - 2; x < pcx + 3; ++x)
			{
				for (int y = pcy - 2; y < pcy + 3; ++y)
				{
					if (x < 0 || y < 0 || x >= chunkWidth || y >= chunkWidth)
					{
						continue;
					}
					IntVector2 pos = new IntVector2(x, y);
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
					IntVector2 pos = new IntVector2(x, y);
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
				for (int i = generationOrder.Count - 1; i >= 0; --i)
				{
					if (pcx + 3 < generationOrder[i].x || pcx - 3 > generationOrder[i].x ||
					    pcy + 3 < generationOrder[i].y || pcy - 3 > generationOrder[i].y)
					{
						Destroy (generatedChunks[generationOrder[i]]);
						generationOrder.RemoveAt(i);
						break;
					}
				}
			}
		}

	}

	public byte BlockAt(int cx, int cy, int bx, int by)
	{
		if (cx < 0 || cx >= chunkWidth || cy < 0 || cy >= chunkHeight)
		{
			return (byte)1;
		}
		return generatedChunks[new IntVector2(cx, cy)].GetComponent<Chunk> ().blocks[bx, by];
	}
}
