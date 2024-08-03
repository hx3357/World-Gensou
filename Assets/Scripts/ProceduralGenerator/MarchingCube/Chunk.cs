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
    
    /// <summary>
    /// Empty: Chunk mesh is created, but no chunk exclusive computation
    /// Hidden: Chunk itself is invisible
    /// Visible: Chunk is visible
    /// </summary>
    public enum ChunkStatus
    {
        Empty,Hidden,Visible
    }

    public ChunkStatus status
    {
        get => _status;
        set
        {
            statusChangeAction?.Invoke(_status, value);
            _status = value;
        }
    }

    private ChunkStatus _status = ChunkStatus.Visible;
    
    /// <summary>
    /// Source status -> Target status
    /// </summary>
    Action<ChunkStatus,ChunkStatus> statusChangeAction;
    
    /// <summary>
    /// If a chunk is static, chunk exclusive computation will be executed constantly
    /// </summary>
    public bool isStatic = false;
    
    /// <summary>
    /// The Chunk Lod Level corresponding to downsampling rate
    /// </summary>
    public enum LODLevel
    {
        Ultra = 1,High = 2,Medium=3,Low=4,Potato=8
    }
    
    public LODLevel lodLevel = LODLevel.Ultra;
    
    public bool isShowVolumeGizmo = true;
    
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
        ProcedualGeneratorUtility.ShowDotFieldGizmo(transform.position,
            new Vector3Int(chunkSize.x+1,chunkSize.y+1,chunkSize.z+1), dotField);
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
    
    void DefaultStatusChangeAction(ChunkStatus sourceStat, ChunkStatus targetStat)
    {
        switch (sourceStat)
        {
            case ChunkStatus.Empty:
                switch (targetStat)
                {
                    case ChunkStatus.Hidden:
                        break;
                    case ChunkStatus.Visible:
                        gameObject.SetActive(true);
                        break;
                }
                break;
            case ChunkStatus.Hidden:
                switch (targetStat)
                {
                    case ChunkStatus.Empty:
                        break;
                    case ChunkStatus.Visible:
                        gameObject.SetActive(true);
                        break;
                }
                break;
            case ChunkStatus.Visible:
                switch (targetStat)
                {
                    case ChunkStatus.Empty:
                        gameObject.SetActive(false);
                        break;
                    case ChunkStatus.Hidden:
                        gameObject.SetActive(false);
                        break;
                }
                break;
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
        
        statusChangeAction += DefaultStatusChangeAction;
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
            switch (_status)
            {
                case ChunkStatus.Empty:
                    Gizmos.color = Color.cyan;
                    break;
                case ChunkStatus.Hidden:
                    Gizmos.color = Color.blue;
                    break;
                case ChunkStatus.Visible:
                    Gizmos.color = Color.magenta;
                    break;
            }
            Gizmos.DrawWireCube(transform.position + volumeSize/2, volumeSize);
        }
    }
    
}
