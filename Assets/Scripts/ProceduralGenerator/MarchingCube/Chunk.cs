using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Chunk : MonoBehaviour
{
    public Vector3 origin;
    public Vector3Int chunkSize;
    public Vector3 cellSize;
    
    public bool isShowVolumeGizmo = true;
    
    [Header("Debug")]
    public bool showMeshNormal = false;
    public float normalLength = 0.3f;
    
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 volumeSize;
    
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
        chunkSize = m_cellCounts;
        cellSize = m_cellSize;
        volumeSize = new Vector3((chunkSize.x) * cellSize.x, (chunkSize.y) * cellSize.y, (chunkSize.z) * cellSize.z);
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

    void ShowMeshNormal()
    {
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        for (int i = 0; i < vertices.Length; i++)
        {
            Debug.DrawLine(vertices[i], vertices[i] + normalLength * normals[i],Color.red);
        }
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
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + volumeSize/2, volumeSize);
        }
    }
}
