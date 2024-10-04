using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    public readonly static float ChunkDestroyTime = 5;
    public static Dictionary<Vector3,Chunk> zombieChunkDict = new Dictionary<Vector3, Chunk>();
    
    private bool isZombie = false;
    private float zombieTimer = 0;
    
    public Vector3 origin;
    public Vector3 center;
    public Vector3Int chunkSize;
    public Vector3 cellSize;
    public Vector3Int chunkCoord;
    public Vector4[] dotField;
    public Vector3Int dotFieldSize;
    
    public static Vector3Int universalChunkSize;
    public static Vector3 universalCellSize;
    
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
    public float normalLength = 1f;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 volumeSize;
    
    #region Static Field
    public static void SetUniversalChunkSize(Vector3Int size,Vector3 cellsize)
    {
        universalChunkSize = size;
       universalCellSize = cellsize;
    }

    public static Vector3  GetChunkOriginByCoord(Vector3Int coord)
    {
        return new Vector3(coord.x*universalChunkSize.x*universalCellSize.x,
            coord.y*universalChunkSize.y*universalCellSize.y,
            coord.z*universalChunkSize.z*universalCellSize.z);
    }
    
    public static Vector3 GetChunkCenterByCoord(Vector3Int coord)
    {
        return GetChunkOriginByCoord(coord) + new Vector3((universalChunkSize.x-1)*universalCellSize.x/2,
            (universalChunkSize.y-1)*universalCellSize.y/2,
            (universalChunkSize.z-1)*universalCellSize.z/2);
    }
    
    public static Vector3Int GetChunkCoordByPosition(Vector3 position)
    {
        return new Vector3Int(Mathf.FloorToInt(position.x/universalChunkSize.x/universalCellSize.x),
            Mathf.FloorToInt(position.y/universalChunkSize.y/universalCellSize.y),
            Mathf.FloorToInt(position.z/universalChunkSize.z/universalCellSize.z));
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Cell size then chunk size</returns>
    public static (Vector3, Vector3Int) GetCellAndChunkSize()
    {
        return (universalCellSize,universalChunkSize);
    }

    public static Vector3 GetWorldSize()
    {
        return new Vector3(universalChunkSize.x * universalCellSize.x, 
            universalChunkSize.y * universalCellSize.y, 
            universalChunkSize.z * universalCellSize.z);
    }
    
    #endregion
    
    public void SetMesh(Mesh m_mesh)
    {
        mesh = m_mesh;
        if(meshFilter!=null)
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
        if(meshFilter!=null)
            meshRenderer.enabled = false;
    }
    
    public void ShowMesh()
    {
        if(meshFilter!=null)
            meshRenderer.enabled = true;
    }

    public void ClearChunk()
    {
        isZombie = true;
        zombieChunkDict[origin] = this;
        HideMesh();
    }
    
    public void EnableChunk()
    {
        isZombie = false;
        zombieTimer = 0;
        zombieChunkDict.Remove(origin);
        ShowMesh();
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
            var position = transform.position;
            Debug.DrawLine(vertices[i]+position, vertices[i] +position+ normalLength * normals[i],Color.red);
        }
    }
    
    void DestoryChunk()
    {
        if(zombieChunkDict.TryGetValue(origin,out Chunk chunk))
        {
            zombieChunkDict.Remove(origin);
        }
        Destroy(gameObject);
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

        if (isZombie)
        {
            zombieTimer += Time.deltaTime;
            if (zombieTimer >= ChunkDestroyTime)
            {
                DestoryChunk();
            }
        }
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
