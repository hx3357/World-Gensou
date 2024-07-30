using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDispatcher : MonoBehaviour
{
   private const int ViewDistance = 8;
   private Transform playerTransform;
   
   private ChunkFactory chunkFactory;

   private Vector3Int chunkSize;
   
   private Dictionary<Vector3Int,Chunk> visibleChunkDict = new Dictionary<Vector3Int, Chunk>();
   
}
