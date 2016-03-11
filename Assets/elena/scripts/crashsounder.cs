using UnityEngine;
using System.Collections;

public class crashsounder : MonoBehaviour {


    public AudioClip crashSound;
    private AudioSource source;
    
    void Awake (){

        source = GetComponent<AudioSource>();

    }

    void OnCollisionEnter(Collision collision)
    {
       
            source.PlayOneShot(crashSound);
    }
}
