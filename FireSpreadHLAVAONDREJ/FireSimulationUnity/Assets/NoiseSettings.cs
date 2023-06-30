using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoiseSettings
{
    [Min(1)]
    public int octaves = 4;
    [Min(0.001f)]
    public float startFrequency = 0.01f;
    [Min(0)]
    public float persistance = 0.4f;
}
