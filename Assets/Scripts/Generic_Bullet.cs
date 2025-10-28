using System;
using UnityEngine;

// Generic Bullet, used by Generic (Non-magic guns) i.e pistol, shotgun etc
public class Generic_Bullet : MonoBehaviour, I_Common_Projectile
{
    public float damage; //damage done by bullet hit

    public String Enemy_Tag; //the tag of the enemy
     public AudioClip explosion_sound; 
    public AudioClip hit;
    void OnCollisionEnter(Collision collision) //when bullet collides with object
    {
        if (collision.gameObject.CompareTag(Enemy_Tag))
        {
            On_Enemy_Hit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag("Exploder"))
        {
            Explode(collision.transform.position);
            Collider[] interactable_colliders_in_radius = Physics.OverlapSphere(transform.position, 15.0f);
            foreach (var interactable_collider in interactable_colliders_in_radius)
            {
                GameObject interactable_object_in_radius = interactable_collider.gameObject;
                if (interactable_object_in_radius.CompareTag("Player")) //if player in explosion radius
                {
                    Player_Movement movement = interactable_object_in_radius.GetComponent<Player_Movement>();
                    movement.Loose_Health_Points(20, 1.0f); //hurt player
                    movement.Ragdoll();
                    Rigidbody ghostRigid = movement.ghost.GetComponent<Rigidbody>(); //ragdoll player
                    ghostRigid.AddExplosionForce(200 * 20.0f, transform.position, 35.0f);
                    if (movement.Health <= 0) //if exploder kills player
                    {
                        movement.Die();
                    }
                }
                else if (interactable_object_in_radius.CompareTag("Enemy")) // if enemy in explosion radius
                {
                    interactable_object_in_radius.GetComponent<I_TakeDamage>().TakeDamage(20); //damage enemy
                    Rigidbody ghostRigid = interactable_object_in_radius.GetComponent<Rigidbody>();
                    ghostRigid.AddExplosionForce(200 * 20.0f, transform.position, 35.0f);
                }
                AudioSource.PlayClipAtPoint(explosion_sound, this.gameObject.transform.position, 1.0f);
                Destroy(collision.gameObject); //destroy Exploder object
            }
        }
        Destroy_Bullet();
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    void Explode(Vector3 explosionPosition) //propels all rigid bodies in explosion radius
    {
        Collider[] blast_radius = Physics.OverlapSphere(transform.position, 20.0f);

        foreach (Collider nearby in blast_radius)
        {
            Rigidbody new_rb = nearby.attachedRigidbody;
            if (new_rb != null)
            {
                new_rb.AddExplosionForce(200 * 20.0f, transform.position, 35.0f);
            }
        }

    }

    void On_Enemy_Hit(GameObject hitEnemy) //damage enemy on bullet hit
    {
        hitEnemy.GetComponent<I_TakeDamage>().TakeDamage(damage);
    }

    void On_Effected_Hit() //on effected hit (not implemented for generic weapons)
    {
        Debug.Log("hit effect");
    }
}
