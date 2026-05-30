using UnityEngine;

public class SoulDrop : MonoBehaviour
{   
    [SerializeField] float magnetRange = 10f;
    [SerializeField] float magnetSpeed = 15f;

    int dropAmount = 10;
    PlayerInstance player;

    void Start()
    {
        player = PlayerInstance.instance;
    }

    public void InitDrop()
    {
        
    }

    void LateUpdate()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance <= magnetRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.transform.position, magnetSpeed * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player.AddSouls(dropAmount);
            Destroy(gameObject);
        }
    }
}
