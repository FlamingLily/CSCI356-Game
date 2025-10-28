using UnityEngine;

//Objects that can inflict damage on the player
public class Hurter : MonoBehaviour
{
    public float hurt_tick_speed; //how often the player can be hurt in a time period
    public int damage_points; //damage inflicted per hurt tick
    public bool insta_kill; //if Hurter insta-kills player

    private float last_hurt_tick = 0f; //last time the player was hurt
//on collision between player and Hurter
    public void Hurt(Player_Movement player)
    {
        if (Time.time >= last_hurt_tick + hurt_tick_speed) //if the last time the hurter damaged the player is more than the hurt tick
        {
            last_hurt_tick = Time.time; //reset last hurt tick
            player.Loose_Health_Points(damage_points, hurt_tick_speed); //hurt player
            player.player_audio_source.PlayOneShot(player.hurter_audio, 0.5f); //play player hurting sound
        }
        if (insta_kill) //if hurter is Insta-kill
        {
            player.Die(); //kill player
        }
    }

    // Update is called once per frame

}
