using UnityEngine;
using System.Collections.Generic;
using System.Collections;




public class NewNoiseDetection : MonoBehaviour
{
    public enum state
    {
        Listening,
        Investigating,
        Staring
    }
    public state monsterState;
    public float visibility;
    private Dictionary<string, int> scores = new Dictionary<string, int>();
    private Renderer objRenderer;
    private float lookDuration;
    private Vector3 lookTarget;
    [SerializeField] private List<Vector3> noiseLocations = new List<Vector3>();
    [SerializeField] private float duration;
    [Header("Layer Mask")]
    [SerializeField] private LayerMask target;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private LayerMask playerMask;

    [SerializeField] private AudioSource[] audiosSources;
    [SerializeField] private AudioSource noiseIndicator; // this is for testing remove it for the final script
    [SerializeField] private AudioClip audioFile; // this is for testing remove it for the final script
    [SerializeField] private bool noiseDetected; // this is for testing remove it for the final script
    [SerializeField] private float soundSensitivity;

    private Vector3 currentDir;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objRenderer = GetComponent<Renderer>();
        lookDuration = 5f;
        duration = 10f;
        currentDir = transform.forward;
    }

    void Update()
    {
        ManageStates();
        EntityDetection();
    }
    
    private bool DetectSound(float range)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, ~target);
        Debug.Log(hitColliders.Length);
        foreach(Collider hit in hitColliders)
        {
            Transform parent = hit.transform.parent;
            AudioSource[] audios = parent != null ? parent.GetComponentsInChildren<AudioSource>() : hit.GetComponentsInChildren<AudioSource>();

            foreach(AudioSource audio in audios){
                float distance = Vector3.Distance(transform.position, hit.transform.position);
                if(GetRMS(audio) > 0.6f && !Physics.Raycast(transform.position, (hit.transform.position - transform.position).normalized, distance, obstacleMask))
                {
                    noiseLocations.Clear();
                    noiseLocations.Add(hit.transform.position);
                    return true;
                }
            }
        }
        return false;
    }

    private bool Observe(Vector3 targetPos, float range)
    {
        Vector3 targetRotation = (targetPos - transform.position).normalized;

        currentDir = Vector3.RotateTowards(currentDir, targetRotation, 3f * Time.deltaTime, 0f);

        Vector3 leftDir = Quaternion.AngleAxis(-60f * 0.5f, Vector3.up) * currentDir;
        Vector3 rightDir = Quaternion.AngleAxis(60f * 0.5f, Vector3.up) * currentDir;

        float leftAngle = Vector3.SignedAngle(currentDir, leftDir, Vector3.up);
        float rightAngle = Vector3.SignedAngle(currentDir, rightDir, Vector3.up);

        Collider[] targets = Physics.OverlapSphere(transform.position, range, ~target);
        foreach(Collider hit in targets)
        {
            int sweepSteps = 20;
            for (int i = 0; i <= sweepSteps; i++)
            {
                float t = i / (float)sweepSteps;
                Vector3 dirToHit = Vector3.Slerp(leftDir, rightDir, t);

                float checkAngle = Vector3.SignedAngle(currentDir, dirToHit, Vector3.up);
                Debug.DrawRay(transform.position, dirToHit * 20f, Color.red); // This is for testing ONLY!!

                if (checkAngle >= leftAngle && checkAngle <= rightAngle)
                {
                    float distToHit = Vector3.Distance(transform.position, hit.transform.position);
                    Vector3 top = transform.position + Vector3.up * (objRenderer.bounds.size.y * 0.5f - 0.3f);
                    Vector3 bottom = transform.position - Vector3.up * (objRenderer.bounds.size.y * 0.5f - 0.3f);

                    bool blocked = Physics.CapsuleCast(top, bottom, 0.3f, dirToHit, out RaycastHit rayHit, distToHit, obstacleMask);
                    bool hittingPlayer = Physics.CapsuleCast(top, bottom, 0.3f, dirToHit, out RaycastHit targetInfo, distToHit, playerMask);
                    
                    if (!blocked && hittingPlayer && targetInfo.collider == hit)
                    {
                        Debug.Log("Monster sees player"); // this is for testing purposes
                        objRenderer.material.color = Color.red; // this is for testing purposes
                        noiseLocations.Clear();
                        noiseLocations.Add(hit.transform.position);
                        return true;
                    }
                    else objRenderer.material.color = Color.green; // this is for testing purposes
                    
                }
            }
        }


        Debug.DrawRay(transform.position, leftDir * 20f, Color.yellow); // This is for testing ONLY!!
        Debug.DrawRay(transform.position, rightDir * 20f, Color.yellow); // This is for testing ONLY!!
        return false;
    }

    private void UpdateLookDirection()
    {
        lookDuration = CountDown(lookDuration);
        if(lookDuration <= 0)
        {
            lookTarget = RandomizePosition();
            lookDuration = 5f;
        }
    }

    private void ManageStates()
    {
        if(monsterState == state.Listening)
        {
            UpdateLookDirection();
            if(DetectSound(20f) || Observe(lookTarget, 20f))
            {
                monsterState = state.Investigating;
                noiseIndicator.PlayOneShot(audioFile); // this is for testing remove it for the final script
            }
        }
        else if(monsterState == state.Investigating && !DetectSound(20f) && duration <= 0)
        {
            duration = 10f;
            monsterState = state.Listening;
        }
    }
    private float CountDown(float time)
    {
        time -= Time.deltaTime;
        if(time < 0) time = 0;

        return time;
    }

    private Vector3 RandomizePosition()
    {
        Vector3 monsterPos = transform.position;
        Vector2 randomCircle = Random.insideUnitCircle * 5f;

        Vector3 targetPosition = new Vector3(monsterPos.x + randomCircle.x, monsterPos.y, monsterPos.z + randomCircle.y);

        return targetPosition;
    }

    private float GetRMS(AudioSource source)
    {
        float[] samples = new float[256];
        if (!source.isPlaying) return 0;

        source.GetOutputData(samples, 0);

        float sum = 0f;

        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }

        float rmsValue = Mathf.Sqrt(sum / samples.Length);

        // scaled version for use in gameplay
        float scaledValue = rmsValue * soundSensitivity;
        // Debug.Log(scaledValue);
        return scaledValue;
    }
    private void EntityDetection()
    {
        if(monsterState == state.Listening){
            objRenderer.material.color = Color.green; // this is for testing purposes only
        }
        else if(monsterState == state.Investigating){
            duration = DetectSound(20f) ? 10f : CountDown(duration);
            foreach(Vector3 location in noiseLocations)
            {
                bool spotted = Observe(location, 20f);
                if(spotted) break;
            }
        }

    }

    // Notes:
    /*
        Createing field of view:
        - Use an overlapshere and only check at certain angles
        - Overlapshere for detecting noise 
    */
}
