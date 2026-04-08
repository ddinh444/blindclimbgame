using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]private bool loop;
    [SerializeField]private Transform[] waypoints;
    [SerializeField]private float speed;

    private int index = 0;

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, waypoints[index].position, Time.deltaTime * speed);
        if(Vector3.Distance(transform.position, waypoints[index].position) < 0.01f){
            index += 1;
            if(index > waypoints.Length){
                index = 0;
            }
        }
    }
}
