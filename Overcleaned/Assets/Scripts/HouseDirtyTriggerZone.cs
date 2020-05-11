using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class HouseDirtyTriggerZone : MonoBehaviourPunCallbacks
{
    public static float PenaltyTeam1 { get; private set; }
    public static float PenaltyTeam2 { get; private set; }

    public enum TeamID 
    {
        Team1 = 0,
        Team2 = 1
    }

    [Header("House Options:")]
    [SerializeField]
    private TeamID currentTeam;

    public float debugCurrentPenaltyAmount;

    #region ### Private Variables
    private int poopMask = 1 << 13;

    private int previousCount = 0;

    private const float PENALTY_PER_POOP = 40;

    private BoxCollider houseTriggerArea;
    #endregion


    private void Awake() 
    {
        houseTriggerArea = GetComponent<BoxCollider>();
    }

    private void Update() 
    {
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.IsConnected == false)
        {
            Collider[] allPoop = Physics.OverlapBox(transform.position, houseTriggerArea.size / 2, Quaternion.identity, poopMask);

            if (allPoop.Length != previousCount)
            {
                Set_HousePenalty(allPoop.Length * PENALTY_PER_POOP);
                previousCount = allPoop.Length;

                HouseManager.InvokeOnObjectStatusCallback((int)currentTeam);
            }
        }
    }

    [PunRPC]
    private void Stream_HousePenalty(float currentPenalty) 
    {
        debugCurrentPenaltyAmount = currentPenalty;

        switch(currentTeam) 
        {
            case TeamID.Team1:
                PenaltyTeam1 = currentPenalty;
                return;

            case TeamID.Team2:
                PenaltyTeam2 = currentPenalty;
                return;
        }
    } 

    private void Set_HousePenalty(float currentPenalty) 
    {
        if(NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_HousePenalty), RpcTarget.AllBuffered, currentPenalty);
            return;
        }

        Stream_HousePenalty(currentPenalty);
    }
}
