using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class GameManager : NetworkBehaviour {

	
	static public GameManager instance { get { return _instance; } }
	static protected GameManager _instance;
	
	[SyncVar]
	public bool GameRunning;
	
	static public GamePlayer[] Players = new GamePlayer[2];
	
	public Text[] PlayerScoreText;
	public Text[] PlayerNameText;
	public GameEndPanel EndPanel;
	
	public void Awake()
	{
		_instance = this;
	}
	
	void Start()
	{
		//init any player pre added (happen on server hosted by client, were other player can connect before the scene was loaded)
		for (int i = 0; i < GamePlayer.player_objects.Count; ++i)
		{
			if (GamePlayer.player_objects[i] == null)
				continue;
			
			//thid is done in the OnStartLocalClient of the paddle too, but in some case, paddle is created before the scene
			//so there is no PongManager instance. Hence we need to do it here too to be sure that thing are setup properly
			PlayerNameText[i].text = GamePlayer.player_objects[i].playerName;
		}
	}
	
	public void CheckReady()
	{
		if (GameRunning)
			return;
		
		bool allReady = true;
		foreach (GamePlayer p in Players)
		{
			if (p == null)
				continue;
			
			allReady &= p.isReadyToPlay;
		}
		
		if (allReady)
		{
			GameRunning = true;
			
			ball.isFrozen = false;
			RpcToggleBall(true);
			ball.ResetBall(0);
			
			foreach (GamePlayer p in Players)
			{
				if (p == null)
					continue;
				
				p.score = 0;
				p.GetComponent<SimpleController>().enabled = true;
				p.RpcStartGame();
			}
		}
	}
	
	public const int winningScore = 5;
	public void CheckScores()
	{
		GamePlayer winningPaddle = null;
		foreach (GamePlayer p in Players)
		{
			if (p == null)
				continue;
			
			if (p.score >= winningScore)
			{
				winningPaddle = p;
				break;
			}
		}
		
		if (winningPaddle != null)
		{
			//Do RPC function to set objects as non-active on client here.
			
			GameRunning = false;
			
			foreach (GamePlayer p in Players)
			{
				if (p == null)
					continue;
				p.isReadyToPlay = false;
				//p.GetComponent<SimpleController>().enabled = false;
				p.RpcGameFinished( winningPaddle == p );
			}
		}
	}
	
	//this is used to assign all data per instance (e.g each paddle its score text etc..)
	[Server]
	static public void AddPlayer(GamePlayer player_obj)
	{
		Players[player_obj.number] = player_obj;
		player_obj.SpawnAt(player_obj.number);
		
		if (instance != null)
		{
			//add player can be called BEFORE the object is built (i.e. when a client is also server, another client can have
			//loaded its scene faster. So no instance exist yet. All palyer will be init in the awake fonction then
			//if that function is called after the Awake (and so when an instance exist), we still need to init the player, so better do it here.
		}
	}
}
