using System.Diagnostics;
using UnityEngine;

public class DetectNoise : MonoBehaviour
{

    private float xPos;
    private float zPos;
    private bool noiseDetected;
    private Vector3 noiseTarget;
    private Vector3 wanderTarget;

    [Header("Player Layermask")]
    public LayerMask targetLayer; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        xPos = transform.position.x;
        zPos = transform.position.z;
        noiseDetected = false;
    }

    // Update is called once per frame
    void Update()
    {
        MoveAround();
        UnityEngine.Debug.Log(SearchAround());
    }

    private Vector3 RandomizePosition()
    {
        if(transform.position.x == xPos && transform.position.z == zPos){
            xPos = Random.Range(-25, -5);
            zPos = Random.Range(0, 20);
        }

        Vector3 targetPosition = new Vector3(xPos, transform.position.y, zPos);

        return targetPosition;
    }
    private void Wander(Vector3 targetPosition)
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 2.5f);
    }

    private void MoveAround()
    {
        var searchResult = SearchAround();

        if (searchResult.detection)
        {
            noiseDetected = true;
            noiseTarget = searchResult.targetPos;
        }

        if (noiseDetected)
        {
            Wander(noiseTarget);

            if (Vector3.Distance(transform.position, noiseTarget) < 0.5f)
            {
                noiseDetected = false;
                wanderTarget = RandomizePosition();
                noiseTarget.y = transform.position.y;
            }

            return;
        }
        if (Vector3.Distance(transform.position, wanderTarget) < 0.5f)
        {
            wanderTarget = RandomizePosition();
            wanderTarget.y = transform.position.y;
        }

        Wander(wanderTarget);
    }

    private (Vector3 targetPos, bool detection) SearchAround()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f, targetLayer);

        foreach (var entity in hitColliders)
        {
            if (entity.transform.parent == null) continue;

            Transform parentTransform = entity.transform.parent;
            Transform specificChild = parentTransform.Find("SoundPlayer");

            if (specificChild == null) continue;

            PlaySound playSound = specificChild.GetComponent<PlaySound>();

            if (playSound == null) continue;

            if (playSound.ReturnVolume() > 0.6f)
            {
                return (parentTransform.position, true);
            }
        }

        return (Vector3.zero, false);
    }
}
