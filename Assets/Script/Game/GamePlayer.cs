using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayer : MonoBehaviour
{
    //Variables
    public Vector2 upForce;
    private bool isDead;

    //Components
    private Rigidbody2D rb2d;
    private Animator anim;

    //Audio Stuff
    [Header("Audio Stuff")]
    AudioSource audioSource;
    public AudioClip[] audioClips;

    public ParticleSystem PointParticleSystem;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //If we are not dead, we can press
        if(!isDead)
        {
            //For Mouse, Keyboard, Touch
            if(Input.GetMouseButtonDown(0) ||
               Input.GetKeyDown(KeyCode.Space))
            {
                //Stop the object and push it by "upForce"
                rb2d.velocity = Vector2.zero;
                rb2d.AddForce(upForce);
               // anim.SetTrigger("Flap");
                PlaySound("Flap");
            }
        }
    }

    //when an incoming collider makes contact with this object's collider (2D physics only).
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Stop the object
        rb2d.velocity = Vector2.zero;
        isDead = true;
        anim.SetTrigger("Hit");

        //Set it to Game Over
        GameController.instance.GameOver();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PointParticleSystem.transform.position = this.transform.position;
        PointParticleSystem.Play();
    }

    public void PlaySound(string type)
    {
        /*AudioClip clip = audioClips[0];
        switch(type)
        {
            case "Point":
                clip = audioClips[0];
                break;
            case "Flap":
                clip = audioClips[1];
                break;
            case "Die":
                clip = audioClips[2];
                break;
            case "Hit":
            default:
                clip = audioClips[3];
                break;
        }
        audioSource.PlayOneShot(clip);*/
    }
}
