using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState { Patrolling, Investigating, Attacking }
    [Header("State Management")]
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Movement Settings")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float attackSpeed = 5.5f;
    [SerializeField] private float patrolSpeed = 2.0f;

    [Header("Detection Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float loseTargetDuration = 5f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource spatialAudioSource; // Set to 3D in Inspector
    [SerializeField] private AudioClip[] idleGrowls;
    [SerializeField] private AudioClip[] attackGrowls;
    [SerializeField] private float idleGrowlInterval = 2.5f;
    [SerializeField] private float attackGrowlInterval = 1f;
    private float growlInterval = 2.5f;

    private int currentPatrolIndex;
    private float loseTargetTimer;
    private Vector3 investigationPoint;
    private bool isLurking = false;

    private void OnEnable()
    {
        GameEvents.OnFallDmgNoise += HandleFallDmgNoiseDetected;
    }

    private void OnDisable()
    {
        GameEvents.OnFallDmgNoise -= HandleFallDmgNoiseDetected;
    }

    void Start()
    {
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
        StartCoroutine(PlayRandomGrowls());        
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                UpdatePatrolState();
                break;
            case EnemyState.Investigating:
                UpdateInvestigateState();
                break;
            case EnemyState.Attacking:
                UpdateAttackState();
                break;
        }

        CheckForPlayerProximity();
    }

    //State Logic
    void UpdatePatrolState()
    {
        growlInterval = idleGrowlInterval;
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextPatrolPoint();
    }

    void UpdateInvestigateState()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f && !isLurking)
            StartCoroutine(LurkRoutine());
    }

    void UpdateAttackState()
    {
        growlInterval = attackGrowlInterval;
        agent.SetDestination(playerTransform.position);

        if (HasLineOfSight())
        {
            loseTargetTimer = 0;
        }
        else
        {
            loseTargetTimer += Time.deltaTime;
            if (loseTargetTimer >= loseTargetDuration)
            {
                investigationPoint = playerTransform.position;
                EnterState(EnemyState.Investigating);
            }
        }
        Debug.Log("ATTACKING");
        if(Vector3.Distance(playerTransform.position, transform.position) < 1f)
        {
            //SWITCH OUT FOR DEATH SCENE LATER ON
            Debug.Log("YOU DIED");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }

    //Actions/Transitions

    public void EnterState(EnemyState newState)
    {
        currentState = newState;
        loseTargetTimer = 0;
        isLurking = false;

        switch (newState)
        {
            case EnemyState.Patrolling:
                agent.speed = patrolSpeed;
                GoToNextPatrolPoint();
                break;
            case EnemyState.Investigating:
                agent.speed = patrolSpeed;
                agent.SetDestination(investigationPoint);
                break;
            case EnemyState.Attacking:
                agent.speed = attackSpeed;
                break;
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;
        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    IEnumerator LurkRoutine()
    {
        isLurking = true;
        yield return new WaitForSeconds(3f); // Lurk for 3 seconds
        EnterState(EnemyState.Patrolling);
    }

    // Detection
    void CheckForPlayerProximity()
    {
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist < detectionRadius && currentState != EnemyState.Attacking)
        {
            if (HasLineOfSight()) EnterState(EnemyState.Attacking);
        }
    }

    public void HandleNoiseDetected(Vector3 noisePos)
    {
        if (currentState == EnemyState.Attacking) return;
        
        investigationPoint = noisePos;
        EnterState(EnemyState.Investigating);
    }

    bool HasLineOfSight()
    {
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        if (!Physics.Raycast(transform.position + Vector3.up, dirToPlayer, Vector3.Distance(transform.position, playerTransform.position), obstacleMask))
        {
            return true;
        }
        return false;
    }
    public void HandleFallDmgNoiseDetected(Vector3 noisePos)
    {
        float dist = Vector3.Distance(transform.position, noisePos);
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        Debug.Log("Player fell");
        // If fall noise is within range and LOS, skip investigation and attack immediately
        if (dist <= 50f && !Physics.Raycast(transform.position + Vector3.up, dirToPlayer, Vector3.Distance(transform.position, playerTransform.position), obstacleMask))
        {
            EnterState(EnemyState.Attacking);
        }
    }

    //audio

    IEnumerator PlayRandomGrowls()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(growlInterval, growlInterval * 1.15f));
            
            AudioClip[] currentClips = (currentState == EnemyState.Attacking) ? attackGrowls : idleGrowls;
            
            if (currentClips.Length > 0 && !spatialAudioSource.isPlaying)
            {
                EcholocationSingleton.Instance.AddSound(transform.position, spatialAudioSource.maxDistance/25f, spatialAudioSource.maxDistance);
                spatialAudioSource.clip = currentClips[Random.Range(0, currentClips.Length)];
                spatialAudioSource.Play();
            }
        }
    }
}