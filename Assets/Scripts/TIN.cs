using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class TIN : MonoBehaviour
{
    [SerializeField]
    public Vector3[] vertices;
    public List<Triangle> triangles;

    #region Vertex Methods and Variables
    [Header("Vertices")]
    public int vertexCount = 4;
    public Vector2 maxRange= Vector2.one;
    public Vector2 minRange= -Vector2.one;

    public void Start()
    {
        InitTriangulation();
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space)) { 
        ProcessNextPoint();
        }
    }

    public void GenerateNewVertices()
    {
        if (vertices == null || vertices.Length == 0) SetBoundingVertices();
        Vector3[] new_vertices= new Vector3[vertexCount+4];

        new_vertices[0] = vertices[0];
        new_vertices[1] = vertices[1];
        new_vertices[2] = vertices[2];
        new_vertices[3] = vertices[3];

        for (int vertex_index= 0; vertex_index < vertexCount; vertex_index++)
        {
            new_vertices[vertex_index+4] = new Vector2(
                Random.Range(minRange.x, maxRange.x),
                Random.Range(minRange.y, maxRange.y)
                );
        }

        vertices= new_vertices;
    }

    public void SetBoundingVertices()
    {
        Vector2 min = new Vector2(2*minRange.x-maxRange.x, 2*minRange.y-maxRange.y);
        Vector2 max = new Vector2(2*maxRange.x-minRange.x, 2*maxRange.y-minRange.y);

        Vector3 top_left = new Vector3(min.x, max.y, 0f);
        Vector3 top_right = new Vector3(max.x, max.y, 0f);
        Vector3 bottom_right = new Vector3(max.x, min.y, 0f);
        Vector3 bottom_left = new Vector3(min.x, min.y, 0f);

        if (vertices == null || vertices.Length == 0)
        {
            vertices = new Vector3[4];
        }
        vertices[0] = top_left;
        vertices[1] = top_right;
        vertices[2] = bottom_right;
        vertices[3] = bottom_left;
    }

    #endregion

    #region Triangulation
    [Space(25)]
    [Header("Triangulation")]
    public bool showMesh = false;
    private Mesh mesh;

    public void InitTriangulation()
    {
        Triangle t1 = new Triangle(0, new int[] {2,1,0}, this);
        Triangle t2 = new Triangle(1, new int[] {3,2,0}, this);

        t1.neighbor_indecies[2] = t2.ID;
        t2.neighbor_indecies[1] = t1.ID;


        triangles = new List<Triangle> { 
            t1,
            t2,
        };
        UpdateMesh();
    }
    private int[] GetTrianglesArr()
    {
        int[] triangles_arr = new int[3*triangles.Count];

        for(int triangle_index= 0; triangle_index < triangles.Count; triangle_index++)
        {
            for (int i = 0; i < 3; i++) triangles_arr[triangle_index * 3 + 2 - i] = triangles[triangle_index].vertex_indecies[i];
        }
        return triangles_arr;
    }
    private int FindTriangleWhichContains(int point_index)
    {
        for(int triangle_index= triangles.Count-1; triangle_index>=0; triangle_index--)
        {
            if (triangles[triangle_index].ContainsPoint(point_index)) return triangle_index;
        }
        Debug.LogError("Did not find triangle containing point (" + vertices[point_index].x +", " + vertices[point_index].y + ", " + vertices[point_index].z + ") ");
        return -1;
    }


    #region Insertion
    private List<int> processedTriangles = new List<int>();
    private List<int> deleteTriangles = new List<int>();
    private List<int> cavityTriangles = new List<int>() ;
    private List<int> cavityEdges = new List<int>();
    private int nextPoint = 4;

    public void ProcessNextPoint()
    {
        LocateTriangles(nextPoint);
        InsertPoint(nextPoint);
        UpdateMesh();
        CleanUp();
    }
    private void LocateTriangles(int point_index)
    {
        int firstTriangle = FindTriangleWhichContains(point_index);
        NeighboringTriangle(firstTriangle, point_index, new int[] { 0, 1, 2 });
    }
    private void NeighboringTriangle(int triangle_index, int point_index, int[] nbSequence)
    {
        deleteTriangles.Add(triangle_index);
        processedTriangles.Add(triangle_index);

        int[] neighbor_ids = triangles[triangle_index].neighbor_indecies;
        int n_id;
        int sequence;

        int k;

        // Loop through Neighbors of root triangle (triangle_index)
        for(int n=0; n < 3; n++)
        {
            sequence = nbSequence[n];
            n_id = neighbor_ids[sequence];

            if (n_id == -1) { // If Neighbor is domain boundary add root triangle and the boundaries index
                cavityTriangles.Add(triangle_index);
                cavityEdges.Add(sequence);
            }
            else if(processedTriangles.Contains(n_id)) { } // Skip if neighbor already processed
            else {
                k = System.Array.IndexOf(triangles[n_id].neighbor_indecies, triangle_index);

                if (triangles[n_id].CircumcircleContainsPoint(point_index))
                {
                    NeighboringTriangle(n_id, point_index, new int[] { (k + 1) % 3, (k + 2) % 3, (k + 3) % 3, });
                }
                else
                {
                    cavityTriangles.Add(n_id);
                    cavityEdges.Add(k);
                }
            }
        }
    }
    private void InsertPoint(int point_indx)
    {
        HashSet<int> freeIDset = new HashSet<int>(deleteTriangles);
        List<int> freeIDs = freeIDset.ToList<int>();
        freeIDs.Add(triangles.Count);
        freeIDs.Add(triangles.Count + 1);

        List<Triangle> triangleList = new List<Triangle>();

        triangles.Add(new Triangle(triangles.Count, this));
        triangles.Add(new Triangle(triangles.Count, this));

        int edgeVert1, edgeVert2;
        int[] newTriangle_vertices, newTriangle_neighbors;
        bool isBoundary;
        int cavityTriangle_ID, cavityEdge_indx;

        for (int cavity_indx = 0; cavity_indx < cavityTriangles.Count; cavity_indx++)
        {
            cavityTriangle_ID = cavityTriangles[cavity_indx];
            cavityEdge_indx = cavityEdges[cavity_indx];

            edgeVert1 = triangles[cavityTriangle_ID].vertex_indecies[cavityEdge_indx];
            edgeVert2 = triangles[cavityTriangle_ID].vertex_indecies[(cavityEdge_indx + 1) % 3];

            

            isBoundary = triangles[cavityTriangle_ID].neighbor_indecies[cavityEdge_indx] == -1;
            if (isBoundary)
            {
                // Set vertices of triangle
                newTriangle_vertices = new int[] {
                    edgeVert1,
                    edgeVert2,
                    point_indx
                };

                // Set neighbors of triangle
                newTriangle_neighbors = new int[] {
                    -1,
                    freeIDs[(cavity_indx + 1) % cavityTriangles.Count],
                    freeIDs[(cavity_indx + cavityTriangles.Count - 1) % cavityTriangles.Count]
                };
            }
            else
            {
                // Set new neighbor for cavity triangle
                triangles[cavityTriangle_ID].neighbor_indecies[cavityEdge_indx] = freeIDs[cavity_indx];

                // Set vertices of triangle
                newTriangle_vertices = new int[] {
                    edgeVert1,
                    point_indx,
                    edgeVert2
                };

                // Set neighbors of triangle
                newTriangle_neighbors = new int[] {
                    freeIDs[(cavity_indx + 1) % cavityTriangles.Count],
                    freeIDs[(cavity_indx + cavityTriangles.Count - 1) % cavityTriangles.Count],
                    cavityTriangle_ID
                };
            }
            triangleList.Add(new Triangle(freeIDs[cavity_indx], newTriangle_vertices, newTriangle_neighbors, this));
        }

        foreach(Triangle newTri in triangleList)
        {
            triangles[newTri.ID] = newTri;
        }
    }
    private void CleanUp()
    {
        processedTriangles = new List<int>();
        deleteTriangles = new List<int>();
        cavityTriangles = new List<int>();
        cavityEdges = new List<int>();
        nextPoint++;
    }

    #endregion
    private void UpdateMesh()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices;
        mesh.triangles = GetTrianglesArr();
        mesh.RecalculateNormals();
    }
    #endregion


    private void OnValidate()
    {
        StartCoroutine(ToggleMeshRenderer(showMesh));
    }
    IEnumerator ToggleMeshRenderer(bool state)
    {
        yield return null;

        GetComponent<MeshRenderer>().enabled = state;
    }

    private void OnDrawGizmos()
    {
        if (vertices == null) return;

        foreach(Vector3 v in vertices)
        {
            Gizmos.DrawSphere(v, 0.2f);
        }
    }
}
