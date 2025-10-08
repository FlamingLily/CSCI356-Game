using System;
using UnityEngine;

public class Generic_Bullet : MonoBehaviour, I_Common_Projectile
{
    public float damage;

    public String Enemy_Tag;
    // public String Effect_Tag = ""; //i.e Freezable etc (?), probably not applicable for generic bullets


    void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag(Enemy_Tag))
        {
            On_Enemy_Hit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Exploder"))
        {
            Destroy(collision.gameObject);
            Explode(collision.transform.position);


        }

        // else if (Effect_Tag != "" && collision.gameObject.CompareTag(Effect_Tag))
        // {

        //     On_Effected_Hit();
        // }
        Destroy_Bullet();
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    void Explode(Vector3 explosionPosition)
    {
        Debug.Log("EXPLODER EXPLDER");
        Collider[] blast_radius = Physics.OverlapSphere(transform.position, 20.0f);

        foreach (Collider nearby in blast_radius)
        {
            Rigidbody new_rb = nearby.attachedRigidbody;
            if (new_rb != null)
            {
                new_rb.AddExplosionForce(200 * 20.0f, transform.position, 35.0f);
            }
            if (nearby.gameObject.CompareTag("Exploder"))
            {
                // Vector3 explodePos = nearby.transform.position;
                // Destroy(nearby.gameObject);
                // Explode(explodePos);
                StartCoroutine(DelayedExplosion(nearby.gameObject));
            }
        }
        // Destroy_Bullet();
    }

    System.Collections.IEnumerator DelayedExplosion(GameObject exploder)
    {
        yield return new WaitForSeconds(1.0f);
         Vector3 explodePos = exploder.transform.position;
            Destroy(exploder);
            Explode(explodePos);
    }

    void On_Enemy_Hit(GameObject hitEnemy)
    {
        Debug.Log("hit enemy");
        hitEnemy.GetComponent<AIBehaviour>().TakeDamage(damage);
    }

    void On_Effected_Hit()
    {
        Debug.Log("hit effect");
    }
}
