using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Linq;

public class scrGameManager : MonoBehaviour
{
    public static scrGameManager Instance { get; private set; } = null;


    private static int iScore = 0;
    public static int GetScore() { return iScore; }
    public static void AddScore(int _iScore){iScore += _iScore; ScoreChanged.Invoke(); }
    public static void SetScore(int _iScore){iScore = _iScore; ScoreChanged.Invoke(); }

    public static UnityEvent ScoreChanged { get; private set; } = new UnityEvent();

    private ChromaticAberration CA;

    private VolumeProfile PostProcess;

    private AudioSource AudioSRC;

    private bool bWonGame = false;
    public bool GetWonGame() { return bWonGame; }

    [SerializeField]
    private GameObject VictoryText;

    // Singleton
    private void Awake()
    {
        if (!Instance) Instance = this;
        else Destroy(gameObject);

        iScore = 0;
        ScoreChanged = new UnityEvent();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameObject GlobalVolume = GameObject.Find("PostProcess");
        if (GlobalVolume)
        {
            PostProcess = GlobalVolume.GetComponent<Volume>().profile;
            PostProcess.TryGet<ChromaticAberration>(out CA);
        }

        // Set text size to zero
        VictoryText.transform.localScale = Vector3.zero;

        // Get audio source
        AudioSRC = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Constantly ensure chromatic aberration is reset
        CA.intensity.value = Mathf.Lerp(CA.intensity.value, 0f, 0.1f);

        // Check for remaining blocks
        if(scrBlock.lAllBlocks.Where(b => b.GetEnabled()).Count() == 0 && !bWonGame)
        {
            bWonGame = true;

            // Scale victory text up
            VictoryText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack);

            // Play death fizzle
            GameObject NewFizzle = Instantiate<GameObject>(scrBall.Instance.DeathFizzle, scrBall.Instance.transform.position, Quaternion.identity);

            // Disable ball
            scrBall.Instance.gameObject.SetActive(false);

            // Play sound
            AudioSRC.PlayOneShot(Resources.Load<AudioClip>("Audio/Victory"));
        }

        // Reset game
        if (Input.GetKeyDown(KeyCode.Space) && bWonGame) ResetGame();
    }

    public static void ChromaticEffect()
    {
        Instance.CA.intensity.value = 1f;
    }

    private void ResetGame()
    {
        // Reset won bool
        bWonGame = false;

        // Activate all blocks
        scrBlock.lAllBlocks.ForEach(b => b.SetEnabled(true));

        // Reset ball
        scrBall.Instance.gameObject.SetActive(true);
        scrBall.Instance.NewGameReset();

        // Reset score
        SetScore(0);

        // Scale victory text down
        VictoryText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);

        // Play sound
        AudioSRC.PlayOneShot(Resources.Load<AudioClip>("Audio/Reset"));
    }

}
