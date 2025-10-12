using UnityEngine;

public class Hurter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public float hurt_tick_speed; //how often it can hurt you
    public int damage_points;
    public bool insta_kill;

    private float last_hurt_tick = 0f;
    void Start()
    {

    }

    public void Hurt(Player_Movement player)
    {
            Debug.Log("IN HURT");
        if (Time.time >= last_hurt_tick + hurt_tick_speed)
        {

            last_hurt_tick = Time.time;
            player.Loose_Health_Points(damage_points, hurt_tick_speed);
            player.player_audio_source.PlayOneShot(player.hurter_audio, 0.5f);
            Debug.Log("HURT HURT");
        }
        if (insta_kill)
        {
            player.Die();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
