using UnityEngine;

public class Magic : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;



    private CharacterController playerController;


    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;
    float lastFired = 0;

    public float fire_rate;

    bool is_left_click_held = false;



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
                projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);
                Destroy(projectile, 2f);


                lastFired = Time.time;
                Debug.Log("HANDGUN FIRE");
                is_left_click_held = true;
            }
        }
        else
        {
            is_left_click_held = false;
        }
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        playerController = GetComponentInParent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
    }
}
