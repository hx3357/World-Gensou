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

    public static float universalChunkSize;
    
    public int chunkResolution;
    
    /// <summary>
    /// If a chunk is static, chunk exclusive computation will be executed constantly
    /// </summary>
    public bool isStatic = false;
    
    public bool isShowVolumeGizmo ;
    public bool isShowDotFieldGizmo = false;
    
    [Header("Debug")]
    public bool showMeshNormal = false;
    public float normalLength = 1f;
    
    private Mesh mesh;
    private Dictionary<int,Mesh> lodMeshDict = new Dictionary<int, Mesh>();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 volumeSize;
    
    #region Static Field
    public static void SetUniversalChunkSize(float cellsize)
    {
        universalChunkSize = cellsize;
    }

    public static Vector3  GetChunkOriginByCoord(Vector3Int coord)
    {
        return new Vector3(coord.x*universalChunkSize,
            coord.y*universalChunkSize,
            coord.z*universalChunkSize);
    }
    
    public static Vector3 GetChunkCenterByCoord(Vector3Int coord)
    {
        return GetChunkOriginByCoord(coord) + universalChunkSize/2*Vector3.one;
    }
    
    public static Vector3Int GetChunkCoordByPosition(Vector3 position)
    {
        return new Vector3Int(Mathf.FloorToInt(position.x/universalChunkSize),
            Mathf.FloorToInt(position.y/universalChunkSize),
            Mathf.FloorToInt(position.z/universalChunkSize));
    }

    public static Vector3 GetWorldSize()
    {
        return universalChunkSize * Vector3.one;
    }
    
    public static void DrawChunkGizmo(Vector3Int coord)
    {
        Vector3 origin = GetChunkOriginByCoord(coord);
        Vector3 volumeSize = universalChunkSize * Vector3.one;
        Gizmos.DrawWireCube(origin + volumeSize/2, volumeSize);
    }
    
    #endregion
    
    public void SetMeshAndResolution(Mesh m_mesh,int resolution)
    {
        lodMeshDict[resolution] = m_mesh;
        chunkResolution = resolution;
        mesh = m_mesh;
        if(meshFilter!=null)
            meshFilter.mesh = mesh;
    }
    
    public bool TrySetResolution(int resolution)
    {
        bool isContain = lodMeshDict.ContainsKey(resolution);
        if (isContain)
        {
            chunkResolution = resolution;
            mesh = lodMeshDict[resolution];
            if(meshFilter!=null)
                meshFilter.mesh = mesh;
        }
        return isContain;
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
    
    public void DestroyChunk()
    {
        if(zombieChunkDict.TryGetValue(origin,out Chunk chunk))
        {
            zombieChunkDict.Remove(origin);
        }
        Destroy(gameObject);
    }
    
    public void EnableChunk()
    {
        isZombie = false;
        zombieTimer = 0;
        zombieChunkDict.Remove(origin);
        ShowMesh();
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
            Gizmos.DrawWireCube(transform.position + volumeSize/2, volumeSize);
        }

        if (isShowDotFieldGizmo)
        {
            ShowDotFieldGizmo();
        }
    }
    
}
