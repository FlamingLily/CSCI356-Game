using UnityEngine;

public class Revolver : MonoBehaviour, ICommon_Gun_Actions
{

    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;


    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;
    float lastFired = 0;

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
        
            if (Input.GetKey(KeyCode.X))
        {
            if (!is_left_click_held)
            {
                //launch bullet prefab from gun tip
                //fire once then flip boolean to true, preventing it from firing more than once per left click
                GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);
                projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce);
                Destroy(projectile, 2f);
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

    }

    // Update is called once per frame
    void Update()
    {

    }
}
