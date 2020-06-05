using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("References:")]
    [SerializeField]
    private AudioSource source;

    [SerializeField]
    private Animator digitalAnim;

    [SerializeField]
    private Text timerFront;

    [SerializeField]
    private Text timerBack;

    [SerializeField]
    private Transform clockArmHinge;

    [SerializeField]
    private Image fill_CleanAmount;

    [SerializeField]
    private Image fill_CleanAmount_OtherTeam;

    [SerializeField]
    private Animator anim_OpponentProgressBar;

    [SerializeField]
    private Animator anim_OurProgressBar;

    #region ### Private Variables ###
    private const string BOOL_DISPLAYOPPONENTPROGRESS = "Popup";

    private Color fill_StartColor;
    #endregion

    private void OnEnable()
    {
        fill_StartColor = fill_CleanAmount.color;

        HouseManager.OnTimeChanged += UpdateTimer;
        HouseManager.OnCleaningProgressionVisualChanged += UpdateCleaningProgressionUI;
        HouseManager.OnFinishedCountdown += DisplayCleaningProgressionUI;
    }

    private void OnDestroy() 
    {
        HouseManager.OnTimeChanged -= UpdateTimer;
        HouseManager.OnCleaningProgressionVisualChanged -= UpdateCleaningProgressionUI;
        HouseManager.OnFinishedCountdown -= DisplayCleaningProgressionUI;
    }

    private void DisplayCleaningProgressionUI() 
    {
        const string BOOL_POPUP_NAME = "Popup";
        print(anim_OurProgressBar);
        anim_OurProgressBar.SetBool(BOOL_POPUP_NAME, true);
    }

    private void UpdateCleaningProgressionUI(int teamID) 
    {
        const string BOUNCE_TRIGGER_STRING = "Bounce";

        if (NetworkManager.localPlayerInformation.team == teamID)
        {
            fill_CleanAmount.fillAmount = HouseManager.CleanPercentage;
            fill_CleanAmount.color = Mathf.Approximately(fill_CleanAmount.fillAmount, 1) ? Color.green : fill_StartColor;
            anim_OurProgressBar.SetTrigger(BOUNCE_TRIGGER_STRING);
            return;
        }

        fill_CleanAmount_OtherTeam.fillAmount = HouseManager.Get_OtherTeamCleaningPercentage();
        fill_CleanAmount_OtherTeam.color = Mathf.Approximately(fill_CleanAmount_OtherTeam.fillAmount, 1) ? Color.green : fill_StartColor;
        anim_OpponentProgressBar.SetTrigger(BOUNCE_TRIGGER_STRING);
    }

    private int totalSeconds = 0;
    private bool clockSet = false;
    public void UpdateTimer(TimeSpan timeRemaining) 
    {
        totalSeconds = clockSet == false ? (int)timeRemaining.TotalSeconds : totalSeconds;
        clockSet = true;

        int ceiledTotalSeconds = Mathf.CeilToInt((float)timeRemaining.TotalSeconds);
        int minutes = 0;

        if (ceiledTotalSeconds >= 60)
        {
            minutes = Mathf.FloorToInt(ceiledTotalSeconds / 60f);
            ceiledTotalSeconds -= minutes * 60;
        }

        const int END_ROTATION = -360;
        const string DIGITALTIMER_TRIGGER = "BounceTimer";

        float second_based_rotation = 360f / totalSeconds;

        string timeLeftText = minutes.ToString() + ':' + (ceiledTotalSeconds < 10 ? 0.ToString() : "") + (ceiledTotalSeconds).ToString();

        if (timeRemaining.TotalSeconds < 6) 
        {
            timerBack.color = Color.red;
            digitalAnim.SetTrigger(DIGITALTIMER_TRIGGER);
            source.PlayOneShot(source.clip);
        }

        Vector3 rotation = Vector3.zero;

        rotation.z = float.IsNaN(-END_ROTATION + (second_based_rotation * (float)timeRemaining.TotalSeconds)) ? 0 : -END_ROTATION + (second_based_rotation * (float)timeRemaining.TotalSeconds);

        clockArmHinge.transform.localEulerAngles = rotation;

        timerFront.text = timeLeftText;
        timerBack.text = timeLeftText;
    }

    public void DisplayOpponentProgressbar(bool state) 
    {
        if(anim_OpponentProgressBar) 
        {
            if (anim_OpponentProgressBar.GetBool(BOOL_DISPLAYOPPONENTPROGRESS) != state) 
            {
                anim_OpponentProgressBar.SetBool(BOOL_DISPLAYOPPONENTPROGRESS, state);
            }
        }
    }
}
