using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scrFirework : MonoBehaviour
{
    [SerializeField]
    private AudioSource AudioSRC;

    // Start is called before the first frame update
    void Start()
    {
        // Get a random number
        int iRandom = Mathf.Clamp(Random.Range(0, 3),0,2);

        // Load the corresponding firework sound
        AudioClip FireworkClip = Resources.Load<AudioClip>("Audio/Firework" + iRandom.ToString());

        // Play it
        AudioSRC.PlayOneShot(FireworkClip);
    }


}
