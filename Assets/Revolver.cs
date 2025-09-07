using UnityEngine;

public class Revolver : MonoBehaviour, ICommon_Gun_Actions
{
    public void Fire()
    {
         Debug.Log("REVOLVER FIRE");
    }

    public void Scope_in() { }

    public void Reload(){}
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
