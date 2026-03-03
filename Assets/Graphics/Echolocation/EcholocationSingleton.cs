using System.Runtime.InteropServices;
using UnityEngine;

public class EcholocationSingleton : MonoBehaviour
{
    public static EcholocationSingleton Instance{private set; get;}

    [SerializeField]
    float waveSpeed = 25f;
    const int MAX_SOURCES = 16;

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

    private float[] fadeSpeeds;
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
        fadeSpeeds = new float[MAX_SOURCES + 3];

        audioSourceBuffer = new ComputeBuffer(MAX_SOURCES + 3, Marshal.SizeOf<AudioSourceData>(), ComputeBufferType.Structured);
        audioSourcesActive = MAX_SOURCES + 3;

        for (int i = 0; i < MAX_SOURCES + 3; i++) cpuAudioSources[i].strength = 0;


        //reserve last 3 spots of buffer for controllers and head
        cpuAudioSources[audioSourcesActive - 3].strength = 1;
        cpuAudioSources[audioSourcesActive - 3].radius = 0.3f;

        cpuAudioSources[audioSourcesActive - 2].strength = 1;
        cpuAudioSources[audioSourcesActive - 2].radius = 0.3f;

        cpuAudioSources[audioSourcesActive - 1].strength = 1;
        cpuAudioSources[audioSourcesActive - 1].radius = 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < MAX_SOURCES; i++)
        {
            cpuAudioSources[i].radius += Time.deltaTime * waveSpeed;

            cpuAudioSources[i].strength -= fadeSpeeds[i] * Time.deltaTime;
            if (cpuAudioSources[i].strength < 0) cpuAudioSources[i].strength = 0;
        }

        audioSourceBuffer.SetData(cpuAudioSources);
        
        wavePropagationMaterial.SetBuffer("_AudioSources", audioSourceBuffer);
        wavePropagationMaterial.SetInt("_AudioSourceCount", MAX_SOURCES);

        echolocationOutlineMaterial.SetBuffer("_AudioSources", audioSourceBuffer);
        echolocationOutlineMaterial.SetInt("_AudioSourceCount", audioSourcesActive);
    }

    public void AddSound(Vector3 worldPosition, float waveLifetime, float audioMaxRange)
    {
        //check if sound is too far or it hits an audio insulator
        float distance = Vector3.Distance(worldPosition, cam.position);
        if(distance > audioMaxRange)
        {
            return;
        }

        Vector3 direction = cam.position - worldPosition;
        RaycastHit info;
        if(Physics.Raycast(worldPosition, direction, out info, distance, audioInsulatorLayer))
        {
            Debug.Log(info.collider.name);
            Debug.DrawLine(worldPosition, info.point, Color.white, 2);
            return;
        }
       //noise's distance from camera ranging from 0 to 1, where 1 = audiomaxrange and 0 = same position
        float normalizedDistance = Mathf.Clamp01(distance / audioMaxRange);
        float minValueThreshold = 0.1f;
        float maxValueThreshold = 0.8f;
        if(normalizedDistance < minValueThreshold)
        {
            normalizedDistance = minValueThreshold;
        }
        if (normalizedDistance > maxValueThreshold)
        {
            normalizedDistance = maxValueThreshold;
        }
        float t = (normalizedDistance - minValueThreshold) / (maxValueThreshold - minValueThreshold);
        float strength = Mathf.Lerp(1.0f, 0.1f, t);
        
        cpuAudioSources[nextIndex].position = worldPosition;
        cpuAudioSources[nextIndex].radius = 0f;
        cpuAudioSources[nextIndex].strength = strength;

        fadeSpeeds[nextIndex] = (waveLifetime > 0) ? (1f / waveLifetime) : 1f;
        fadeSpeeds[nextIndex] *= strength;

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
