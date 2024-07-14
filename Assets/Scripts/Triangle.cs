using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Triangle
{
    public readonly int ID;
    public int[] vertex_indecies = new int[3] { -1, -1, -1 };
    public int[] neighbor_indecies= new int[3] { -1, -1, -1};
    private readonly TIN tin;

    public Triangle(int id, TIN tin_)
    {
        ID = id;
        this.tin = tin_;
        if (tin == null) Debug.LogError("tin not set");
    }
    public Triangle(int iD, int[] vertex_indecies, TIN tin) : this(iD,tin)
    {
        this.vertex_indecies = vertex_indecies;
    }
    public Triangle(int iD, int[] vertex_indecies_, int[] neighbor_indecies_, TIN tin) : this(iD, tin)
    {
        this.vertex_indecies = vertex_indecies_;
        this.neighbor_indecies = neighbor_indecies_;
    }
    public int this[int key]
    {
        get => vertex_indecies[key];
    }
    public Vector3 GetSide(int side_index)
    {
        //Debug.Log("Triangle " + ID + " side " + side_index + " " + ((side_index + 1) % 3) + ", " + side_index);
        return tin.vertices[vertex_indecies[(side_index + 1) % 3]] - tin.vertices[vertex_indecies[side_index]];
    }
    public Vector2 GetNormal(int side_index)
    {
        Vector3 side = GetSide(side_index);
        return new Vector2(side.y, -side.x);
    }
    public bool ContainsPoint(int point_index)
    {
        //if (tin == null) Debug.LogError("TIN ref not set for triangle " + ID);
        //else if (tin.vertices == null) Debug.LogError("TIN.vertices not set");
        Vector3 aP = tin.vertices[point_index] - tin.vertices[vertex_indecies[0]];
        Vector3 bP = tin.vertices[point_index] - tin.vertices[vertex_indecies[1]];
        Vector3 cP = tin.vertices[point_index] - tin.vertices[vertex_indecies[2]];

        float[] s = new float[3]
        {
            Vector3.Dot(aP,GetNormal(0)),
            Vector3.Dot(bP,GetNormal(1)),
            Vector3.Dot(cP,GetNormal(2))
        };
        return ((s[0] * s[1] > 0) && (s[1] * s[2] > 0));
    }
    public bool CircumcircleContainsPoint(int point_index)
    {
        Vector3 a = tin.vertices[vertex_indecies[0]];
        Vector3 b = tin.vertices[vertex_indecies[1]];
        Vector3 c = tin.vertices[vertex_indecies[2]];
        Vector3 d = tin.vertices[point_index];

        float ax_ = a.x - d.x;
        float ay_ = a.y - d.y;
        float bx_ = b.x - d.x;
        float by_ = b.y - d.y;
        float cx_ = c.x - d.x;
        float cy_ = c.y - d.y;

        return (
        (ax_ * ax_ + ay_ * ay_) * (bx_ * cy_ - cx_ * by_) -
        (bx_ * bx_ + by_ * by_) * (ax_ * cy_ - cx_ * ay_) +
        (cx_ * cx_ + cy_ * cy_) * (ax_ * by_ - bx_ * ay_)
        ) > 0;
    }

    public void SetVertexIndecies(int a, int b, int c)
    {
        vertex_indecies[0] = a;
        vertex_indecies[1] = b;
        vertex_indecies[2] = c;
    }
    public void SetNeighborIndecies(int a, int b, int c)
    {
        neighbor_indecies[0] = a;
        neighbor_indecies[1] = b;
        neighbor_indecies[2] = c;
    }
}
