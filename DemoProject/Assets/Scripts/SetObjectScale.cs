using System;
using UnityEngine;

public class SetObjectScale : MonoBehaviour
{
    private Manager manager;

    private void Start()
    {
        manager = Manager.Instance;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        float sum = 0;
        for (int i = 0; i < data.Length; i++)
            sum += data[i];

        manager.scale = 100 * Math.Abs(sum / data.Length);
    }
}
