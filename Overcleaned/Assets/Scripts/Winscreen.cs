using Photon.Chat.UtilityScripts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Winscreen : MonoBehaviour
{
    [SerializeField]
    private Image whitescreen;

    [SerializeField]
    private Animator winscreenAnimator;

    [SerializeField]
    private GameObject statisticsObject;

    [SerializeField]
    private GameObject menuButton;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip[] clips;

    [Header("Statistic Settings:")]
    [SerializeField]
    private TextMeshProUGUI result;

    [SerializeField]
    private Text percentageYourTeam;

    [SerializeField]
    private Text percentageOpponentTeam;

    [SerializeField]
    private Text percentageDifference;

    [SerializeField]
    private Image fillbarYourTeam;

    [SerializeField]
    private Image fillbarOpponentTeam;

    private void Start() 
    {
        ServiceLocator.GetServiceOfType<PlayerManager>().Set_PlayerLockingstate(true);
        audioSource.PlayOneShot(clips[4]);

        StartCoroutine(StartUILoop());
    }

    private IEnumerator StartUILoop() 
    {
        const string TRIGGER_STRING = "BounceIn";

        Color screenColor = Color.white;
        screenColor.a = whitescreen.color.a;

        while(screenColor.a < 1) 
        {
            screenColor.a += Time.deltaTime / 2;
            whitescreen.color = screenColor;
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1.3f);

        winscreenAnimator.enabled = true;
        winscreenAnimator.SetTrigger(TRIGGER_STRING);

        yield return new WaitForSeconds(1.5f);

        statisticsObject.SetActive(true);
        audioSource.PlayOneShot(clips[0]);

        yield return new WaitForSeconds(2.5f);

        float finalCleaningprogressionOpponent = HouseManager.Get_OtherTeamCleaningPercentage();
        float finalCleaningprogressionYours = HouseManager.Get_CleanPercentage();

        Debug.Log(finalCleaningprogressionOpponent + " is other team.");
        Debug.Log(finalCleaningprogressionYours + "is our team.");

        audioSource.PlayOneShot(clips[1]);
        audioSource.Play();

        while(finalCleaningprogressionYours > fillbarYourTeam.fillAmount || finalCleaningprogressionOpponent > fillbarOpponentTeam.fillAmount) 
        {
            fillbarYourTeam.fillAmount += Time.deltaTime / 8;
            fillbarOpponentTeam.fillAmount += Time.deltaTime / 8;

            fillbarYourTeam.fillAmount = Mathf.Clamp(fillbarYourTeam.fillAmount, 0, finalCleaningprogressionYours);
            fillbarOpponentTeam.fillAmount = Mathf.Clamp(fillbarOpponentTeam.fillAmount, 0, finalCleaningprogressionOpponent);

            float highestValue = fillbarYourTeam.fillAmount > fillbarOpponentTeam.fillAmount ? fillbarYourTeam.fillAmount : fillbarOpponentTeam.fillAmount;
            float lowestValue = fillbarYourTeam.fillAmount < fillbarOpponentTeam.fillAmount ? fillbarYourTeam.fillAmount : fillbarOpponentTeam.fillAmount;

            percentageDifference.text = Mathf.RoundToInt((highestValue - lowestValue) * 100).ToString() + '%';

            Debug.Log("Filling the fillamount");

            percentageYourTeam.text = Mathf.RoundToInt(fillbarYourTeam.fillAmount * 100f).ToString() + '%';
            percentageOpponentTeam.text = Mathf.RoundToInt(fillbarOpponentTeam.fillAmount * 100f).ToString() + '%';

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(clips[1].length + 1);

        const string winMessage = "Congratulations, your team has won!";
        const string loseMessage = "Better luck next time.";
        const string tiedMessage = "How'd you even ti.. i mean good job!";

        result.gameObject.SetActive(true);
        result.text = finalCleaningprogressionOpponent < finalCleaningprogressionYours ? winMessage : finalCleaningprogressionYours == finalCleaningprogressionOpponent ? tiedMessage : loseMessage;

        AudioClip toPlay = finalCleaningprogressionOpponent < finalCleaningprogressionYours ? clips[2] : finalCleaningprogressionOpponent == finalCleaningprogressionYours ? clips[2] : clips[3];

        if(toPlay == clips[2]) 
        {
            audioSource.PlayOneShot(clips[5]);
        }

        audioSource.PlayOneShot(toPlay);


        yield return new WaitForSeconds(4);
        menuButton.SetActive(true);
    }
}
