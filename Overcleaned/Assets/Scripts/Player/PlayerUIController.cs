using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("References:")]
    [SerializeField]
    private Text timerFront;

    [SerializeField]
    private Text timerBack;

    [SerializeField]
    private Transform clockArmHinge;

    [SerializeField]
    private Image fill_CleanAmount;

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
        HouseManager.OnCleanableObjectStatusChanged += UpdateCleaningProgressionUI;
    }

    private void OnDestroy() 
    {
        HouseManager.OnTimeChanged -= UpdateTimer;
        HouseManager.OnCleanableObjectStatusChanged -= UpdateCleaningProgressionUI;
    }

    private void UpdateCleaningProgressionUI() 
    {
        const string BOUNCE_TRIGGER_STRING = "Bounce";

        fill_CleanAmount.fillAmount = HouseManager.CleanPercentage;

        if(Mathf.Approximately(fill_CleanAmount.fillAmount, 1)) 
        {
            fill_CleanAmount.color = Color.green;
        }
        else 
        {
            fill_CleanAmount.color = fill_StartColor;
        }

        anim_OurProgressBar.SetTrigger(BOUNCE_TRIGGER_STRING);
    }

    private int totalSeconds = 0;
    private bool clockSet = false;
    public void UpdateTimer(TimeSpan timeRemaining) 
    {
        totalSeconds = clockSet == false ? (int)timeRemaining.TotalSeconds : totalSeconds;
        clockSet = true;

        const int END_ROTATION = -360;
        float second_based_rotation = 360f / totalSeconds;

        string timeLeftText = timeRemaining.Minutes.ToString() + ':' + (timeRemaining.Seconds < 10 ? 0.ToString() : "") + timeRemaining.Seconds.ToString();

        Vector3 rotation = Vector3.zero;
        rotation.z = -END_ROTATION + (second_based_rotation * (float)timeRemaining.TotalSeconds);

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
