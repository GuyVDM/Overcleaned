using UnityEngine;

public class StaticAssignedHousePositions : MonoBehaviour
{
    private static Vector3 Team1HousePos { get; set; } = Vector3.zero;
    private static Vector3 Team2HousePos { get; set; } = Vector3.zero;

    public enum TeamID 
    {
        Team1,
        Team2
    }

    [Header("House Team Owner:")]
    [SerializeField]
    private TeamID owningTeam;

    private void Start() 
    {
        switch(owningTeam) 
        {
            case TeamID.Team1:
                Team1HousePos = transform.position;
                break;

            case TeamID.Team2:
                Team2HousePos = transform.position;
                break;
        }

        Destroy(gameObject);
    }

    public static Vector3 Get_EnemyHousePosition() 
    {
        switch (NetworkManager.localPlayerInformation.team)
        {
            case 0:
                return Team2HousePos;

            case 1:
                return Team1HousePos;

            default:
                return Vector3.zero;
        }
    }
}
