using UnityEngine;
using System.Collections;
//Freezing Magic Gun
public class Magic : MonoBehaviour, ICommon_Gun_Actions //extends Common Gun Actions interface
{
    public Transform gun_grab_point; //where gun is held by player

    public Transform scopePosition; //camera position for scope in

    public Transform barrelDirection; //barrel direction of gun

    public float kick_back_force; //distance gun 'kicks-back' player when fired
    public float kick_back_time; //kick-back animation time

    private Coroutine kickbackRoutine; //kick-back animation

    public Material Frozen; //On-Effected-Hit Material

    public CharacterController playerController; //Player controller

    public GameObject projectilePrefab = null; //prefab of fired bullet (Magic Bullet)
    public float launchForce = 0.0f; //launch force of gun
    float lastFired = 0; //tick when gun was last fired

    public float fire_rate; //how rapidly gun can fire

    bool is_left_click_held = false; //is left click being held? boolean

    public float bullet_damage; //damage points inflicted by fired bullet

            public AudioSource player_audio_source; //player audio source

public AudioClip fire_audio; //firing audio clip
    public AudioClip scope_in_audio; //scope in sound
    public AudioClip scope_out_audio; //scope out sound
    public AudioClip on_effect_audio; //effect-hit sound

    public Transform Get_Grab_Point() //return grab point of gun
    {
        return gun_grab_point;
    }
    public Transform Get_Barrel_Direction() //return barrel direction (position and direction in which bullets are fired from)
    {
        return barrelDirection;
    }
    public Transform Get_Scope() //get scope position
    {
        return scopePosition;
    }
    public void Fire() //On gun fire
    {
        if (Input.GetKey(KeyCode.V)) //On firing button click
        {
            if (!is_left_click_held && Time.time >= lastFired + fire_rate) //if left click is not being held, and gun is able to be fired 
            //i.e gun can only be fired every time firing button is held, then released
            //gun will not fire continously if firing button is held and not released
            {
                GameObject projectile = Instantiate(projectilePrefab, barrelDirection.position, projectilePrefab.transform.rotation); //instantiate bullet prefab (Magic Bullet)
                projectile.GetComponent<Renderer>().material = Frozen; //set bullet material to given effect material
                Magic_Bullet bullet_brains = projectile.GetComponent<Magic_Bullet>(); //get bullet script
                bullet_brains.shooter = this.gameObject;
                if (bullet_brains != null) //if bullet behaviour exists
                {
                    bullet_brains.damage = bullet_damage; //damage inflicted by bullet
                    bullet_brains.Enemy_Tag = "Enemy"; //tagged enemies effected by bullet
                    bullet_brains.Effect_Tag = "Freezable"; //tagged objects effected by bullet
                }

                projectile.GetComponent<Rigidbody>().AddForce(barrelDirection.forward * launchForce); //propel bullet
                player_audio_source.PlayOneShot(fire_audio, 1); //play firing audio

                if (kickbackRoutine != null) StopCoroutine(kickbackRoutine); //start gun kickback animation
                kickbackRoutine = StartCoroutine(Gun_Kick());
                lastFired = Time.time; //reset last time fired
                is_left_click_held = true;
            }
            else
            {
                is_left_click_held = false;
            }

        }
    }

    public void Do_Magic(GameObject target) //Do Magic, called on collision with effectable objects and enemies
    {
        if (target.CompareTag("Enemy")) //if enemy
        {
            AIBehaviour ai_script = target.GetComponent<AIBehaviour>(); //get enemy behaviour
            target.GetComponent<Renderer>().material = Frozen; //set enemy material to 'Frozen'
            ai_script.moveSpeed = 0.75f; //modify enemy move speed
        }
        else //if not enemy
        {
            target.GetComponent<Renderer>().material = Frozen; //change collision objects material
            MonoBehaviour script = target.GetComponent<MonoBehaviour>(); //get collision behaviour
            if (script != null && !target.CompareTag("Enemy")) //if not enemy (i.e Hurter)
            {
                script.enabled = false; //disable object script
            }
            Animator collidedAnimator = target.gameObject.GetComponent<Animator>(); //get collision object animator
            if (collidedAnimator != null) //if has animation
            {
                collidedAnimator.speed = 0.25f; //slow animation speed
            }
        }
        player_audio_source.PlayOneShot(on_effect_audio, 1); //play 'effected object hit' sound
    }

    IEnumerator Gun_Kick() //Gun Kickback animation
    {
        float halfTime = kick_back_time / 2f; //half way point of animation

        Vector3 startPos = transform.localPosition; //start position is current position
        Vector3 kickbackPos = startPos + Vector3.back * kick_back_force; //kicked-back position is offsetted current position


        float t = 0f; //animation time
        while (t < 1f) //if animation is less than half time (in kick back portion of animation)
        {
            t += Time.deltaTime / halfTime;
            transform.localPosition = Vector3.Lerp(startPos, kickbackPos, t); //Lerp move position of gun
            Vector3 player_kickback = -playerController.transform.forward * (kick_back_force / 100); //move player contorller bacl
            playerController.Move(player_kickback);
            yield return null;
        }


        t = 0f;
        while (t < 1f) //if animation is more than half time (return to original position)
        {
            t += Time.deltaTime / halfTime;
            transform.localPosition = Vector3.Lerp(kickbackPos, startPos, t); //Lerp move back to current position
            yield return null;
        }

        transform.localPosition = startPos; //reset position
        kickbackRoutine = null; //end rountine
    }

  public void Scope_in() //on gun scope in
    {
        player_audio_source.PlayOneShot(scope_in_audio, 1); //play scope in noise
    }

    public void Scope_out() //on scope out
    {
        player_audio_source.PlayOneShot(scope_out_audio, 1); //play scope out noise
    }


    public void Reload() //when firing button is released, gun is 'reloaded' (left click is not longer being held)
    {
        is_left_click_held = false;
    }

    void Start()
    {


    }

    void Update()
    {
    }
}
