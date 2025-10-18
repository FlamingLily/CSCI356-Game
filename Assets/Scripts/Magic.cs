using UnityEngine;
using System.Collections;

public class Magic : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;

    public float kick_back_force;
    public float kick_back_time;

    private Vector3 originalLocalPos;
    private Coroutine kickbackRoutine;

    Vector3 move = Vector3.zero;


    public Material Frozen;

    public CharacterController playerController;


    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;
    float lastFired = 0;

    public float fire_rate;

    bool is_left_click_held = false;

    public float bullet_damage;

            public AudioSource player_audio_source;

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
        Debug.Log("REVOLVER FIRE");

        if (Input.GetKey(KeyCode.V))
        {
            if (!is_left_click_held && Time.time >= lastFired + fire_rate)
            {
                GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);
                Magic_Bullet bullet_brains = projectile.GetComponent<Magic_Bullet>();
                bullet_brains.shooter = this.gameObject;
                // bullet_brains.shooterScript = this;


                if (bullet_brains != null)
                {
                    bullet_brains.damage = bullet_damage;
                    bullet_brains.Enemy_Tag = "Enemy";
                    bullet_brains.Effect_Tag = "Freezable";
                }

                projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);
                player_audio_source.PlayOneShot(fire_audio, 1);

                if (kickbackRoutine != null) StopCoroutine(kickbackRoutine);
                kickbackRoutine = StartCoroutine(Gun_Kick());
                lastFired = Time.time;
                //Debug.Log("HANDGUN FIRE");
                is_left_click_held = true;
            }
            else
            {
                is_left_click_held = false;
            }

        }
    }

    public void Do_Magic(GameObject target)
    {
        target.GetComponent<Renderer>().material = Frozen;
        MonoBehaviour script = target.GetComponent<MonoBehaviour>();
        if (script != null)
        {
            script.enabled = false;
        }
    }

    IEnumerator Gun_Kick()
    {
        float halfTime = kick_back_time / 2f;


        Vector3 startPos = transform.localPosition;
        Vector3 kickbackPos = startPos + Vector3.back * kick_back_force;


        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfTime;
            transform.localPosition = Vector3.Lerp(startPos, kickbackPos, t);
            Vector3 player_kickback = -playerController.transform.forward * (kick_back_force / 100);
            playerController.Move(player_kickback);
            yield return null;
        }


        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / halfTime;
            transform.localPosition = Vector3.Lerp(kickbackPos, startPos, t);
            yield return null;
        }

        transform.localPosition = startPos;
        kickbackRoutine = null;
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
        is_left_click_held = false;
    }

    void Start()
    {


    }

    void Update()
    {
    }
}
