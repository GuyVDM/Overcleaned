using Photon.Realtime;
using Photon.Pun;

public class LocalPlayerInformation
{
	public Player photonPlayer { get; private set; }
	public int team = -1;
	public int numberInTeam = -1;

	public LocalPlayerInformation(Player myPhotonPlayer)
	{
		photonPlayer = myPhotonPlayer;
	}
}
