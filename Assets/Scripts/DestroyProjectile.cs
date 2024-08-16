using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class DestroyProjectile : NetworkBehaviour
{
    void Start()
    {
        if (IsServer)
            StartCoroutine(KillObject());
    }

    IEnumerator KillObject()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (IsServer)
            Destroy(gameObject);
    }
}