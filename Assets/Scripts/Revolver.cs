using UnityEngine;
using System.Collections;

// Single Fire Generic Weapon (Revolver)
public class Revolver : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform gun_grab_point;//where gun is held by player

    public Transform scopePosition;//camera position for scope in

    public Transform barrelDirection;//barrel direction of gun

    public float kick_back_force;//distance gun 'kicks-back' player when fired
    public float kick_back_time;//kick-back animation time

    private Coroutine kickbackRoutine; //kickback animation routine

    public CharacterController playerController; //Player controller

    public AudioSource player_audio_source; //player audio source

    public AudioClip fire_audio; //gun firing audio clip
    public AudioClip scope_in_audio;//scope in audio clip
    public AudioClip scope_out_audio;//scope out audio clip


    public GameObject projectilePrefab = null; //prefab of fired bullet (Generic Bullet)
    public float launchForce = 0.0f; //launch force of gun
    float lastFired = 0;//tick when gun was last fired

    public float fire_rate; //how rapidly gun can fire

    bool is_left_click_held = false; //only allows the gun to be fired once per left click

    public float bullet_damage; //damage points inflicted by fired bullet



    public Transform Get_Grab_Point() //return grab point of gun
    {
        return gun_grab_point;
    }
    public Transform Get_Barrel_Direction() //return barrel direction (position and direction in which bullets are fired from)
    {
        return barrelDirection;
    }
    public Transform Get_Scope() //get scope position
    {
        return scopePosition;
    }
    public void Fire()
    {

        if (!is_left_click_held && Time.time >= lastFired + fire_rate)
        //if left click is not being held, and gun is able to be fired 
        //i.e gun can only be fired every time firing button is held, then released
        //gun will not fire continously if firing button is held and not released
        {
            GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);

            Generic_Bullet bullet_brains = projectile.GetComponent<Generic_Bullet>(); //instantiate generic bullet
            if (bullet_brains != null)
            {
                bullet_brains.damage = bullet_damage; //set bullet properties
                bullet_brains.Enemy_Tag = "Enemy";
            }

            projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce); //fire bullet
            player_audio_source.PlayOneShot(fire_audio, 1); //play firing audio

            if (kickbackRoutine != null) StopCoroutine(kickbackRoutine);
            kickbackRoutine = StartCoroutine(Gun_Kick());
            lastFired = Time.time; //set last fired timestamp
            is_left_click_held = true;
        }
        else
        {
            is_left_click_held = false;
        }


        IEnumerator Gun_Kick() //gun kick animation
        {
            float halfTime = kick_back_time / 2f;


            Vector3 startPos = transform.localPosition;
            Vector3 kickbackPos = startPos + Vector3.back * kick_back_force;


            float t = 0f;
            while (t < 1f) //animating gun going backwards / 'kicking'
            {
                t += Time.deltaTime / halfTime;
                transform.localPosition = Vector3.Lerp(startPos, kickbackPos, t);
                Vector3 player_kickback = -playerController.transform.forward * (kick_back_force / 100);
                playerController.Move(player_kickback);
                yield return null;
            }


            t = 0f;
            while (t < 1f) //animating gun returning to its normal position
            {
                t += Time.deltaTime / halfTime;
                transform.localPosition = Vector3.Lerp(kickbackPos, startPos, t);
                yield return null;
            }

            transform.localPosition = startPos;
            kickbackRoutine = null;
        }
    }

    public void Scope_in() //play scope in sound
    {
        player_audio_source.PlayOneShot(scope_in_audio, 1);
    }

    public void Scope_out() //play scope out sound
    {
        player_audio_source.PlayOneShot(scope_out_audio, 1);
    }



    public void Reload() //called when left click is released, allowing the player to fire again
    {
        is_left_click_held = false;
    }

    public void Do_Magic(GameObject target) //not implemented for generic weapons
    {

    }

}
