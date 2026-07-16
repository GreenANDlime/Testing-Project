using System.Collections.Generic;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance;

    private Dictionary<int, PlayerData> players = new Dictionary<int, PlayerData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            // Keeps this manager alive when scenes change.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Prevents duplicate managers.
            Destroy(gameObject);
        }
    }

    public void AddPlayer(int playerID, string playerName)
    {
        if (!players.ContainsKey(playerID))
        {
            PlayerData newPlayer = new PlayerData(playerID, playerName);

            players.Add(playerID, newPlayer);

            Debug.Log("Added player: " + playerName);
        }
    }

    public PlayerData GetPlayerData(int playerID)
    {
        if (players.TryGetValue(playerID, out PlayerData data))
        {
            return data;
        }

        Debug.LogWarning("No player found with ID: " + playerID);
        return null;
    }

    public void RemovePlayer(int playerID)
    {
        players.Remove(playerID);
    }
}