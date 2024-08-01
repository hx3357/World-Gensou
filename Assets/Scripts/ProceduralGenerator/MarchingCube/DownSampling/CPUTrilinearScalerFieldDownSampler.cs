using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUTrilinearScalerFieldDownSampler : IScalerFieldDownSampler
{
    
    public Vector4[] DownSample(Vector4[] dotField,Vector3Int m_dotFieldCount,Vector3 cellSize,float downSampleRate,
        out Vector3Int m_newDotFieldSize,out Vector3Int m_newChunkSize,out Vector3 m_newCellSize)
    {
        m_newDotFieldSize = new Vector3Int((int)(m_dotFieldCount.x / downSampleRate), 
            (int)(m_dotFieldCount.y / downSampleRate), 
            (int)(m_dotFieldCount.z / downSampleRate));
        m_newChunkSize = new Vector3Int(m_newDotFieldSize.x-1, m_newDotFieldSize.y-1, m_newDotFieldSize.z-1);
        m_newCellSize  = new Vector3((m_dotFieldCount.x-1) / (float)(m_newChunkSize.x) * cellSize.x, 
            (m_dotFieldCount.y-1) / (float)(m_newChunkSize.y) * cellSize.y, 
            (m_dotFieldCount.z-1) / (float)(m_newChunkSize.z) * cellSize.z);
        if(downSampleRate<= 1)
            return dotField;
        Vector4[] newDotField = new Vector4[m_newDotFieldSize.x * m_newDotFieldSize.y * m_newDotFieldSize.z];
        for (int x = 0; x < m_newDotFieldSize.x; x++)
        {
            for(int y = 0; y < m_newDotFieldSize.y; y++)
            {
                for(int z = 0; z < m_newDotFieldSize.z; z++)
                {
                    int index = ProcedualGeneratorUtility.GetBufferIndex(x, y, z, m_newDotFieldSize);
                    Vector3 newDotFieldPosition = new Vector3(x*m_newCellSize.x, y*m_newCellSize.y, z*m_newCellSize.z);
                    Vector3Int surroundingCubePosition = new Vector3Int((int)newDotFieldPosition.x, 
                        (int)newDotFieldPosition.y, (int)newDotFieldPosition.z);
                    Vector4[] surroundingCube = new Vector4[8];
                    for(int i = 0; i < 8; i++)
                    {
                        int xIndex = surroundingCubePosition.x + (i & 1);
                        xIndex = xIndex >= m_dotFieldCount.x ? m_dotFieldCount.x - 1 : xIndex;
                        int yIndex = surroundingCubePosition.y + ((i & 2) >> 1);
                        yIndex = yIndex >= m_dotFieldCount.y ? m_dotFieldCount.y - 1 : yIndex;
                        int zIndex = surroundingCubePosition.z + ((i & 4) >> 2);
                        zIndex = zIndex >= m_dotFieldCount.z ? m_dotFieldCount.z - 1 : zIndex;
                        surroundingCube[i] = dotField[ProcedualGeneratorUtility.GetBufferIndex(xIndex, yIndex, zIndex, m_dotFieldCount)];
                    }
                    Vector3 lerpParams =new Vector3 ((newDotFieldPosition.x - surroundingCubePosition.x) / m_newCellSize.x,
                        (newDotFieldPosition.y - surroundingCubePosition.y) / m_newCellSize.y,
                        (newDotFieldPosition.z - surroundingCubePosition.z) / m_newCellSize.z);
                    //Crush z axis
                    Vector4 p00 = Vector4.Lerp(surroundingCube[0], surroundingCube[4], lerpParams.z);
                    Vector4 p01 = Vector4.Lerp(surroundingCube[1], surroundingCube[5], lerpParams.z);
                    Vector4 p10 = Vector4.Lerp(surroundingCube[2], surroundingCube[6], lerpParams.z);
                    Vector4 p11 = Vector4.Lerp(surroundingCube[3], surroundingCube[7], lerpParams.z);
                    //Crush y axis
                    Vector4 p0 = Vector4.Lerp(p00, p10, lerpParams.y);
                    Vector4 p1 = Vector4.Lerp(p01, p11, lerpParams.y);
                    //Crush x axis
                    newDotField[index] = Vector4.Lerp(p0, p1, lerpParams.x);
                }
            }
        }
        return newDotField;
    }
}
