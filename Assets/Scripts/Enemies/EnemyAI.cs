using UnityEngine;

using System.Collections.Generic;
using System;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class EnemyAI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource spatialAudioSrc;
    [SerializeField] private NavMeshAgent agent;

    [Header("General")]
    [SerializeField] private bool startFromHanging = true;
    [SerializeField] private int deathSceneIndex = 3;

    [Header("Hanging")]
    [SerializeField] private float timeHangingBeforeNoise;
    [SerializeField] private AudioClip hangingNoise;
    [SerializeField] private float timeHangingAfterNoise;

    [Header("Patrolling")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private AudioClip[] patrolNoises;
    [SerializeField] private float timeBetweenPatrolNoises;
    [SerializeField] private float patrolIdleTime = 2f;
    [SerializeField] private float patrolSpeed = 2.5f;

    [Header("Detection")]
    [SerializeField] private Transform player;
    [SerializeField] private float detectionRange = 3f;
    [SerializeField] private float loseTargetTime = 3f;
    [SerializeField] private LayerMask losMask;

    [Header("Investigating")]
    [SerializeField] private AudioClip investigateStartNoise;
    [SerializeField] private float investigateIdleTime = 2f;
    [SerializeField] private AudioClip investigateEndNoise;
    private Vector3 investigateTarget;

    [Header("Attacking")]
    [SerializeField] private float attackRange;
    [SerializeField] private float attackingSpeed;
    [SerializeField] private AudioClip[] attackingNoises;
    [SerializeField] private float timeBetweenAttackNoises;

    private float jumpCooldown = 0.5f;
    private float jumpTimer;
    private Dictionary<String, State> states;

    State currState;

    void Start()
    {
        states = new Dictionary<string, State>()
        {
            { "Hanging", new HangingState() },
            { "Dropping", new DroppingState() },
            { "Patrolling", new PatrollingState() },
            { "Investigating", new InvestigatingState() },
            { "Attacking", new AttackingState() }
        };

        foreach (var s in states.Values)
            s.ai = this;

        currState = startFromHanging ? states["Hanging"] : states["Patrolling"];
        currState.Enter();
    }

    public void ChangeState(String stateName)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.Log($"state {stateName} does not exist");
            return;
        }

        Debug.Log("new state state:" + stateName);

        State nextState = states[stateName];
        currState.Exit();
        currState = nextState;
        currState.Enter();
    }

    void Update()
    {
        currState.Update();
    }

    void FixedUpdate()
    {
        jumpTimer -= Time.fixedDeltaTime;
        currState.FixedUpdate();
    }

    public void Investigate(Vector3 position)
    {
        investigateTarget = position;
        ChangeState("Investigating");
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > detectionRange) return false;

        Vector3 dir = (player.position - transform.position).normalized;

        if (!Physics.Raycast(transform.position, dir, out RaycastHit hit, detectionRange, losMask))
        {
            return true;
        }

        return false;
    }

    public abstract class State
    {
        public EnemyAI ai;
        public abstract void Enter();
        public abstract void Update();
        public abstract void FixedUpdate();
        public abstract void Exit();
    };

    class HangingState : State
    {
        private float timer;
        private bool hasMadeNoise;
        public override void Enter()
        {
            ai.agent.enabled = false;
            ai.anim.SetBool("Hanging", true);
            timer = 0;
            hasMadeNoise = false;
            MusicSingleton.Instance.PlayAmbient();
        }

        public override void Exit()
        {
            ai.anim.SetBool("Hanging",false);
        }

        public override void FixedUpdate()
        {
            timer += Time.fixedDeltaTime;
            if(timer >= ai.timeHangingBeforeNoise && !hasMadeNoise)
            {
                ai.spatialAudioSrc.clip = ai.hangingNoise;
                ai.spatialAudioSrc.Play();
                if(HapticSingleton.Instance != null)
                {
                    HapticSingleton.Instance.SendDirectionalImpulse(1, 0.3f, ai.transform.position, 10);
                }
                EcholocationSingleton.Instance.AddSound(ai.transform.position, ai.hangingNoise.length + .5f);
                hasMadeNoise = true;
            }

            if(timer >= ai.timeHangingAfterNoise + ai.timeHangingBeforeNoise)
            {
                ai.ChangeState("Dropping");
            }
        }

        public override void Update(){}
    }

    class DroppingState : State
    {
        private float velocity = 0;
        public override void Enter()
        {
            velocity = 0;
            ai.agent.enabled = false;
            ai.anim.SetBool("Hanging",true);
        }

        public override void FixedUpdate()
        {
            velocity -= Time.fixedDeltaTime * 10;
            ai.transform.position += Vector3.up * velocity * Time.fixedDeltaTime;
            // Check if grounded via raycast
            if (Physics.Raycast(ai.transform.position, Vector3.down, 1f))
            {
                ai.ChangeState("Patrolling");
            }
        }

        public override void Update(){}

        public override void Exit()
        {
            ai.agent.enabled = true;
            ai.anim.SetBool("Hanging", false);
            ai.anim.SetBool("Grounded", true);
        }
    }

    class PatrollingState : State
    {
        private float noiseTimer;
        private float idleTimer;
        private Transform targetPoint;
        private int lastIndex;
        private bool isIdle;

        public override void Enter()
        {
            ai.agent.enabled = true;
            ai.agent.speed = ai.patrolSpeed;
            PickNewPoint();
            lastIndex = -1;
            MusicSingleton.Instance.PlayAmbient();
            ai.anim.SetBool("Grounded", true);
        }

        void PickNewPoint()
        {
            if (ai.patrolPoints.Length == 0) return;

            int index = UnityEngine.Random.Range(0, ai.patrolPoints.Length);
            if(index == lastIndex)
            {
                index = (index + 1) % ai.patrolPoints.Length;
            }
            targetPoint = ai.patrolPoints[index];
            ai.agent.SetDestination(targetPoint.position);
            isIdle = false;
            lastIndex = index;
        }

        public override void Update()
        {
            noiseTimer += Time.deltaTime;
            if(noiseTimer >= ai.timeBetweenPatrolNoises)
            {
                int index = UnityEngine.Random.Range(0, ai.patrolNoises.Length);
                ai.spatialAudioSrc.clip = ai.patrolNoises[index];
                ai.spatialAudioSrc.Play();
                if(HapticSingleton.Instance != null)
                {
                    HapticSingleton.Instance.SendDirectionalImpulse(0.85f, 0.15f, ai.transform.position, 10);
                }
                EcholocationSingleton.Instance.AddSound(ai.transform.position, ai.spatialAudioSrc.clip.length);
                noiseTimer = 0;
            }

            // Check for player
            if (ai.CanSeePlayer())
            {
                ai.ChangeState("Attacking");
                return;
            }

            if (!isIdle)
            {
                ai.anim.SetBool("Investigating", false);
                if (!ai.agent.pathPending && ai.agent.remainingDistance < 0.5f)
                {
                    isIdle = true;
                    idleTimer = 0f;
                }
            }
            else
            {
                ai.anim.SetBool("Investigating", true);
                idleTimer += Time.deltaTime;
                if (idleTimer >= ai.patrolIdleTime)
                {
                    PickNewPoint();
                }
            }

            if (ai.agent.isOnOffMeshLink)
            {
            }
        }

        public override void FixedUpdate(){}

        public override void Exit(){}
    }

    class InvestigatingState : State
    {
        private float idleTimer;
        private bool isIdle;

        public override void Enter()
        {
            ai.spatialAudioSrc.clip = ai.investigateStartNoise;
            ai.spatialAudioSrc.Play();
            if(HapticSingleton.Instance != null)
            {
                HapticSingleton.Instance.SendDirectionalImpulse(0.85f, 0.15f, ai.transform.position, 10);
            }
            EcholocationSingleton.Instance.AddSound(ai.transform.position);
            ai.agent.SetDestination(ai.investigateTarget);
            isIdle = false;
             ai.anim.SetBool("Grounded", true);
        }

        public override void Update()
        {
            if (ai.CanSeePlayer())
            {
                ai.ChangeState("Attacking");
                return;
            }

            if (!isIdle)
            {
                ai.anim.SetBool("Investigating", false);
                if (!ai.agent.pathPending && ai.agent.remainingDistance < 0.5f)
                {
                    isIdle = true;
                    idleTimer = 0f;
                }
            }
            else
            {
                ai.anim.SetBool("Investigating", true);
                idleTimer += Time.deltaTime;
                if (idleTimer >= ai.investigateIdleTime)
                {
                    ai.spatialAudioSrc.clip = ai.investigateEndNoise;
                    ai.spatialAudioSrc.Play();
                    if(HapticSingleton.Instance != null)
                    {
                        HapticSingleton.Instance.SendDirectionalImpulse(0.85f, 0.15f, ai.transform.position, 10);
                    }
                    EcholocationSingleton.Instance.AddSound(ai.transform.position);
                    ai.ChangeState("Patrolling");
                }
            }
        }

        public override void FixedUpdate(){}

        public override void Exit(){}
    }

    class AttackingState : State
    {
        private float noiseTimer;
        private float lostTimer;

        public override void Enter()
        {
            ai.anim.SetBool("Attacking", true);
            ai.agent.speed = ai.attackingSpeed;
            lostTimer = 0f;
            noiseTimer = 0;
            ai.anim.SetBool("Grounded", true);

            MusicSingleton.Instance.PlayChase();
        }

        public override void Update()
        {
            noiseTimer += Time.deltaTime;
            if(noiseTimer >= ai.timeBetweenAttackNoises)
            {
                int index = UnityEngine.Random.Range(0, ai.attackingNoises.Length);
                ai.spatialAudioSrc.clip = ai.attackingNoises[index];
                ai.spatialAudioSrc.Play();
                if(HapticSingleton.Instance != null)
                {
                    HapticSingleton.Instance.SendDirectionalImpulse(1, .3f, ai.transform.position, 10);
                }
                EcholocationSingleton.Instance.AddSound(ai.transform.position);
                noiseTimer = 0;
            }


            if (ai.player == null)
            {
                ai.ChangeState("Patrolling");
                return;
            }

            ai.agent.SetDestination(ai.player.position);

            if (ai.CanSeePlayer())
            {
                lostTimer = 0f;

                if(Vector3.Distance(ai.transform.position, ai.player.transform.position) < ai.attackRange){
                    SceneManager.LoadScene(ai.deathSceneIndex);
                }
            }
            else
            {
                lostTimer += Time.deltaTime;
                if (lostTimer >= ai.loseTargetTime)
                {
                    ai.investigateTarget = ai.transform.position;
                    ai.ChangeState("Investigating");
                }
            }

            if (ai.agent.isOnOffMeshLink && ai.jumpTimer < 0)
            {
                ai.jumpTimer = ai.jumpCooldown;
            }
        }

        public override void FixedUpdate(){}

        public override void Exit()
        {
            ai.anim.SetBool("Attacking", false);
        }
    }
}