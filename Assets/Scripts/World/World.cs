using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class World : MonoBehaviour
{
    public const int MAX_LOADED_CHUNKS = 50;
    public static bool DEBUG = false;
    
	[NonSerialized] public int numBlocksWide = 50;
    [NonSerialized] public int numBlocksHigh = 40;
    [NonSerialized] public int numChunksWide = 30;
    [NonSerialized] public int numChunksHigh = 10;
    [NonSerialized] public GameObject localPlayer;

    private GameObject blockmapPrefab;
    private GameObject lightmapPrefab;
    private GameObject backmapPrefab;

    private Dictionary<Tuple, GameObject> generatedBlockmaps = new Dictionary<Tuple, GameObject>();
    private Dictionary<Tuple, GameObject> generatedLightmaps = new Dictionary<Tuple, GameObject>();
    private Dictionary<Tuple, GameObject> generatedBackmaps = new Dictionary<Tuple, GameObject>();
    private Dictionary<Tuple, Blockmap> blockmaps = new Dictionary<Tuple, Blockmap>();
    private Dictionary<Tuple, Lightmap> lightmaps = new Dictionary<Tuple, Lightmap>();
    private Dictionary<Tuple, Backmap> backmaps = new Dictionary<Tuple, Backmap>();
    private Dictionary<Tuple, Dictionary<Tuple, int>> damage = new Dictionary<Tuple, Dictionary<Tuple, int>>();
    private List<Tuple> generationOrder = new List<Tuple>();
	private int playerCX = -1;
	private int playerCY = -1;

    private byte[,] fullChunk;
    private HashSet<Tuple> emptyHashSet = new HashSet<Tuple>();

	private long lightingPassesDone = 0;
	private long lightingTimeTaken = 0;

    private HashSet<Tuple> refreshLightQueue = new HashSet<Tuple>();

    void Awake()
    {
        blockmapPrefab = Resources.Load("Prefabs/Blockmap") as GameObject;
        lightmapPrefab = Resources.Load("Prefabs/Lightmap") as GameObject;
        backmapPrefab = Resources.Load("Prefabs/Backmap") as GameObject;
    }

	void Start()
	{
        fullChunk = new byte[numBlocksWide, numBlocksHigh];
        for (int x = 0; x < numBlocksWide; ++x)
        {
            for (int y = 0; y < numBlocksHigh; ++y)
            {
                fullChunk[x, y] = 1;
            }
        }
	}

	void Update()
	{
		int pcx = (int)localPlayer.transform.position.x / numBlocksWide;
		int pcy = (int)localPlayer.transform.position.y / numBlocksHigh;

		if (pcx != playerCX || pcy != playerCY)
		{
			// Generate terrain for all chunks in a 5x5 box around your character
			for (int x = pcx - 2; x < pcx + 3; ++x)
			{
				for (int y = pcy - 2; y < pcy + 3; ++y)
				{
					if (x < 0 || y < 0 || x >= numChunksWide || y >= numChunksWide)
					{
						continue;
					}
                    Tuple pos = new Tuple(x, y);
					if (!generatedBlockmaps.ContainsKey(pos))
					{
                        GameObject blockmap = (GameObject)Instantiate(blockmapPrefab);
                        generatedBlockmaps[pos] = blockmap;
                        blockmaps[pos] = blockmap.GetComponent<Blockmap>();
                        generationOrder.Add(pos);
                        blockmap.name = "Blockmap(" + x.ToString() + "," + y.ToString() + ")";
                        blockmap.transform.parent = this.transform;
                        blockmap.GetComponent<Blockmap>().Setup(this, numBlocksWide, numBlocksHigh, x, y);
                        
                        GameObject backmap = (GameObject)Instantiate(backmapPrefab);
                        generatedBackmaps[pos] = backmap;
                        backmaps[pos] = backmap.GetComponent<Backmap>();
                        backmap.name = "Backmap(" + x.ToString() + "," + y.ToString() + ")";
                        backmap.transform.parent = this.transform;
                        backmap.GetComponent<Backmap>().Setup(this, numBlocksWide, numBlocksHigh, x, y);

                        GameObject lightmap = (GameObject)Instantiate(lightmapPrefab);
                        generatedLightmaps[pos] = lightmap;
                        lightmaps[pos] = lightmap.GetComponent<Lightmap>();
                        lightmap.name = "Lightmap(" + x.ToString() + "," + y.ToString() + ")";
                        lightmap.transform.parent = this.transform;
                        lightmap.GetComponent<Lightmap>().Setup(this, numBlocksWide, numBlocksHigh, x, y);
					}
				}
			}

			// Create meshes in a 3x3 box around your character
			for (int x = pcx - 1; x < pcx + 2; ++x)
			{
				for (int y = pcy - 1; y < pcy + 2; ++y)
				{
					if (x < 0 || y < 0 || x >= numChunksWide || y >= numChunksWide)
					{
						continue;
					}
                    Tuple pos = new Tuple(x, y);

                    if (!lightmaps[pos].isActive)
                    {
                        lightmaps[pos].Activate(blockmaps[pos], backmaps[pos]);
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
                        DestroyObject(generatedBlockmaps[generationOrder[i]]);
                        generatedBlockmaps.Remove(generationOrder[i]);
                        blockmaps.Remove(generationOrder[i]);

                        DestroyObject(generatedBackmaps[generationOrder[i]]);
                        generatedBackmaps.Remove(generationOrder[i]);
                        backmaps.Remove(generationOrder[i]);

                        DestroyObject(generatedLightmaps[generationOrder[i]]);
                        generatedLightmaps.Remove(generationOrder[i]);
                        lightmaps.Remove(generationOrder[i]);

                        generationOrder.RemoveAt(i);
						break;
					}
				}
			}
		}
		if (Input.GetMouseButtonDown(1))
        {
			Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			int blockHoverX = Mathf.RoundToInt(mousePos.x - 0.5f);
			int blockHoverY = Mathf.RoundToInt(mousePos.y - 0.5f);
			int cx = blockHoverX / numBlocksWide;
			int cy = blockHoverY / numBlocksHigh;
			int bx = blockHoverX % numBlocksWide;
			int by = blockHoverY % numBlocksHigh;
			if (BlockAt (cx, cy, bx, by) != 0)
		    {
                blockmaps[new Tuple(cx, cy)].DestroyBlock(bx, by);
			}
		}

        if (refreshLightQueue.Count != 0)
        {
            RefreshLight();
        }
	}

	public byte BlockAt(int cx, int cy, int bx, int by)
	{
		if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
		{
			return (byte)1;
		}
        return blockmaps[new Tuple(cx, cy)].blocks[bx, by];
	}

    public byte BlockAt(int x, int y)
    {
        return BlockAt(x / numBlocksWide, y / numBlocksHigh, x % numBlocksWide, y % numBlocksHigh);
    }

    public byte[,] BlockmapAt(int cx, int cy)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return fullChunk;
        }
        return blockmaps[new Tuple(cx, cy)].blocks;
    }

    public byte BackAt(int cx, int cy, int bx, int by)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return (byte)1;
        }
        return backmaps[new Tuple(cx, cy)].bgBlocks[bx, by];
    }

    public byte BackAt(int x, int y)
    {
        return BackAt(x / numBlocksWide, y / numBlocksHigh, x % numBlocksWide, y % numBlocksHigh);
    }

    public byte[,] BackmapAt(int cx, int cy)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return fullChunk;
        }
        return backmaps[new Tuple(cx, cy)].bgBlocks;
    }

    public HashSet<Tuple> LightSourcesAt(int cx, int cy)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return emptyHashSet;
        }
        Tuple pos = new Tuple(cx, cy);
        if (generatedLightmaps.ContainsKey(pos))
        {
            return lightmaps[pos].lightSources;
        }
        return null;
    }

    public bool LightSouceAt(int cx, int cy, int bx, int by)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return false;
        }
        return lightmaps[new Tuple(cx, cy)].lightSources.Contains(new Tuple(bx, by));
    }

    public bool LightSouceAt(int x, int y)
    {
        return lightmaps[new Tuple(x / numBlocksWide, y / numBlocksHigh)].lightSources.Contains(new Tuple(x % numBlocksWide, y % numBlocksHigh));
    }

    public float BrightnessAt(int cx, int cy, int bx, int by)
    {
        if (cx < 0 || cx >= numChunksWide || cy < 0 || cy >= numChunksHigh)
        {
            return 0;
        }
        return lightmaps[new Tuple(cx, cy)].brightness[bx, by];
    }

    public float BrightnessAt(int x, int y)
    {
        
        if (x < 0 || x >= numChunksWide * numBlocksWide || y < 0 || y >= numChunksHigh * numBlocksHigh)
        {
            return 0;
        }
        return lightmaps[new Tuple(x / numBlocksWide, y / numBlocksHigh)].brightness[x % numBlocksWide, y % numBlocksHigh];
    }

    public void SetBrightnessAt(float brightness, int x, int y)
    {
        Tuple chunkAt = new Tuple(x / numBlocksWide, y / numBlocksHigh);
        lightmaps[chunkAt].Relight(brightness, x % numBlocksWide, y % numBlocksHigh);
        blockmaps[chunkAt].Redraw(brightness, x % numBlocksWide, y % numBlocksHigh);
        backmaps[chunkAt].Redraw(brightness, BlockAt(x, y), x % numBlocksWide, y % numBlocksHigh);
    }

    public void DamageBlock(int x, int y, int dmg)
    {
        Tuple c = new Tuple(x / numBlocksWide, y / numBlocksHigh);
        Tuple b = new Tuple(x % numBlocksWide, y % numBlocksHigh);

        if (damage.ContainsKey(c))
        {
            if (damage[c].ContainsKey(b))
            {
                damage[c][b] -= dmg;
            }
            else
            {
                // TODO: Implement different health for different block types
                damage[c].Add(b, 100 - dmg);
            }
        }
        else
        {
            damage.Add(c, new Dictionary<Tuple, int>());
            damage[c].Add(b, 100 - dmg);
        }

        if (damage[c][b] <= 0)
        {
            blockmaps[new Tuple(c.x, c.y)].DestroyBlock(b.x, b.y);
            RefreshBlock(c.x, c.y, b.x, b.y);
        }
    }

	public void RecordLightingTime(long time)
	{
		lightingTimeTaken += time;
		lightingPassesDone += 1;
		Log("Lighting: Last: " +(((float)time) / 1000f).ToString("n3") + ", Average: " + ((float)(lightingTimeTaken / lightingPassesDone) / 1000f).ToString("n3"));
	}

    public void RefreshBlock(int cx, int cy, int bx, int by)
    {
        refreshLightQueue.Add(new Tuple(bx + cx * numBlocksWide, by + cy * numBlocksHigh));
    }

    // This needs much more optimizing....
    public void RefreshLight()
    {
        //var watch = Stopwatch.StartNew();

        Dictionary<Tuple, float> modList = new Dictionary<Tuple, float>();

        for (int i = 0; i < Lightmap.ITERATIONS; ++i)
        {
            //long before = watch.ElapsedMilliseconds;
            //long after;

            HashSet<Tuple> oldChecklist = refreshLightQueue;
            refreshLightQueue = new HashSet<Tuple>();

            foreach (Tuple pos in oldChecklist)
            {
                // Determine our maximal light value at this point in time.
                float lightValue = 0;

                Tuple t = new Tuple(pos.x / numBlocksWide, pos.y / numBlocksHigh);
                Tuple t1 = new Tuple((pos.x - 1) / numBlocksWide, (pos.y - 1) / numBlocksHigh);
                Tuple t2 = new Tuple((pos.x    ) / numBlocksWide, (pos.y - 1) / numBlocksHigh);
                Tuple t3 = new Tuple((pos.x + 1) / numBlocksWide, (pos.y - 1) / numBlocksHigh);
                Tuple t4 = new Tuple((pos.x - 1) / numBlocksWide, (pos.y    ) / numBlocksHigh);
                Tuple t5 = new Tuple((pos.x + 1) / numBlocksWide, (pos.y    ) / numBlocksHigh);
                Tuple t6 = new Tuple((pos.x - 1) / numBlocksWide, (pos.y + 1) / numBlocksHigh);
                Tuple t7 = new Tuple((pos.x    ) / numBlocksWide, (pos.y + 1) / numBlocksHigh);
                Tuple t8 = new Tuple((pos.x + 1) / numBlocksWide, (pos.y + 1) / numBlocksHigh);
                float l = lightmaps[t].brightness[pos.x % numBlocksWide, pos.y % numBlocksHigh];
                float l1 = lightmaps[t1].brightness[(pos.x - 1) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                float l2 = lightmaps[t2].brightness[(pos.x    ) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                float l3 = lightmaps[t3].brightness[(pos.x + 1) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                float l4 = lightmaps[t4].brightness[(pos.x - 1) % numBlocksWide, (pos.y    ) % numBlocksHigh];
                float l5 = lightmaps[t5].brightness[(pos.x + 1) % numBlocksWide, (pos.y    ) % numBlocksHigh];
                float l6 = lightmaps[t6].brightness[(pos.x - 1) % numBlocksWide, (pos.y + 1) % numBlocksHigh];
                float l7 = lightmaps[t7].brightness[(pos.x    ) % numBlocksWide, (pos.y + 1) % numBlocksHigh];
                float l8 = lightmaps[t8].brightness[(pos.x + 1) % numBlocksWide, (pos.y + 1) % numBlocksHigh];
                byte b = blockmaps[t].blocks[pos.x % numBlocksWide, pos.y % numBlocksHigh];
                byte b1 = blockmaps[t1].blocks[(pos.x - 1) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                byte b2 = blockmaps[t2].blocks[(pos.x    ) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                byte b3 = blockmaps[t3].blocks[(pos.x + 1) % numBlocksWide, (pos.y - 1) % numBlocksHigh];
                byte b4 = blockmaps[t4].blocks[(pos.x - 1) % numBlocksWide, (pos.y    ) % numBlocksHigh];
                byte b5 = blockmaps[t5].blocks[(pos.x + 1) % numBlocksWide, (pos.y    ) % numBlocksHigh];
                byte b6 = blockmaps[t6].blocks[(pos.x - 1) % numBlocksWide, (pos.y + 1) % numBlocksHigh];
                byte b7 = blockmaps[t7].blocks[(pos.x    ) % numBlocksWide, (pos.y + 1) % numBlocksHigh];
                byte b8 = blockmaps[t8].blocks[(pos.x + 1) % numBlocksWide, (pos.y + 1) % numBlocksHigh];

                if ((b == 0 && BackAt(pos.x, pos.y) == 0) || LightSouceAt(pos.x, pos.y))
                {
                    lightValue = 255f;
                }
                else if (b == 0)
                {
                    lightValue = Mathf.Max(0f,
                                           l1 - Lightmap.DIAGONAL_UNIT,
                                           l2 - Lightmap.LIGHT_UNIT,
                                           l3 - Lightmap.DIAGONAL_UNIT,
                                           l4 - Lightmap.LIGHT_UNIT,
                                           l5 - Lightmap.LIGHT_UNIT,
                                           l6 - Lightmap.DIAGONAL_UNIT,
                                           l7 - Lightmap.LIGHT_UNIT,
                                           l8 - Lightmap.DIAGONAL_UNIT);
                }
                else
                {
                    float adjacent = Lightmap.BLOCK_REDUCTION * Lightmap.LIGHT_UNIT;
                    float diagonal = Lightmap.BLOCK_REDUCTION * Lightmap.DIAGONAL_UNIT;
                    lightValue = Mathf.Max(0f,
                                           l1 - diagonal,
                                           l2 - adjacent,
                                           l3 - diagonal,
                                           l4 - adjacent,
                                           l5 - adjacent,
                                           l6 - diagonal,
                                           l7 - adjacent,
                                           l8 - diagonal);
                }

                float diag = lightValue - Lightmap.DIAGONAL_UNIT;
                float horz = lightValue - Lightmap.LIGHT_UNIT;
                float bdiag = lightValue - Lightmap.BLOCK_REDUCTION * Lightmap.DIAGONAL_UNIT;
                float bhorz = lightValue - Lightmap.BLOCK_REDUCTION * Lightmap.LIGHT_UNIT;
                if (l < lightValue)
                {
                    lightmaps[t].brightness[pos.x % numBlocksWide, pos.y % numBlocksHigh] = lightValue;
                    if (modList.ContainsKey(new Tuple(pos.x, pos.y)))
                    {
                        modList[new Tuple(pos.x, pos.y)] = lightValue;
                    }
                    else
                    {
                        modList.Add(new Tuple(pos.x, pos.y), lightValue);
                    }
                    if (l1 < (b1 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y - 1));
                    if (l2 < (b2 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x    , pos.y - 1));
                    if (l3 < (b3 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y - 1));
                    if (l4 < (b4 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y    ));
                    if (l5 < (b5 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y    ));
                    if (l6 < (b6 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y + 1));
                    if (l7 < (b7 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x    , pos.y + 1));
                    if (l8 < (b8 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y + 1));
                }
                else if (l > lightValue)
                {
                    lightmaps[t].brightness[pos.x % numBlocksWide, pos.y % numBlocksHigh] = lightValue;
                    if (modList.ContainsKey(new Tuple(pos.x, pos.y)))
                    {
                        modList[new Tuple(pos.x, pos.y)] = lightValue;
                    }
                    else
                    {
                        modList.Add(new Tuple(pos.x, pos.y), lightValue);
                    }
                    if (l1 > (b1 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y - 1));
                    if (l2 > (b2 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x    , pos.y - 1));
                    if (l3 > (b3 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y - 1));
                    if (l4 > (b4 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y    ));
                    if (l5 > (b5 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y    ));
                    if (l6 > (b6 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x - 1, pos.y + 1));
                    if (l7 > (b7 == 0 ? horz : bhorz)) refreshLightQueue.Add(new Tuple(pos.x    , pos.y + 1));
                    if (l8 > (b8 == 0 ? diag : bdiag)) refreshLightQueue.Add(new Tuple(pos.x + 1, pos.y + 1));
                }
            }
            //after = watch.ElapsedMilliseconds;
            //UnityEngine.Debug.Log(oldChecklist.Count + " : " + (after - before).ToString());
        }
        //watch.Stop();
        //UnityEngine.Debug.Log("Total time taken: " + watch.ElapsedMilliseconds);
        refreshLightQueue.Clear();

        foreach (Tuple pos in modList.Keys)
        {
            SetBrightnessAt(modList[pos], pos.x, pos.y);
        }
    }

    public static void Log(string message)
    {
        if (DEBUG)
        {
            UnityEngine.Debug.Log("World: " + message);
        }
    }
}
