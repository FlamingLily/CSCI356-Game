using System;
using UnityEngine;

public class Magic_Bullet : MonoBehaviour, I_Common_Projectile
{
    public float damage;



    public String Enemy_Tag;
    public String Effect_Tag; //i.e Freezable etc (?)

        public GameObject shooter;
    // public MonoBehaviour shooterScript;

    // public ICommon_Gun_Actions common_gun;


    void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag(Enemy_Tag))
        {

            On_Enemy_Hit(collision.gameObject);
        }
        else if (collision.gameObject.CompareTag(Effect_Tag))
        {

            On_Effected_Hit(collision.gameObject);
        }

        Destroy_Bullet();
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    void On_Enemy_Hit(GameObject hitEnemy)
    {
        Debug.Log("hit enemy");
        hitEnemy.GetComponent<I_TakeDamage>().TakeDamage(damage);
    }

    void On_Effected_Hit(GameObject hit)
    {
        Debug.Log("hit effect, calling parent");
        // if (common_gun != null)
        // {
            
            ICommon_Gun_Actions gun_interface = shooter.GetComponent<ICommon_Gun_Actions>();
            gun_interface.Do_Magic(hit);
        // }
        // effector_collider.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Frozen");
        // MonoBehaviour script = effector_collider.GetComponent<MonoBehaviour>();
        // if (script != null)
        // {
        //     script.enabled = false;
        // }
    }
}

