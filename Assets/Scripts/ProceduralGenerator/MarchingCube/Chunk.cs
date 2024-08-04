using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    public Vector3 origin;
    public Vector3 center;
    public Vector3Int chunkSize;
    public Vector3 cellSize;
    public Vector3Int chunkCoord;
    public Vector4[] dotField;
    public Vector3Int dotFieldSize;
    
    /// <summary>
    /// If a chunk is static, chunk exclusive computation will be executed constantly
    /// </summary>
    public bool isStatic = false;
    
    /// <summary>
    /// The Chunk Lod Level corresponding to downsampling rate
    /// </summary>
    public enum LODLevel
    {
        High,Low,Potato,Culling
    }
    
    public static readonly Dictionary<LODLevel,float> lodDownSampleRateTable = new Dictionary<LODLevel, float>()
    {
        {LODLevel.High,2},
        {LODLevel.Low,3},
        {LODLevel.Potato,4},
        {LODLevel.Culling,8}
    };
    
    /// <summary>
    /// Max view distance of each LOD level
    /// </summary>
    public static readonly List<int> lodViewDistanceTable = new List<int>()
    {
        3,5,10
    };
    
    public LODLevel lodLevel = LODLevel.High;
    
    public bool isShowVolumeGizmo ;
    public bool isShowDotFieldGizmo = false;
    
    [Header("Debug")]
    public bool showMeshNormal = false;
    public float normalLength = 0.3f;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 volumeSize;
    
    #region Static Field
    public static void SetUniversalChunkSize(Vector3Int size,Vector3 cellsize)
    {
        IChunkFactory.universalChunkSize = size;
        IChunkFactory.universalCellSize = cellsize;
    }

    public static Vector3 GetChunkOriginByCoord(Vector3Int coord)
    {
        return new Vector3(coord.x*IChunkFactory.universalChunkSize.x*IChunkFactory.universalCellSize.x,
            coord.y*IChunkFactory.universalChunkSize.y*IChunkFactory.universalCellSize.y,
            coord.z*IChunkFactory.universalChunkSize.z*IChunkFactory.universalCellSize.z);
    }
    
    public static Vector3 GetChunkCenterByCoord(Vector3Int coord)
    {
        return GetChunkOriginByCoord(coord) + new Vector3((IChunkFactory.universalChunkSize.x-1)*IChunkFactory.universalCellSize.x/2,
            (IChunkFactory.universalChunkSize.y-1)*IChunkFactory.universalCellSize.y/2,
            (IChunkFactory.universalChunkSize.z-1)*IChunkFactory.universalCellSize.z/2);
    }
    
    public static Vector3Int GetChunkCoordByPosition(Vector3 position)
    {
        return new Vector3Int(Mathf.FloorToInt(position.x/IChunkFactory.universalChunkSize.x/IChunkFactory.universalCellSize.x),
            Mathf.FloorToInt(position.y/IChunkFactory.universalChunkSize.y/IChunkFactory.universalCellSize.y),
            Mathf.FloorToInt(position.z/IChunkFactory.universalChunkSize.z/IChunkFactory.universalCellSize.z));
    }
    
    /// <summary>
    /// If chunk is too far away from player, exceeding the distance table, it will return int max value
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static LODLevel GetLODLevelByDistance(float distance)
    {
        for(int i = 0;i<lodViewDistanceTable.Count;i++)
        {
            if(distance <= lodViewDistanceTable[i])
                return (LODLevel)i;
        }
        return (LODLevel)int.MaxValue;
    }
    #endregion
    
    public void SetMesh(Mesh m_mesh)
    {
        mesh = m_mesh;
        meshFilter.mesh = mesh;
    }
    
    public void SetMaterial(Material material)
    {
        meshRenderer.material = material;
    }
    
    public void SetVolume(Vector3 m_origin,Vector3Int m_cellCounts,Vector3 m_cellSize)
    {
        origin = m_origin;
        center = origin + new Vector3((m_cellCounts.x-1)*m_cellSize.x/2,(m_cellCounts.y-1)*m_cellSize.y/2,(m_cellCounts.z-1)*m_cellSize.z/2);
        chunkSize = m_cellCounts;
        cellSize = m_cellSize;
        volumeSize = new Vector3((chunkSize.x) * cellSize.x, (chunkSize.y) * cellSize.y, (chunkSize.z) * cellSize.z);
    }

    public void ShowDotFieldGizmo()
    {
        ProcedualGeneratorUtility.ShowDotFieldGizmo(transform.position, dotFieldSize, dotField);
    }
    
    public void HideMesh()
    {
        meshRenderer.enabled = false;
    }
    
    public void ShowMesh()
    {
        meshRenderer.enabled = true;
    }

    public void DestroyChunk()
    {
        Destroy(gameObject);
    }
    
    public void SetLODLevel(LODLevel level)
    {
        lodLevel = level;
    }
    
    public void SetDotField(Vector4[] m_dotField, Vector3Int m_dotFieldSize)
    {
        dotField = m_dotField;
        dotFieldSize = m_dotFieldSize;
    }

    void ShowMeshNormal()
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.DrawLine(vertices[i], vertices[i] + normalLength * normals[i],Color.red);
        }
    }
    
    
    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
    }

    private void Update()
    {
        if(showMeshNormal)
            ShowMeshNormal();
    }

    private void OnDrawGizmos()
    {
        if(isShowVolumeGizmo)
        {
            switch (lodLevel)
            {
                case LODLevel.High:
                    Gizmos.color = Color.blue;
                    break;
                case LODLevel.Low:
                    Gizmos.color = Color.yellow;
                    break;
                case LODLevel.Potato:
                    Gizmos.color = Color.red;
                    break;
                case LODLevel.Culling:
                    Gizmos.color = Color.gray;
                    break;
                    
            }
            Gizmos.DrawWireCube(transform.position + volumeSize/2, volumeSize);
        }

        if (isShowDotFieldGizmo)
        {
            ShowDotFieldGizmo();
        }
    }
    
}
