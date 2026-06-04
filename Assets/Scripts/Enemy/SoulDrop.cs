using System.Collections;
using UnityEngine;

public class SoulDrop : MonoBehaviour
{   
    [SerializeField] float magnetRange = 10f;
    [SerializeField] float magnetSpeed = 15f;
    [SerializeField] ParticleSystem particles;
    [SerializeField] MeshRenderer mesh;

    int dropAmount = 10;
    PlayerInstance player;
    bool used = false;

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

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player") && !used)
        {
            player.AddSouls(dropAmount);
            StartCoroutine(DelayedDestroy());
        }
    }

    IEnumerator DelayedDestroy()
    {
        used = true;
        mesh.enabled = false;
        GetComponent<Collider>().enabled = false;
        particles.Stop();

        yield return new WaitForSeconds(2f);
        
        Destroy(gameObject);
    }
}
