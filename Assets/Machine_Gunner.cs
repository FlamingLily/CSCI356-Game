using UnityEngine;

public class Machine_Gunner : MonoBehaviour, ICommon_Gun_Actions
{

    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;

       float bullet_spray = 0.1f;

    public GameObject projectilePrefab = null;
    public float launchForce = 0.0f;
    float lastFired = 0;

    //how fast the gun can fire

    float fireRate = 0.05f;

    float kick_back = 0.0f;



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

        //prevents the machine gun from firing more than every 0.05f
        if (timeNow - lastFired >= fireRate)
        {
            //if last bullet left more that 0.05f ago, fire machine gun
            Shoot();
            lastFired = timeNow;
        }
        Debug.Log("REVOLVER FIRE");
    }

    public void Shoot()
    {
        Vector3 randomDirection = barrelDirection.forward +
                      new Vector3(
                          Random.Range(-bullet_spray, bullet_spray),
                          Random.Range(-bullet_spray, bullet_spray),
                          0f
                      );
        //make prefab of bullet                     
        GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation);
        //fire bullet using random jitter
        projectile.GetComponent<Rigidbody>().AddForce(randomDirection * launchForce);
        //after 2 seconds destroy bullet
        Destroy(projectile, 2f);
        Debug.Log("MACHINEGUN FIRE");

        //kickback
        //reload speed
    }

    public void Scope_in()
    {
        Debug.Log("REVOLVER SCOPE IN");
    }

    public void Scope_out()
    {
        Debug.Log("revolver SCOPE out");

    }

    public void Reload() { }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
}
