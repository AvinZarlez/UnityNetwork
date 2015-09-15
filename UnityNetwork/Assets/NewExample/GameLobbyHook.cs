using UnityEngine;
using System.Collections;

//Needs to refernece LobbyHook
using UnityStandardAssets.Network;

public class GameLobbyHook : LobbyHook
{
	public override void OnLobbyServerSceneLoadedForPlayer(UnityEngine.Networking.NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer)
	{
		LobbyPlayer l = lobbyPlayer.GetComponent<LobbyPlayer>();
			
		GamePlayer player_obj = gamePlayer.GetComponent<GamePlayer>();
		player_obj.number = l.slot;
		player_obj.color = l.playerColor;
		player_obj.playerName = l.playerName;
			
		GameManager.AddPlayer(player_obj);
	}
}