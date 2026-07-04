using UnityEngine;
using System.Collections.Generic;
using System.Collections;



public class NewNoiseDetection : MonoBehaviour
{
    public enum state
    {
        Listening,
        Investigating
    }
    public state monsterState;
    public float visibility;
    private Dictionary<string, int> scores = new Dictionary<string, int>();
    private Renderer objRenderer;
    [SerializeField] private List<Vector3> noiseLocations = new List<Vector3>();
    [SerializeField] private float duration;
    [SerializeField] private LayerMask target;
    [SerializeField] private LayerMask obstacleMask;
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
                if(GetRMS(audio) > 0.6f){
                    noiseLocations.Clear();
                    noiseLocations.Add(hit.transform.position);
                    return true;
                }
            }
        }
        return false;
    }

    private void Observe(Vector3 targetPos, float range)
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
            Vector3 dirToHit = (hit.transform.position - transform.position).normalized;
            float checkAngle = Vector3.SignedAngle(currentDir, dirToHit, Vector3.up);
            if (checkAngle >= leftAngle && checkAngle <= rightAngle)
            {
                float distToHit = Vector3.Distance(transform.position, hit.transform.position);
                RaycastHit hitInfo;
                if (!Physics.SphereCast(transform.position, visibility, dirToHit, out hitInfo, distToHit, obstacleMask)){ 
                    Debug.Log("Monster sees player"); // this is for testing purposes
                    objRenderer.material.color = Color.red; // this is for testing purposes
                }
                else objRenderer.material.color = Color.green; // this is for testing purposes

            }
        }


        Debug.DrawRay(transform.position, leftDir * 20f, Color.yellow); // This is for testing ONLY!!
        Debug.DrawRay(transform.position, rightDir * 20f, Color.yellow); // This is for testing ONLY!!
        Debug.DrawRay(transform.position, currentDir * 20f, Color.red); // This is for testing ONLY!!
    }

    private IEnumerator FOVroutine(Collider targetObj, float range)
    {
        WaitForSeconds wait = new WaitForSeconds(4f);
        while (true)
        {
            yield return wait;
        }
    }

    private void ManageStates()
    {
        if(monsterState == state.Listening && DetectSound(10f))
        {
            monsterState = state.Investigating;
            noiseIndicator.PlayOneShot(audioFile); // this is for testing remove it for the final script
        }
        else if(monsterState == state.Investigating && !DetectSound(10f) && duration <= 0)
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
            objRenderer.material.color = Color.green;
        }
        else if(monsterState == state.Investigating){
            duration = DetectSound(10f) ? 10f : CountDown(duration);
            foreach(Vector3 location in noiseLocations)
            {
                Observe(location, 10f);
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
