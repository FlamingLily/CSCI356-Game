using System;
using UnityEngine;

public class Magic_Bullet : MonoBehaviour, I_Common_Projectile
{
    public float damage;

    public String Enemy_Tag;
    public String Effect_Tag; //i.e Freezable etc (?)


    void OnCollisionEnter(Collision collision)
    {


        if (collision.gameObject.CompareTag(Enemy_Tag))
        {

            On_Enemy_Hit();
        }
        else if (collision.gameObject.CompareTag(Effect_Tag))
        {

            On_Effected_Hit();
        }

        Destroy_Bullet();
    }

    void Destroy_Bullet()
    {
        Destroy(this.gameObject);
    }

    void On_Enemy_Hit()
    {
        Debug.Log("hit enemy");
        //get enemy monobehav and reduce health
    }

    void On_Effected_Hit()
    {
        Debug.Log("hit effect");
    }
}

