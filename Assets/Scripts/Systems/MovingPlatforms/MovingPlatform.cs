using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]private bool loop;
    [SerializeField]private Transform[] waypoints;
    [SerializeField]private float speed;
    [SerializeField] private float waypointLingerTime = 1;

    private int index = 0;
    private Rigidbody rb;
    private PhysicsRig rig;
    private bool isPlayerOnPlatform;
    private float timer;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rig = GameObject.FindGameObjectWithTag("Rig").GetComponent<PhysicsRig>();
    }

    void FixedUpdate()
    {
        if(waypoints.Length == 0)
        {
            return;
        }
        Vector3 target = waypoints[index].position;
        Vector3 newPosition = Vector3.MoveTowards(rb.position, target, Time.fixedDeltaTime * speed);

        if (isPlayerOnPlatform)
        {
            Vector3 delta = newPosition - rb.position;
            rig.ApplyPlatformDelta(delta);
        }

        rb.MovePosition(newPosition);

        if (Vector3.Distance(rb.position, target) < 0.01f)
        {
            timer += Time.fixedDeltaTime;
            if(timer > waypointLingerTime)
            {
                timer = 0;
                index++;
                if (index >= waypoints.Length && loop) index = 0;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        Debug.Log("parented player");

        isPlayerOnPlatform = true;
    }

    void OnTriggerExit(Collider other)
    {
        if(other.tag != "Player")
        {
            return;
        }

        Debug.Log("deparented player");


        isPlayerOnPlatform = false;
    }
}
