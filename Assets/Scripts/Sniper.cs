using UnityEngine;

using System.Collections;

public class Sniper : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;

    public float kick_back_force;
    public float kick_back_time;

    private Vector3 originalLocalPos;
    private Coroutine kickbackRoutine;

    Vector3 move = Vector3.zero;


    public CharacterController playerController;


    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;
    float lastFired = 0;

    public float fire_rate;

    bool is_left_click_held = false;

    public float bullet_damage;



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

        if (!is_left_click_held && Time.time >= lastFired + fire_rate)
        {
            GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);


            Generic_Bullet bullet_brains = projectile.GetComponent<Generic_Bullet>();
            if (bullet_brains != null)
            {
                bullet_brains.damage = bullet_damage;
                bullet_brains.Enemy_Tag = "Enemy";
                // bullet_brains.Effect_Tag = "";
            }

        projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);

            if (kickbackRoutine != null) StopCoroutine(kickbackRoutine);
            kickbackRoutine = StartCoroutine(Gun_Kick());
            lastFired = Time.time;
            //Debug.Log("sniper FIRE");
            is_left_click_held = true;
        }
        else
        {
            is_left_click_held = false;
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
        Debug.Log("REVOLVER SCOPE IN");
    }

    public void Scope_out()
    {
        Debug.Log("revolver SCOPE out");

    }


    public void Reload()
    {
        is_left_click_held = false;
    }

    void Start()
    {


    }
      public void Do_Magic(GameObject target)
    {
        
    }


    void Update()
    {
    }

}
