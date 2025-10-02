using UnityEngine;

public class Shotgun : MonoBehaviour, ICommon_Gun_Actions
{
    public Transform scopePosition;
    public Transform barrelDirection;
    public Transform grabPoint;

     public Transform Get_Grab_Point()
    {
        return grabPoint;
    }

    public Transform Get_Scope()
    {
        return scopePosition;
    }
    
    public Transform Get_Barrel_Direction()
    {
        return barrelDirection;
    }
    public void Fire()
    {
        Debug.Log("SHOTGUN FIRE");
    }

    public void Scope_in()
    {
        Debug.Log("SHOTGUN SCOPE IN");

    }

    public void Scope_out()
    {
        Debug.Log("SHOTGUN SCOPE out");

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


}
