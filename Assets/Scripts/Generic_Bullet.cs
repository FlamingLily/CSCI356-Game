using System;
using UnityEngine;

// Generic Bullet, fired by Generic (Non-Magic GUns)
public class Generic_Bullet : MonoBehaviour, I_Common_Projectile //extends the Common Projectile interface
{
    public float damage; //damage points inflicted

    public String Enemy_Tag; //Tag of Enemy to be damaged by bullet

    public AudioClip explosion_sound; //Audioclip played on collision with 'Exploder'
    public AudioClip hit; //Audio clip played on hit

    void OnCollisionEnter(Collision collision) //when bullet collides with an object
    {
        if (collision.gameObject.CompareTag(Enemy_Tag)) //if that object is an 'Enemy'
        {
            On_Enemy_Hit(collision.gameObject); // damange Enemy
        }
        else if (collision.gameObject.CompareTag("Exploder")) //if object is 'Exploder'
        {
            // Explode(collision.transform.position);
            Collider[] interactable_colliders_in_radius = Physics.OverlapSphere(transform.position, 15.0f); //get all colliders in Explosion radius
            foreach (var interactable_collider in interactable_colliders_in_radius)
            {
                GameObject interactable_object_in_radius = interactable_collider.gameObject;
                if (interactable_object_in_radius.CompareTag("Player")) //if player in collider radius
                {
                    Player_Movement movement = interactable_object_in_radius.GetComponent<Player_Movement>();
                    movement.Loose_Health_Points(20, 1.0f); //player takes damage
                    movement.Ragdoll(); //player ragdolls
                    Rigidbody ghostRigid = movement.ghost.GetComponent<Rigidbody>();
                    ghostRigid.AddExplosionForce(200 * 20.0f, transform.position, 35.0f); //player ragdoll is repelled by explosion
                }
                else if (interactable_object_in_radius.CompareTag("Enemy")) //if enemy in explosion radius
                {
                    interactable_object_in_radius.GetComponent<I_TakeDamage>().TakeDamage(20); //damage enemy
                }
                // AudioSource.PlayClipAtPoint(explosion_sound, this.gameObject.transform.position, 1.0f); //play explosion noise at explosion source
                // Destroy(collision.gameObject); //destroy Exploder
            }
            AudioSource.PlayClipAtPoint(explosion_sound, this.gameObject.transform.position, 1.0f); //play explosion noise at explosion source
            Destroy(collision.gameObject); //destroy Exploder
        }
        Destroy_Bullet();
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    // void Explode(Vector3 explosionPosition)
    // {
    //     Debug.Log("EXPLODER EXPLDER");
    //     Collider[] blast_radius = Physics.OverlapSphere(transform.position, 20.0f);

    //     foreach (Collider nearby in blast_radius)
    //     {
    //         Rigidbody new_rb = nearby.attachedRigidbody;
    //         if (new_rb != null)
    //         {
    //             new_rb.AddExplosionForce(200 * 20.0f, transform.position, 35.0f);
    //         }
    //         //     if (nearby.gameObject.CompareTag("Exploder"))
    //         //     {
    //         //         // Vector3 explodePos = nearby.transform.position;
    //         //         // Destroy(nearby.gameObject);
    //         //         // Explode(explodePos);
    //         //         StartCoroutine(DelayedExplosion(nearby.gameObject));
    //         //     }
    //     }
    //     // // Destroy_Bullet();
    // }

    // System.Collections.IEnumerator DelayedExplosion(GameObject exploder)
    // {
    //     yield return new WaitForSeconds(1.0f);
    //      Vector3 explodePos = exploder.transform.position;
    //         Destroy(exploder);
    //         Explode(explodePos);
    // }

    void On_Enemy_Hit(GameObject hitEnemy)
    {
        Debug.Log("hit enemy");
        hitEnemy.GetComponent<I_TakeDamage>().TakeDamage(damage); //on enemy hit, enemy takes damage
    }

    void On_Effected_Hit() //on effect hit not implemented for generic bullets
    {
        Debug.Log("hit effect");
    }
}
