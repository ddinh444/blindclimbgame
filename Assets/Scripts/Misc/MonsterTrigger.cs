using UnityEngine;

public class MonsterTrigger : MonoBehaviour
{
    [SerializeField] KeyItems itemRequired;
    [SerializeField] GameObject monster;
    void OnTriggerEnter(Collider collider)
    {
        if(collider.tag != "Player")
        {
            return;
        }

        if(itemRequired == KeyItems.Nothing || (Inventory.Instance.HasKeyItem(itemRequired)))
        {
            monster.SetActive(true);
        }
    }
}
