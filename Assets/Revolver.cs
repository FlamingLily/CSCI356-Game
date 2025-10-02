using UnityEngine;

public class Revolver : MonoBehaviour, ICommon_Gun_Actions
{

    public Transform gun_grab_point;

    public Transform scopePosition;

    public Transform barrelDirection;



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
}
