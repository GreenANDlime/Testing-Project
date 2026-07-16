[System.Serializable]
public class PlayerData
{
    public int playerID;
    public string playerName;

    public float averageSpeed;
    public float totalDistance;
    public float totalMovementTime;
    public float currentSpeed;

    public PlayerData(int id, string name)
    {
        playerID = id;
        playerName = name;
    }

    public void CalculateAverageSpeed()
    {
        if (totalMovementTime > 0f)
        {
            averageSpeed = totalDistance / totalMovementTime;
        }
    }

}
