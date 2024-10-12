using System;
using UnityEngine;

public static class PoissonSampler
{
    public const int MAX_K = 10;
    
    public static int GetPoissonSampleCount(float lamada,Vector3 worldPosition)
    {
        float[] possiblitiesSum = new float[MAX_K];
        for (int i = 0; i < possiblitiesSum.Length; i++)
        {
            possiblitiesSum[i] = GetPoissonPossibility(lamada, i) + (i == 0 ? 0 : possiblitiesSum[i - 1]);
        }
        
        float randomValue = Mathf.Abs(worldPosition.GetHashCode() % 1000 / 1000f);
        
        for (int i = 0; i < possiblitiesSum.Length; i++)
        {
            if (randomValue <= possiblitiesSum[i])
            {
                return i;
            }
        }

        return possiblitiesSum.Length;
    }
    
    private static float GetPoissonPossibility(float lamada, int k)
    {
        return Mathf.Pow(lamada, k) * Mathf.Exp(-lamada) / Factorial(k);
    }

    private static int Factorial(int i)
    {
        if (i < 0)
            throw new Exception("The factorial of a negative number is not defined");
        if(i>=13)
            return Int32.MaxValue;
        int result = 1;
        for(int j = 1; j <= i; j++)
        {
            result *= j;
        }
        return result;
    }
}
