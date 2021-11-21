using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Linq;
using Mirror;

public class scrGameManager : NetworkBehaviour
{
    public static scrGameManager Instance { get; private set; } = null;

    [SyncVar]
    private int iScore = 0;
    public static int GetScore() { return Instance.iScore; }
    public static void AddScore(int _iScore){ Instance.iScore += _iScore; ScoreChanged.Invoke(); }
    public static void SetScore(int _iScore){ Instance.iScore = _iScore; ScoreChanged.Invoke(); }

    public static UnityEvent ScoreChanged { get; private set; } = new UnityEvent();

    private ChromaticAberration CA;

    private VolumeProfile PostProcess;

    private AudioSource AudioSRC;

    [SyncVar]
    private bool bWonGame = false;
    public bool GetWonGame() { return bWonGame; }

    [SerializeField]
    private GameObject VictoryText;

    [SerializeField]
    private GameObject OriginalStart;

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

        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();

        // Destroy original start so that new players don't spawn there
        Destroy(OriginalStart);
    }

    // Update is called once per frame
    void Update()
    {
        // Constantly ensure chromatic aberration is reset
        CA.intensity.value = Mathf.Lerp(CA.intensity.value, 0f, 0.1f);

        // Reset game
        if (Input.GetKeyDown(KeyCode.Space) && bWonGame) ResetGameCallback();

        if (!isServer) return;
        // Check for remaining blocks
        if (scrBlock.lAllBlocks.Where(b => b.GetEnabled()).Count() == 0 && !bWonGame)
        {
            WonGameCallback();
        }

    }
    // Victory sequence
    [ClientRpc]
    public void WonGameCallback()
    {
        bWonGame = true;

        // Scale victory text up
        VictoryText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutBack);

        // Ball handling
        foreach (scrBall _ball in scrBall.Balls)
        {
            // Play death fizzle
            GameObject NewFizzle = Instantiate<GameObject>(_ball.DeathFizzle, _ball.transform.position, Quaternion.identity);

            // Disable ball
            _ball.gameObject.SetActive(false);
        }


        // Play sound
        AudioSRC.PlayOneShot(Resources.Load<AudioClip>("Audio/Victory"));
    }

    [ClientRpc]
    public void ChromaticEffect()
    {
        Instance.CA.intensity.value = 1f;
    }

    [Command(requiresAuthority = false)]
    public void ResetGameCallback()
    {
        ResetGame();
    }

    [ClientRpc]
    private void ResetGame()
    {
        // Reset won bool
        bWonGame = false;

        // Activate all blocks
        scrBlock.lAllBlocks.ForEach(b => b.SetEnabled(true));

        // Reset balls
        scrBall.Balls.ForEach(b => { b.gameObject.SetActive(true); b.NewGameReset(); });

        // Reset score
        SetScore(0);

        // Scale victory text down
        VictoryText.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack);

        // Play sound
        AudioSRC.PlayOneShot(Resources.Load<AudioClip>("Audio/Reset"));
    }

}
