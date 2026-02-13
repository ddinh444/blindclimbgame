using System.Runtime.InteropServices;
using UnityEngine;

public class AudioSourceSingleton : MonoBehaviour
{
    const float WAVE_SPEED = 5.0f;
    const int MAX_SOURCES = 32;

    [StructLayout(LayoutKind.Sequential)]
    struct AudioSourceData
    {
        public Vector3 position;
        public float radius;
        public float ttl;
    }

    public Material wavePropagationMaterial; //intended for walls and environment
    public Material echolocationOutlineMaterial; //intended for interactables

    private ComputeBuffer audioSourceBuffer;
    private AudioSourceData[] cpuAudioSources;

    private int audioSourcesActive;

    void Start()
    {
        cpuAudioSources = new AudioSourceData[MAX_SOURCES];

        audioSourceBuffer = new ComputeBuffer(MAX_SOURCES, Marshal.SizeOf<AudioSourceData>(), ComputeBufferType.Structured);

        audioSourcesActive = 0;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < audioSourcesActive; i++)
        {
            cpuAudioSources[i].radius += Time.deltaTime * WAVE_SPEED;
            cpuAudioSources[i].ttl -= Time.deltaTime;
        }

        audioSourceBuffer.SetData(cpuAudioSources);
    }
}
