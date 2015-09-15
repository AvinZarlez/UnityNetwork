using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class GamePlayer : NetworkBehaviour {
		
		// List of all the players kept within the player script itself,
		// because external managers may not exist when playerscript is created.
	static public List<GamePlayer> player_objects = new List<GamePlayer>();
		
		//Color is synced across clients so that each can color the player's game piece personally
		[SyncVar]
		public Color color;
		
		[SyncVar]
		public string playerName;
		
		//Are you player 1? 2? 27? This byte let's you know!
		[SyncVar]
		public byte number;
		
		//Used when a game is finished to wait for the player to get ready again or leave.
		[SyncVar]
		public bool isReadyToPlay;
		
		[SyncVar(hook = "OnScore")]
		public int score;
		
		public override void OnStartClient()
		{
			//When the client start, we set that panel to the proper color
			//as color is synced, all panels will have the proper color on all clients.
			//GetComponent<Renderer>().material.color = color;
			
			player_objects.Add(this);
			
			//sometime instance isn't yet define at that point (network synchronisation) so we check its existence first
			if(GameManager.instance != null)
				GameManager.instance.PlayerNameText[number].text = playerName;
		}
		
		public override void OnNetworkDestroy()
		{
			player_objects.Remove(this);
		}
		
		[ClientRpc]
		public void RpcGameFinished(bool winner)
		{
			//GetComponent<SimpleController>().enabled = false;
			if(isLocalPlayer)
				GameManager.instance.EndPanel.Display(winner ? "YOU WON" : "YOU LOST", SetReady, ExitToLobby);
		}
		
		[ClientRpc]
		public void RpcStartGame()
		{
			//GetComponent<SimpleController>().enabled = true;
			GameManager.instance.EndPanel.Hide();
		}
		
		public void ExitToLobby()
		{
			CmdExitToLobby();
		}
		
		[Command]
		public void CmdExitToLobby()
		{
			var lobby = NetworkManager.singleton as UnityStandardAssets.Network.LobbyManager;
			if (lobby != null)
			{
				lobby.ServerReturnToLobby();
			}
		}
		
		[ServerCallback]
		public void FixedUpdate()
		{

		}
		
		public void SetReady()
		{
			CmdSetReady();
		}
		
		public void Update()
		{
			if (!isLocalPlayer)
			{
				return;
			}
			
			float val = Input.GetAxis("Jump");
			
			if (val > 0.1f) 
			{
				//CmdFireBall();
			}
		}
		
		[Command]
		public void CmdSetReady()
		{
			isReadyToPlay = true;
			GameManager.instance.CheckReady();
		}
		
		[Command]
		public void CmdFireBall()
		{

		}
		
		// will be called on the client when score changes on the server
		public void OnScore(int newScore)
		{
			GameManager.instance.PlayerScoreText[number].text = newScore.ToString();
			score = newScore;
		}
		
		public void SpawnAt(byte slot)
		{
			//ineffective, but easier here. Real world scenario would cache the spawn points instead of finding them by name.
			transform.position = GameObject.Find("Spawn" + (slot == 0 ? "Left" : "Right")).transform.position;
			
			number = slot;
			isReadyToPlay = true;
		}
	}