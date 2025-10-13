using UnityEngine;
using System.Collections;

public class Enemy_revolver : MonoBehaviour
{
    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;

    public float kick_back_force = 0.1f;
    public float kick_back_time = 0.1f;
    private Coroutine kickbackRoutine;

    Vector3 move = Vector3.zero;


    public CharacterController playerController;


    public GameObject projectilePrefab = null;
    public float launchForce = 32000.0f;

    public float bullet_damage;



    
    void Start()
    {
        barrelDirection = transform.Find("Shoot_Outwards");
        scopePosition = transform.Find("Scope_In");
        gun_grab_point = transform.Find("Grab_Point");
        launchForce = 32000f;
        kick_back_force = 0.1f;
        kick_back_time = 0.1f;
        Renderer rdr = GetComponent<Renderer>();
        rdr.material.color = new Color(0.55f, 0f, 0f, 1f);
    }
    public Transform Get_Grab_Point()
    {
        return gun_grab_point;
    }
    public Transform Get_Barrel_Direction()
    {
        return barrelDirection;
    }
    public void Fire()
    {
            GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);

            Generic_Bullet bullet_brains = projectile.GetComponent<Generic_Bullet>();
            if (bullet_brains != null)
            {
                bullet_brains.damage = bullet_damage;
                bullet_brains.Enemy_Tag = "Player";
                // bullet_brains.Effect_Tag = "";
            }

            projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);

            if (kickbackRoutine != null) StopCoroutine(kickbackRoutine);
            kickbackRoutine = StartCoroutine(Gun_Kick());
            // Debug.Log("HANDGUN FIRE");
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

    public void Cloak()
    {
        Renderer rdr = GetComponent<Renderer>();
        rdr.material.color = new Color(0.55f, 0f, 0f, 0.05f);
    }

    public void UnCloak()
    {
        Renderer rdr = GetComponent<Renderer>();
        rdr.material.color = new Color(0.55f, 0f, 0f, 1f);
    }
}
