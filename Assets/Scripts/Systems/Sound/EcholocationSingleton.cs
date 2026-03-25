using System.Runtime.InteropServices;
using UnityEngine;

public class EcholocationSingleton : MonoBehaviour
{
    public static EcholocationSingleton Instance{private set; get;}

    [SerializeField]
    float waveSpeed = 25f;
    const int MAX_SOURCES = 10;
    const float WAVE_LIFETIME = 2.0f;

    [StructLayout(LayoutKind.Sequential)]
    struct AudioSourceData
    {
        public Vector3 position;
        public float radius;
        public float strength;
    }

    public Material wavePropagationMaterial;
    public Material echolocationOutlineMaterial;

    [SerializeField] private LayerMask audioInsulatorLayer;
    [SerializeField] private Transform cam;

    private ComputeBuffer audioSourceBuffer;
    private AudioSourceData[] cpuAudioSources;

    private float[] lifetimes;
    private int nextIndex = 0;
    private int audioSourcesActive;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        cpuAudioSources = new AudioSourceData[MAX_SOURCES + 3];
        lifetimes = new float[MAX_SOURCES + 3];

        audioSourceBuffer = new ComputeBuffer(MAX_SOURCES + 3, Marshal.SizeOf<AudioSourceData>(), ComputeBufferType.Structured);
        audioSourcesActive = MAX_SOURCES + 3;

        for (int i = 0; i < MAX_SOURCES + 3; i++) cpuAudioSources[i].strength = 0;


        //reserve last 3 spots of buffer for controllers and head
        cpuAudioSources[audioSourcesActive - 3].strength = 1;
        cpuAudioSources[audioSourcesActive - 3].radius = 0.3f;

        cpuAudioSources[audioSourcesActive - 2].strength = 1;
        cpuAudioSources[audioSourcesActive - 2].radius = 0.3f;

        cpuAudioSources[audioSourcesActive - 1].strength = 1;
        cpuAudioSources[audioSourcesActive - 1].radius = 3.5f;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < MAX_SOURCES; i++)
        {
            cpuAudioSources[i].radius += Time.deltaTime * waveSpeed;

            cpuAudioSources[i].strength -= (1 / lifetimes[i]) * Time.deltaTime;
            if (cpuAudioSources[i].strength < 0) cpuAudioSources[i].strength = 0;
        }

        audioSourceBuffer.SetData(cpuAudioSources);
        
        wavePropagationMaterial.SetBuffer("_AudioSources", audioSourceBuffer);

        echolocationOutlineMaterial.SetBuffer("_AudioSources", audioSourceBuffer);
    }

    public void AddSound(Vector3 worldPosition, float duration = 2)
    {
        cpuAudioSources[nextIndex].position = worldPosition;
        cpuAudioSources[nextIndex].radius = 0f;
        cpuAudioSources[nextIndex].strength = 1;

        lifetimes[nextIndex] = duration;

        nextIndex = (nextIndex + 1) % MAX_SOURCES;
    }

    public void SetLeftHandPosition(Vector3 position)
    {
        cpuAudioSources[audioSourcesActive - 3].position = position;
    }

    public void SetRightHandPosition(Vector3 position)
    {
        cpuAudioSources[audioSourcesActive - 2].position = position;
    }

    public void SetHeadPosition(Vector3 position)
    {
        cpuAudioSources[audioSourcesActive - 1].position = position;
    }

    void OnDestroy()
    {
        audioSourceBuffer.Dispose();
    }

}
