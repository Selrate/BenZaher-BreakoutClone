using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class scrExplode : MonoBehaviour
{
    [SerializeField]
    private int iCubesPerAxis = 8;

    [SerializeField]
    private float fDelay = 1f, fForce = 300f, fRadius = 2f;

    private Renderer MainRenderer = null;

    private AudioSource AudioSrc;

    private scrBlock Block;

    // Start is called before the first frame update
    void Start()
    {
        MainRenderer = GetComponent<Renderer>();
        AudioSrc = GetComponent<AudioSource>();
        Block = GetComponent<scrBlock>();
    }

    public void StartExplosion()
    {

        // Create cube at fracture coordinates
        for(int x = 0; x < iCubesPerAxis; x++)
        {
            for(int y = 0; y < iCubesPerAxis; y++)
            {
                for(int z = 0; z < iCubesPerAxis; z++)
                {
                    CreateCube(new Vector3(x, y, z));
                }
            }
        }

        // Play audio
        PlayAudio();

        // Disable
        MainRenderer.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        Block.SetEnabled(false);
    }



    private void PlayAudio()
    {
        // Make sure audio source exists
        if (!AudioSrc) return;

        // Get a random number
        int iRandom = Mathf.Clamp(Random.Range(0, 7), 0, 6);

        // Load the corresponding block sound
        AudioClip BlockClip = Resources.Load<AudioClip>("Audio/Block" + iRandom.ToString());

        // Play it
        AudioSrc.PlayOneShot(BlockClip);
    }

    private void CreateCube(Vector3 _v3Pos)
    {
        // Create a new cube
        GameObject NewCube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        NewCube.gameObject.layer = LayerMask.NameToLayer("Particle");

        // Disable collider
        //NewCube.GetComponent<BoxCollider>().enabled = false;

        // Get it's renderer
        Renderer CubeRenderer = NewCube.GetComponent<Renderer>();

        // Set the material to the original renderer's material
        CubeRenderer.material = MainRenderer.material;

        // Set the scale to a fraction
        NewCube.transform.localScale = transform.localScale / iCubesPerAxis;

        // Get pos of first cube's position
        Vector3 v3FirstCubePos = transform.position - transform.localScale / 2 + NewCube.transform.localScale / 2;

        // Set pos to where it should be
        NewCube.transform.position = v3FirstCubePos + Vector3.Scale(_v3Pos, NewCube.transform.localScale);

        Rigidbody rb = NewCube.AddComponent<Rigidbody>();
        rb.AddForceAtPosition((NewCube.transform.position - transform.position).normalized * Random.Range(fForce*0.4f,fForce*1.8f),NewCube.transform.position);

        NewCube.transform.DOScale(Vector3.zero, 2f).SetEase(Ease.InBack).OnComplete(() => { Destroy(NewCube.gameObject); });
    }
}
