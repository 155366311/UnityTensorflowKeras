﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Accord.Math;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

public static class MathUtils
{


    public static float[] GenerateWhiteNoise(int size, float min, float max)
    {
        if (size <= 0)
            return null;
        float[] result = new float[size];
        for (int i = 0; i < size; ++i)
        {
            result[i] = UnityEngine.Random.Range(min, max);
        }
        return result;
    }

    public static Array GenerateWhiteNoise(int size, float min, float max, int[] noiseDimension = null)
    {
        if (size <= 0)
            return null;
        if (noiseDimension == null || noiseDimension.Length == 0)
            return GenerateWhiteNoise(size, min, max);

        int totalSize = size * noiseDimension.Aggregate((t1, t2) => t1 * t2);

        Debug.Assert(totalSize > 0, "Invalid noiseDimension.");

        float[] result = new float[totalSize];

        for (int i = 0; i < totalSize; ++i)
        {
            result[i] = UnityEngine.Random.Range(min, max);
        }

        Array reshapedResult = Array.CreateInstance(typeof(float), new int[]{ size}.Concatenate( noiseDimension));

        Buffer.BlockCopy(result, 0, reshapedResult, 0, totalSize * sizeof(float));

        return reshapedResult;
    }

    public static float NextGaussianFloat()
    {
        float u, v, S;

        do
        {
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            S = u * u + v * v;
        }
        while (S >= 1.0);

        float fac = Mathf.Sqrt(-2.0f * Mathf.Log(S) / S);
        return u * fac;
    }




    public enum InterpolateMethod
    {
        Linear,
        Log
    }

    /// <summary>
    /// interpolate between x1 and x2 to ty suing the interpolate method
    /// </summary>
    /// <param name="method"></param>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static float Interpolate(float x1, float x2, float t, InterpolateMethod method = InterpolateMethod.Linear)
    {
        if (method == InterpolateMethod.Linear)
        {
            return Mathf.Lerp(x1, x2, t);
        }
        else
        {
            return Mathf.Pow(x1, 1 - t) * Mathf.Pow(x2, t);
        }
    }

    /// <summary>
    /// Return a index randomly. The probability if a index depends on the value in that list
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static int IndexByChance(IList<float> list)
    {
        float total = 0;

        foreach (var v in list)
        {
            total += v;
        }
        Debug.Assert(total > 0);

        float current = 0;
        float point = UnityEngine.Random.Range(0, total);

        for (int i = 0; i < list.Count; ++i)
        {
            current += list[i];
            if (current >= point)
            {
                return i;
            }
        }
        return 0;
    }
    /// <summary>
    /// return the index of the max value in the list
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static int IndexMax(IList<float> list)
    {
        int result = 0;
        float max = Mathf.NegativeInfinity;
        for (int i = 0; i < list.Count; ++i)
        {
            if (max < list[i])
            {
                result = i;
            }
        }
        return result;
    }

    /// <summary>
    /// Shuffle a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="rnd"></param>
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static System.Random Local;

        public static System.Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
