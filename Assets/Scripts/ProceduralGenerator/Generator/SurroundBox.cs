using System;
using UnityEngine;

public class SurroundBox : IComparable<SurroundBox>
{
    int[] surroundBox;
    public Vector3 center;
    public Vector3 size;
    public float volume;
    
    public static readonly SurroundBox InfiniteSurroundBox = 
        new SurroundBox(int.MinValue,int.MaxValue,int.MinValue,int.MaxValue,int.MinValue,int.MaxValue);
    
    public SurroundBox(Vector3 worldPosition,Vector3 worldSize)
    {
        surroundBox = GetSurroundBox(worldPosition,worldSize);
        size = GetSize();
        center = GetCenter();
        volume = size.x * size.y * size.z;
    }
    
    public SurroundBox(int xMin,int xMax,int yMin,int yMax,int zMin,int zMax)
    {
        surroundBox = new int[]{xMin,xMax,yMin,yMax,zMin,zMax};
    }

    Vector3 GetCenter()
    {
        Vector3[] corners = new Vector3[2];
        corners[0] = Chunk.GetChunkOriginByCoord(new Vector3Int(surroundBox[0],surroundBox[2],surroundBox[4]));
        corners[1] = Chunk.GetChunkOriginByCoord(new Vector3Int(surroundBox[1],surroundBox[3],surroundBox[5])) +
                     Chunk.GetWorldSize();
        return (corners[0] + corners[1]) / 2;
    }
    
    Vector3 GetSize()
    {
        Vector3[] corners = new Vector3[2];
        corners[0] = Chunk.GetChunkOriginByCoord(new Vector3Int(surroundBox[0],surroundBox[2],surroundBox[4]));
        corners[1] = Chunk.GetChunkOriginByCoord(new Vector3Int(surroundBox[1],surroundBox[3],surroundBox[5])) +
                     Chunk.GetWorldSize();
        return corners[1] - corners[0];
    }
    
    public bool IsInSurroundBox(Vector3Int chunkCoord)
    {
        if(surroundBox.Length != 6)
            throw new System.Exception("The surround box should have 6 elements");
        return chunkCoord.x >= surroundBox[0] && chunkCoord.x <= surroundBox[1] &&
               chunkCoord.y >= surroundBox[2] && chunkCoord.y <= surroundBox[3] &&
               chunkCoord.z >= surroundBox[4] && chunkCoord.z <= surroundBox[5];
    }
    
    public bool IsInSurroundBox(Vector3 worldPosition)
    {
        Vector3Int chunkCoord = Chunk.GetChunkCoordByPosition(worldPosition);
        return IsInSurroundBox(chunkCoord);
    }

    int[] GetSurroundBox(Vector3 worldPosition,Vector3 worldSize)
    {
        Vector3 halfSize = worldSize / 2;
        Vector3Int minChunkCoord = Chunk.GetChunkCoordByPosition(worldPosition - halfSize);
        Vector3Int maxChunkCoord = Chunk.GetChunkCoordByPosition(worldPosition + halfSize);
        return new []
        {
            minChunkCoord.x,maxChunkCoord.x,
            minChunkCoord.y,maxChunkCoord.y,
            minChunkCoord.z,maxChunkCoord.z
        };
    }

    public void DrawSurroundBoxGizmo()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetCenter(),GetSize());
    }

    public int CompareTo(SurroundBox other)
    {
        if(other == null)
            return 1;
        return volume.CompareTo(other.volume);
    }
}
