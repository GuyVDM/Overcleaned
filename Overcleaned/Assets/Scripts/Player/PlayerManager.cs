﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerManager : MonoBehaviourPunCallbacks, IServiceOfType
{
    [Header("References:")]
    public PlayerController player_Controller;

    public PlayerCameraController player_CameraController;

    public PlayerInteractionController player_InteractionController;

    [Header("Visual References:")]
    [SerializeField]
    private MeshRenderer player_Body;

    #region ### Service Locator Snippet ###
    public void OnInitialise() => ServiceLocator.TryAddServiceOfType(this);

    public void OnDeinitialise() => ServiceLocator.TryRemoveServiceOfType(this);
    #endregion

    #region ### RPC Calls ###
    [PunRPC]
    private void Stream_PlayerColorOverNetwork(float alpha, float blue, float green, float red) 
    {
        player_Body.material.color = new Color()
        {
            a = alpha,
            b = blue,
            g = green,
            r = red
        };
    }
    #endregion

    private void Awake()
    {
        if (photonView.IsMine || PhotonNetwork.IsConnected == false) 
        {
            OnInitialise();
            player_Controller.enabled = true;
            player_CameraController.enabled = true;
            player_InteractionController.enabled = true;

            return;
        }

        this.enabled = false;
    }

    private void OnDestroy() => OnDeinitialise();

    public void Set_EnemyBasePosition(Vector3 pos) => player_CameraController?.Set_EnemyBasePos(pos);

    public void Set_LockingStateOfPlayerController(bool state) => player_Controller.enabled = state;

    public void Set_PlayerColor(Color playerColor)
    {
        if (NetworkManager.IsConnectedAndInRoom)
        {
            photonView.RPC(nameof(Stream_PlayerColorOverNetwork), RpcTarget.AllBuffered, playerColor.a, playerColor.b, playerColor.g, playerColor.r);
            return;
        }

        Stream_PlayerColorOverNetwork(playerColor.a, playerColor.b, playerColor.g, playerColor.r);
    }
}
