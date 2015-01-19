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

	private float tUnit = 0.25f;
	private Vector2 tStone = new Vector2 (0, 0);
	private Vector2 tGrass = new Vector2 (0, 1);

	public List<Vector3> newVertices = new List<Vector3>();
	public List<int> newTriangles = new List<int>();
	public List<Vector2> newUV = new List<Vector2>();

	public List<Vector3> colVertices = new List<Vector3>();
	public List<int> colTriangles = new List<int>();

	private Mesh mesh;
	private MeshCollider col;
	
	private int squareCount;
	private int colCount;

	public bool update = false;

	public GameObject spotlightPrefab;

	public void Setup (World world, int bWidth, int bHeight, int cx, int cy)
	{
		this.world = world;
		this.blockWidth = bWidth;
		this.blockHeight = bHeight;
		this.chunkx = cx;
		this.chunky = cy;
		this.transform.position = new Vector3(bWidth*cx, bHeight*cy, 0);

		GenTerrain ();
	}

	void Awake ()
	{
		mesh = GetComponent<MeshFilter> ().mesh;
		col = GetComponent<MeshCollider> ();
	}

	void Start ()
	{
		//mesh = GetComponent<MeshFilter> ().mesh;
		//col = GetComponent<MeshCollider> ();

		//GenTerrain ();
		BuildMesh ();
		UpdateMesh ();
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

		Mesh newMesh = new Mesh();
		newMesh.vertices = colVertices.ToArray ();
		newMesh.triangles = colTriangles.ToArray ();
		col.sharedMesh = newMesh;

		colVertices.Clear ();
		colTriangles.Clear ();
		colCount = 0;
	}

	void GenSquare(int x, int y, Vector2 texture)
	{
		int neighborSetup = 0;
		neighborSetup += 1 * Convert.ToInt32(Block(x + 1, y) != 0);
		neighborSetup += 2 * Convert.ToInt32(Block(x, y - 1) != 0);
		neighborSetup += 4 * Convert.ToInt32(Block(x - 1, y) != 0);
		neighborSetup += 8 * Convert.ToInt32(Block(x, y + 1) != 0);

		if (neighborSetup == 1)
		{
			SetVertices (new Vector3(x + 0.5f, y + 0.25f, 0),
			             new Vector3(x + 0.5f, y + 0.75f, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x + 1, y, 0),
			             neighborSetup,
			             false);
		}
		else if (neighborSetup == 2)
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x + 0.25f, y + 0.5f, 0),
			             new Vector3(x + 0.75f, y + 0.5f, 0),
			             new Vector3(x + 1, y, 0),
			             neighborSetup,
			             false);
		}
		else if (neighborSetup == 4)
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 0.5f, y + 0.75f, 0),
			             new Vector3(x + 0.5f, y + 0.25f, 0),
			             neighborSetup,
			             false);
		}
		else if (neighborSetup == 8)
		{
			SetVertices (new Vector3(x + 0.25f, y + 0.5f, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x + 0.75f, y + 0.5f, 0),
			             neighborSetup,
			             false);
		}
		else if (neighborSetup == 3) 	// In the next few checks the fourth vertice goes unused
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x + 1, y, 0),
			             new Vector3(x, y, 0),
			             neighborSetup,
			             true);
		}
		else if (neighborSetup == 6)
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 1, y, 0),
			             new Vector3(x, y, 0),
			             neighborSetup,
			             true);
		}
		else if (neighborSetup == 9)
		{
			SetVertices (new Vector3(x + 1, y, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x, y, 0),
			             neighborSetup,
			             true);
		}
		else if (neighborSetup == 12)
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x, y, 0),
			             neighborSetup,
			             true);
		}
		else 			// Nothing fancy happening here
		{
			SetVertices (new Vector3(x, y, 0),
			             new Vector3(x, y + 1, 0),
			             new Vector3(x + 1, y + 1, 0),
			             new Vector3(x + 1, y, 0),
			             neighborSetup,
			             false);
		}

		newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y + tUnit));
		newUV.Add(new Vector2 (tUnit * texture.x + tUnit, tUnit * texture.y));
		newUV.Add(new Vector2 (tUnit * texture.x, tUnit * texture.y));
	}

	void SetVertices (Vector3 vertice0,
	                  Vector3 vertice1,
	                  Vector3 vertice2,
	                  Vector3 vertice3,
	                  int neighborSetup,
	                  bool oneTriangle)
	{
		newVertices.Add (vertice0);
		newVertices.Add (vertice1);
		newVertices.Add (vertice2);
		newVertices.Add (vertice3);

		MeshTriangles(oneTriangle);
		squareCount++;

		if (neighborSetup == 3)
		{
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 1));
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 0));
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 0));
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 1));
			
			ColliderTriangles();
			colCount++;
		}
		else if (neighborSetup == 6)
		{
			colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 1));
			colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 0));
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 0));
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 1));
			
			ColliderTriangles();
			colCount++;
		}
		else if (neighborSetup == 9)
		{
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 1));
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 0));
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 0));
			colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 1));
			
			ColliderTriangles();
			colCount++;
		}
		else if (neighborSetup == 12)
		{
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 1));
			colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 0));
			colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 0));
			colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 1));
			
			ColliderTriangles();
			colCount++;
		}
		else
		{
			//Top
			if (neighborSetup%4 < 2)
			{
				colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 1));
				colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 0));
				colVertices.Add (new Vector3 (vertice3.x, vertice3.y, 0));
				colVertices.Add (new Vector3 (vertice3.x, vertice3.y, 1));
				
				ColliderTriangles();
				colCount++;
			}

			//Bottom
			if (neighborSetup < 8)
			{
				colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 0));
				colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 1));
				colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 1));
				colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 0));
				
				ColliderTriangles();
				
				colCount++;
			}
			
			//Left
			if (neighborSetup%8 < 4)
			{
				colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 1));
				colVertices.Add (new Vector3 (vertice1.x, vertice1.y, 0));
				colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 0));
				colVertices.Add (new Vector3 (vertice0.x, vertice0.y, 1));
			
				ColliderTriangles();
				
				colCount++;
			}
			
			//Right
			if (neighborSetup%2 == 0)
			{
				colVertices.Add (new Vector3 (vertice3.x, vertice3.y, 1));
				colVertices.Add (new Vector3 (vertice3.x, vertice3.y, 0));
				colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 0));
				colVertices.Add (new Vector3 (vertice2.x, vertice2.y, 1));
				
				ColliderTriangles();
				
				colCount++;
			}
		}
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
			stone += 25;

			int dirt = Noise (truepx, 0, 100, 35, 1);
			dirt += Noise (truepx, 0, 50, 30, 1);
			dirt += 25;

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
						GenSquare (px, py, tStone);
					}
					else if (blocks[px,py] == 2)
					{
						GenSquare (px, py, tGrass);
					}
				}
			}
		}
	}

	void ColliderTriangles ()
	{
			colTriangles.Add(colCount*4);
			colTriangles.Add((colCount*4) + 1);
			colTriangles.Add((colCount*4) + 3);
			colTriangles.Add((colCount*4) + 1);
			colTriangles.Add((colCount*4) + 2);
			colTriangles.Add((colCount*4) + 3);
	}

	void MeshTriangles (bool oneTriangle)
	{
		if (oneTriangle)
		{
			newTriangles.Add(squareCount*4);
			newTriangles.Add((squareCount*4)+1);
			newTriangles.Add((squareCount*4)+2);
		}
		else
		{
			newTriangles.Add(squareCount*4);
			newTriangles.Add((squareCount*4)+1);
			newTriangles.Add((squareCount*4)+3);
			newTriangles.Add((squareCount*4)+1);
			newTriangles.Add((squareCount*4)+2);
			newTriangles.Add((squareCount*4)+3);
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
}
















