using UnityEngine;

public class PlayerStatisticsTracker : MonoBehaviour
{
    [SerializeField] public int playerID;
    [SerializeField] private string playerName;

    private Vector3 previousPosition;

    private void Start()
    {
        previousPosition = transform.position;

        PlayerDataManager.Instance.AddPlayer(playerID, playerName);
    }

    private void Update()
    {
        TrackMovement();
    }

    private void TrackMovement()
    {
        PlayerData data = PlayerDataManager.Instance.GetPlayerData(playerID);
        PlayerMovement movementData = GetComponent<PlayerMovement>(); // this will change depending on the name of the player movement script

        if (data == null)
        {
            return;
        }

        // Finds how far the player moved since the previous frame.
        float distanceMoved =
            Vector3.Distance(transform.position, previousPosition);

        data.totalDistance += distanceMoved;
        data.totalMovementTime += Time.deltaTime;
        data.currentSpeed = movementData.moveSpeed; // this maybe different on the player movement as "moveSpeed"

        data.CalculateAverageSpeed();

        // Saves the current position for the next frame.
        previousPosition = transform.position;
    }
}
