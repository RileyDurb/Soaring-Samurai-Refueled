using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MyRandom
{
    public static float RandomRange(float min, float max)
    {
        return Random.Range(min, max);
    }

    public static int RandomRange(int min, int max)
    {
        return Random.Range(min, max);
    }

}
