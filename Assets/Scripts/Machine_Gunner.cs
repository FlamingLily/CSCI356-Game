using UnityEngine;
using System.Collections;

// Rapid Fire Generic Weapon (Machine Gun)
public class Machine_Gunner : MonoBehaviour, ICommon_Gun_Actions //Extends Common Gun Action interface
{
    public Transform gun_grab_point; //where gun is held by player

    public Transform scopePosition; //camera position for scope in
    public Transform barrelDirection; //barrel direction of gun

    public float bullet_damage; //damage points inflicted by fired bullet

    public float bullet_spray = 0.1f; //random jitter of fired bullet

    public GameObject projectilePrefab = null; //prefab of fired bullet (Generic Bullet)
    public float launchForce = 0.0f; //launch force of gun

    private float lastFired = 0; //tick when gun was last fired
    private float fireRate = 0.05f; //how rapidly gun can fire

    public float kick_back_force = 0.05f; //distance gun 'kicks-back' player when fired
    public float kick_back_time = 0.1f; //kick-back animation time
    public CharacterController playerController; //Player controller

    public AudioSource player_audio_source; //player audio source

    private bool isFiringForSound = false; //is firing sound playing? boolean

    public AudioClip fire_audio; //gun firing audio clip
    public AudioClip scope_in_audio; //scope in audio clip
    public AudioClip scope_out_audio; //scope out audio clip

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

    public void Fire() //On gun fire
    {
        float timeNow = Time.time; //timestamp which gun fired
        if (!isFiringForSound) //if firing sound is not playing
        {
            player_audio_source.clip = fire_audio;
            player_audio_source.loop = true;
            player_audio_source.Play(); //start playing and looping firing sounds
            isFiringForSound = true;
        }

        //if last fired time is more than fireRate
        if (timeNow - lastFired >= fireRate) // Prevents the machine gun from firing more than every fireRate seconds
        {
            Shoot();
            lastFired = timeNow; //reset last fired timestamp
        }

    }

//called while firing, is gun was last fired more than fire rate ago
    public void Shoot() 
    {
        // Random bullet spread
        Vector3 randomDirection = barrelDirection.forward +
                      new Vector3(
                          Random.Range(-bullet_spray, bullet_spray),
                          Random.Range(-bullet_spray, bullet_spray),
                          0f
                      );

        GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation); //instantiate Generic Bullet projectile
        Generic_Bullet bullet_brains = projectile.GetComponent<Generic_Bullet>(); //get bullet MonoBehaviour
        if (bullet_brains != null) //if prefab has behaviour
        {
            bullet_brains.damage = bullet_damage; //set bullet damage
            bullet_brains.Enemy_Tag = "Enemy"; //set Enemy tag
        }

        projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce); //propel bullet
    }

    public void Scope_in() //called when player scopes in
    {
        player_audio_source.PlayOneShot(scope_in_audio, 1); //play scope in noise
    }

    public void Scope_out() //called when player scopes out
    {
        player_audio_source.PlayOneShot(scope_out_audio, 1); //play scope out noise
    }

    public void Reload() //called when player lets go of Firing button
    {
        if (isFiringForSound) //if firing sound playing
        {
            player_audio_source.clip = fire_audio;
            player_audio_source.loop = false;
            player_audio_source.Stop(); //Stop playing firing sound
            isFiringForSound = false;
        }
    }

    public void Do_Magic(GameObject target) //Do_Magic, a function on Generic Gun Interface, is created but not implemented for Generic Guns
    {

    }
}