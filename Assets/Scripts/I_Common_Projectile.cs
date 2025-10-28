using UnityEngine;

//Common interface shared by all projectiles (Magic and Generic)
public interface I_Common_Projectile
{
    void Destroy_Bullet() { }

    void On_Enemy_Hit() { }

    void On_Effected_Hit(GameObject hit) { } //only implemented for Magic weapons i.e Freeze and Destroyer
}
