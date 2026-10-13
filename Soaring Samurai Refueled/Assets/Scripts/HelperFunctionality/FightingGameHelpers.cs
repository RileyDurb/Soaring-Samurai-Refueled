using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FightingGameHelpers
{
    private const int mBaseFrameConversionFramerate = 60;

    public static float GetTimeAsFrames(float timeInSeconds, int referenceFramerate = mBaseFrameConversionFramerate)
    {
        return timeInSeconds * referenceFramerate;
    }

    public static float GetFramesAsTime(float framesToConvert, int referenceFramerate = mBaseFrameConversionFramerate)
    {
        return framesToConvert / referenceFramerate;
    }
}
