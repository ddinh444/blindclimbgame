using UnityEngine;
using UnityEngine.SceneManagement;

public class PrototypeDemoEnemy : MonoBehaviour
{
    [SerializeField] private float speed = 8;
    [SerializeField] private float aggroDistance = 3.5f;
    [SerializeField] private float attackDistance = 2;
    [SerializeField] private GameObject target;
    private Rigidbody rb;
    private bool isAggroed;
    private Vector3 origPosition;
    private Vector3 origTargetPosition;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isAggroed = false;
        origPosition = transform.position;
        origTargetPosition = target.transform.position;
    }

    void FixedUpdate()
    {
        float distance = Vector3.Distance(target.transform.position,transform.position);
        if(distance < aggroDistance)
        {
            isAggroed = true;
        }

        if (isAggroed)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.Normalize();
            Vector3 velocity = speed * dir;
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }

        if(distance < attackDistance)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
