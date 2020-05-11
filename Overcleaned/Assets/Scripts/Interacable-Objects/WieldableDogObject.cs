using System;
using System.Collections;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class WieldableDogObject : WieldableObject
{
    private PlayerManager playerManager;

    [SerializeField]
    private int secondsTillExplosion = 15;

    [SerializeField]
    private Animator countdownAnimator;

    [SerializeField]
    private Text countdownText;

    [SerializeField]
    private Light pointLight;

    #region ### Private Variables
    private int secondsPassed = 0;
    #endregion


    protected override void Awake() 
    {
        base.Awake();

        countdownText.text = secondsTillExplosion.ToString();
        StartCoroutine(ExplodeTimer());
        countdownText.color = Color.green;
    }

    private void Update() 
    {
        pointLight.intensity = Mathf.Lerp(pointLight.intensity, secondsPassed, 2 * Time.deltaTime);
    }

    private IEnumerator ExplodeTimer() 
    {
        while(secondsTillExplosion > 0) 
        {
            const string TRIGGER = "Bounce";

            yield return new WaitForSeconds(1);
            secondsTillExplosion--;
            countdownText.text = secondsTillExplosion.ToString();
            countdownAnimator.SetTrigger(TRIGGER);

            if(secondsTillExplosion == 10) 
            {
                countdownText.color = Color.yellow;
            }

            if (secondsTillExplosion == 3)
            {
                countdownText.color = Color.red;
            }

            secondsPassed++;
        }

        Set_ExplosionParticles();
    }

    [PunRPC]
    private void Stream_ExplosionParticles() 
    {
        const string EXPLOSION_VFX = "Explosion_VFX";
        const string POOP_OBJ_NAME = "Poo [Interactable]";

        Instantiate(Resources.Load(EXPLOSION_VFX) as GameObject, transform.position, Quaternion.identity);

        playerManager = ServiceLocator.GetServiceOfType<PlayerManager>();

        if(playerManager.player_InteractionController.currentlyWielding == this) 
        {
            WieldableObject toDrop = playerManager.player_InteractionController.currentlyWielding;

            playerManager.player_Controller.StunPlayer(5);
            playerManager.player_InteractionController.DropObject(toDrop);
        }

        if (PhotonNetwork.IsMasterClient) 
        {
            PhotonNetwork.InstantiateSceneObject(POOP_OBJ_NAME, transform.position, Quaternion.identity);
            PhotonNetwork.Destroy(photonView);
        }
    }

    private void Set_ExplosionParticles() 
    {
        if (NetworkManager.IsConnectedAndInRoom) 
        {
            photonView.RPC(nameof(Stream_ExplosionParticles), RpcTarget.All);
            return;
        }

        Stream_ExplosionParticles();
    }
}
