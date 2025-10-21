using System;
using UnityEngine;

// Magic Bullet, used by Magic Guns (Non-Generic guns)
public class Magic_Bullet : MonoBehaviour, I_Common_Projectile //extends the Common Projectile interface
{
    public float damage; //damage inflicted by bullet
    public String Enemy_Tag; //Tag of Enemy to be damaged by bullet
    public String Effect_Tag; //Tag of Effectable objects by Magic
    public GameObject shooter;

    void OnCollisionEnter(Collision collision) //On bullet collsion with object
    {
        if (collision.gameObject.CompareTag(Enemy_Tag)) //if object is enemy
        {

            On_Enemy_Hit(collision.gameObject); //hit enemy

        }
        else if (collision.gameObject.CompareTag(Effect_Tag)) //is object is Effectable
        {

            On_Effected_Hit(collision.gameObject); //effect object

        }
        else if (collision.gameObject.CompareTag("wake_on_player")) //if object is obstacle
        {
            On_Effected_Hit(collision.gameObject);
        }
        else //if any other object, destroy bullet on collision
        {
            Destroy_Bullet();
        }
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    void On_Enemy_Hit(GameObject hitEnemy) //on enemy hit
    {
        hitEnemy.GetComponent<I_TakeDamage>().TakeDamage(damage); //damage enemy
        Destroy_Bullet(); //destroy bullet
        ICommon_Gun_Actions gun_interface = shooter.GetComponent<ICommon_Gun_Actions>(); 
        gun_interface.Do_Magic(hitEnemy); //Do Magic on enemy
    }

    void On_Effected_Hit(GameObject hit) //If Effectable object hit
    {
        ICommon_Gun_Actions gun_interface = shooter.GetComponent<ICommon_Gun_Actions>();
        gun_interface.Do_Magic(hit); //Do Magic on effectable
        Destroy_Bullet(); //Destroy bullet
    }
}

