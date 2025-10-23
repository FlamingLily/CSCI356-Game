using UnityEngine;

//Interface shared by all guns (Magic and Generic)
public interface ICommon_Gun_Actions
{

    void Fire(); //Fire weapon
    void Reload(); // Reload Weapon

    void Do_Magic(GameObject target); //Do Magic (implemented if gun is magic)

    void Scope_in(); //Scope in with gun
    void Scope_out(); //Scope out with gun
    Transform Get_Scope(); //return scope of gun (camera position for scoping in)
    Transform Get_Barrel_Direction(); //get barrel direction (direction to shoot bullets)
    Transform Get_Grab_Point(); //get grab point (where player holds gun object)

}
