using UnityEngine;

public interface ICommon_Gun_Actions
{

    void Fire();
    void Reload();

     void Do_Magic(GameObject target);

    void Scope_in(); //shift?
    void Scope_out(); //shift?

    /**
    Get Kickback
    Get Firerate
    **/

    Transform Get_Scope();
    Transform Get_Barrel_Direction();
    Transform Get_Grab_Point();

}
