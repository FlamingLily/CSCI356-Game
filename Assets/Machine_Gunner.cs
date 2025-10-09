using UnityEngine;
using System.Collections;

public class Machine_Gunner : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform gun_grab_point;

    public Transform scopePosition;
    public Transform barrelDirection;

    public float bullet_damage;

    public float bullet_spray = 0.1f;

    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;

    private float lastFired = 0;
    private float fireRate = 0.05f;

    public float kick_back_force = 0.05f;
    public float kick_back_time = 0.1f;
    public CharacterController playerController;

    private Coroutine kickbackRoutine;

    public AudioSource player_audio_source;

    private bool isFiringForSound = false;

    public AudioClip fire_audio;
    public AudioClip scope_in_audio;
    public AudioClip scope_out_audio;

    public Transform Get_Grab_Point()
    {
        return gun_grab_point;
    }

    public Transform Get_Barrel_Direction()
    {
        return barrelDirection;
    }

    public Transform Get_Scope()
    {
        return scopePosition;
    }

    public void Fire()
    {
        float timeNow = Time.time;
        if (!isFiringForSound)
        {
            player_audio_source.clip = fire_audio;
            player_audio_source.loop = true;
            player_audio_source.Play();
            isFiringForSound = true;
        }

        // Prevents the machine gun from firing more than every fireRate seconds
        if (timeNow - lastFired >= fireRate)
        {
            Shoot();
            lastFired = timeNow;
        }
        Debug.Log("MACHINE GUN FIRE");
    }

    public void Shoot()
    {
        // Random bullet spread
        Vector3 randomDirection = barrelDirection.forward +
                      new Vector3(
                          Random.Range(-bullet_spray, bullet_spray),
                          Random.Range(-bullet_spray, bullet_spray),
                          0f
                      );

        GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);
        Generic_Bullet bullet_brains = projectile.GetComponent<Generic_Bullet>();
        if (bullet_brains != null)
        {
            bullet_brains.damage = bullet_damage;
            bullet_brains.Enemy_Tag = "Enemy";
            // bullet_brains.Effect_Tag = "";
        }

        projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);


        // if (!player_audio_source.isPlaying)
        // {
        //     player_audio_source.PlayOneShot(fire_audio, 0.5f);
        // }

        Debug.Log("MACHINEGUN FIRE");
    }

    public void Scope_in()
    {
        Debug.Log("MACHINE GUN SCOPE IN");
        player_audio_source.PlayOneShot(scope_in_audio, 1);
    }

    public void Scope_out()
    {
        Debug.Log("MACHINE GUN SCOPE OUT");
        player_audio_source.PlayOneShot(scope_out_audio, 1);
    }

    public void Reload()
    {
        
          if (isFiringForSound)
        {
            player_audio_source.clip = fire_audio;
            player_audio_source.loop = false;
            player_audio_source.Stop();
            isFiringForSound = false;
        }
     }

    void Start()
    {

    }

    void Update()
    {

    }

    public void Do_Magic(GameObject target)
    {

    }
}