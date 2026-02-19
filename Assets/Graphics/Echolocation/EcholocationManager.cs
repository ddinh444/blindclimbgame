using System.Runtime.InteropServices;
using UnityEngine;

public class AudioSourceSingleton : MonoBehaviour
{
    [SerializeField]
    float waveSpeed = 25f;

    [SerializeField]
    float waveLifetime = 2;
    const int MAX_SOURCES = 32;

    private AudioSource clackNoise;

    [StructLayout(LayoutKind.Sequential)]
    struct AudioSourceData
    {
        public Vector3 position;
        public float radius;
        public float strength;
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

        audioSourcesActive = 1;

        cpuAudioSources[0].position = new Vector3(17,-7.94f,-18.41f);
        cpuAudioSources[0].radius = 0;
        cpuAudioSources[0].strength = 1;

        clackNoise = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < audioSourcesActive; i++)
        {
            cpuAudioSources[i].radius += Time.deltaTime * waveSpeed;
            cpuAudioSources[i].strength -= Time.deltaTime / waveLifetime;

            if(cpuAudioSources[i].strength < 0)
            {
                cpuAudioSources[i].radius = 0;
                cpuAudioSources[i].strength = 1;
                clackNoise.time = 0.1f;
                clackNoise.Play();
            }
        }

        audioSourceBuffer.SetData(cpuAudioSources);
        
        wavePropagationMaterial.SetBuffer("_AudioSources", audioSourceBuffer);
        wavePropagationMaterial.SetInt("_AudioSourceCount", audioSourcesActive);

        echolocationOutlineMaterial.SetBuffer("_AudioSources", audioSourceBuffer);
        echolocationOutlineMaterial.SetInt("_AudioSourceCount", audioSourcesActive);
    }

}
