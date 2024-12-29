using System;
using UnityEngine;

namespace ChunkDispatchers.VoxelBasedDispatch
{
    public static class PoissonSampler
    {
        public const int MAX_K = 12;

        public static int GetPoissonSampleCount(float expection, Vector3 worldPosition)
        {
            var possiblitiesSum = new float[MAX_K];
            for (var i = 0; i < possiblitiesSum.Length; i++)
                possiblitiesSum[i] = GetPoissonPossibility(expection, i) + (i == 0 ? 0 : possiblitiesSum[i - 1]);

            var randomValue = Mathf.Abs(worldPosition.GetHashCode() % 1000 / 1000f);

            for (var i = 0; i < possiblitiesSum.Length; i++)
                if (randomValue <= possiblitiesSum[i])
                    return i;

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
            if (i >= 13)
                return int.MaxValue;
            var result = 1;
            for (var j = 1; j <= i; j++) result *= j;
            return result;
        }
    }
}