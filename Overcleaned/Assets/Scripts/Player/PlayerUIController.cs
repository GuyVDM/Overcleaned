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

    #region ### Private Variables ###
    private const string BOOL_DISPLAYOPPONENTPROGRESS = "Popup";
    #endregion

    private void OnEnable()
    {
        HouseManager.OnTimeChanged += UpdateTimer;
        HouseManager.OnCleanableObjectStatusChanged += UpdateCleaningProgressionUI;
    }

    private void Update()
    {
        fill_CleanAmount.fillAmount = HouseManager.CleanPercentage;    
    }

    private void OnDestroy() 
    {
        HouseManager.OnTimeChanged -= UpdateTimer;
        HouseManager.OnCleanableObjectStatusChanged -= UpdateCleaningProgressionUI;
    }

    private void UpdateCleaningProgressionUI() 
    {
        fill_CleanAmount.fillAmount = HouseManager.CleanPercentage;
    }

    public void UpdateTimer(TimeSpan timeRemaining) 
    {
        const int END_ROTATION = -360;
        const float SECOND_BASED_ROTATION = 1.2f;

        string timeLeftText = timeRemaining.Minutes.ToString() + ':' + (timeRemaining.Seconds < 10 ? 0.ToString() : "") + timeRemaining.Seconds.ToString();

        Vector3 rotation = Vector3.zero;
        rotation.z = -END_ROTATION + (SECOND_BASED_ROTATION * (float)timeRemaining.TotalSeconds);

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
