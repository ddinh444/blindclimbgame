using UnityEngine;

public class EnemySoundDetectionSystem : MonoBehaviour
{
    public static EnemySoundDetectionSystem Instance {get; private set;}
    public EnemyAI enemy;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public void CheckForInvestigativeSound(Vector3 origin, float radius)
    {
        if(enemy == null)
        {
            return;
        }
        if(Vector3.Distance(enemy.transform.position, origin) <= radius)
        {
            enemy.Investigate(origin);
        }
    }

    public void CheckForAggroingSound(Vector3 origin, float radius)
    {
        if(enemy == null)
        {
            return;
        }
        if(Vector3.Distance(enemy.transform.position, origin) <= radius)
        {
            enemy.ChangeState("Attacking");
        }
    }
}
