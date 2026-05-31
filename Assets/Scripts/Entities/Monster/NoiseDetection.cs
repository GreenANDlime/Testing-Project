using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoiseDetection : MonoBehaviour
{
    public enum state
    {
        Listening,
        Wandering,
        Chasing,
        Investigating
    }

    

    public state monsterState;
    public LayerMask targetLayer;
    private AudioSource[] audioSources;

    private float xPos;
    private float zPos;
    public float rbVelocity;
    private Vector3 lastPosition;
    public bool delay;
    public bool isMoving;
    private bool hasAnimations;
    private Rigidbody rb;
    public Animator anim;
    private float rotationVelocity;

    [Header("Hearing Level")]
    [SerializeField] private float defaultHearing;
    [SerializeField] private float hearDuration;
    [SerializeField] private GameObject moveBone;
    [SerializeField] private float smoothRotation;
    [SerializeField] private List<Vector3> noiseLocations = new List<Vector3>();
    [SerializeField] private List<AudioClip> monsterAudios = new List<AudioClip>();

    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Vector3 monsterPos = moveBone != null ? moveBone.transform.position : transform.position;

        monsterState = state.Wandering;
        xPos = monsterPos.x;
        zPos = monsterPos.z;
        hearDuration = defaultHearing;
        audioSources = GetComponentsInChildren<AudioSource>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Start()
    {
        hasAnimations = moveBone != null;
        Vector3 monsterPos = hasAnimations ? moveBone.transform.position : transform.position;
        lastPosition = monsterPos;
    }

    // Update is called once per frame
    void Update()
    {
        MonsterMovements();
    }
    void FixedUpdate()
    {
        // isMoving = rb.linearVelocity.magnitude > 0f;
        rbVelocity = rb.linearVelocity.magnitude;
    }
    
    private bool DetectNoise(float hearRange)
    {
        Vector3 monsterPos = hasAnimations ? moveBone.transform.position : transform.position;

        Collider[] hitColliders = Physics.OverlapSphere(monsterPos, hearRange, targetLayer);
        foreach(var entity in hitColliders)
        {
            if(entity.transform.parent != null)
            {
                Transform playerParent = entity.transform.parent;
                if (playerParent.gameObject.CompareTag("Player"))
                {
                    Transform playerSound = playerParent.Find("SoundPlayer");
                    PlaySound playSound = playerSound.GetComponent<PlaySound>();
                    
                    // Remembers either position of where noise came from or locks onto player indeffinetly
                    if(playSound.ReturnVolume() > 0.6f && (monsterState == state.Listening || monsterState == state.Chasing)){
                        Vector3 targetPosition = playerParent.position;
                        targetPosition.y = monsterPos.y;
                        noiseLocations.Clear();
                        noiseLocations.Add(targetPosition);
                    }
                    else if(playSound.ReturnVolume() > 0.6f && monsterState == state.Investigating){
                        noiseLocations.Clear();
                        noiseLocations.Add(playerParent.position);
                    }

                    return playSound.ReturnVolume() > 0.6f;
                }
            }
        }
        return false;
    }

    private Vector3 RandomizePosition()
    {
        Vector3 monsterPos = hasAnimations ? moveBone.transform.position : transform.position;

        if(monsterPos.x == xPos && monsterPos.z == zPos){
            xPos = Random.Range(monsterPos.x - 15, monsterPos.x - 5);
            zPos = Random.Range(monsterPos.z + 15 , monsterPos.z + 20);
        }

        Vector3 targetPosition = new Vector3(xPos, monsterPos.y, zPos);

        return targetPosition;
    }

    private float CountDown(float time)
    {
        time -= Time.deltaTime;
        if(time < 0) time = 0;

        return time;
    }

    IEnumerator Delay(float duration)
    {
        delay = true;
        yield return new WaitForSeconds(duration);
        delay = false;
    }

    private void LocatePosition(Vector3 targetPos, float speed)
    {
        if(hasAnimations){
            targetPos.y = moveBone.transform.position.y;
            Vector3 direction = (targetPos - moveBone.transform.position).normalized;
            Vector3 customForward = moveBone.transform.right.normalized;
            float signedAngle = Vector3.SignedAngle(customForward, direction, Vector3.forward);

            Vector3 currentEuler = moveBone.transform.eulerAngles;
            float targetZ = currentEuler.z + Mathf.DeltaAngle(0f, signedAngle);

            float smoothZ = Mathf.SmoothDampAngle(currentEuler.z, targetZ, ref rotationVelocity, smoothRotation);
            moveBone.transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y, smoothZ);
            moveBone.transform.position = Vector3.MoveTowards(moveBone.transform.position, targetPos, Time.deltaTime * speed);

        }
        else{
            targetPos.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * speed);
        }


    }
    private void MonsterMovements()
    {
        if(monsterState != state.Listening && !delay){
            isMoving = true;
            anim.SetBool("isMoving", true);
        }
        else{
            isMoving = false;
            anim.SetBool("isMoving", false);
        }

        if(monsterState == state.Wandering)
        {
            audioSources[0].pitch = 1f;
            if(DetectNoise(10f)){
                monsterState = state.Listening;
                StartCoroutine(Delay(3f));
            }
            else LocatePosition(RandomizePosition(), 2.5f);
        }
        else if(monsterState == state.Listening)
        {
            if(!delay){
                hearDuration = (hearDuration <= 0 || (DetectNoise(15f) && hearDuration > 0)) ? defaultHearing : CountDown(hearDuration);
                if (DetectNoise(15f) && hearDuration > 0){
                    monsterState = state.Investigating;
                    StartCoroutine(Delay(1.5f));
                }
                else if(hearDuration <= 0) monsterState = state.Wandering;
            }
        }
        else if(monsterState == state.Investigating)
        {
            if(!delay){
                hearDuration = (hearDuration <= 0 || (DetectNoise(15f) && hearDuration > 0)) ? defaultHearing : CountDown(hearDuration);
                foreach(Vector3 location in noiseLocations){
                    LocatePosition(location, 1.5f);
                }
                if(DetectNoise(10f) && hearDuration > 0){
                    monsterState = state.Chasing;
                    rb.linearVelocity = Vector3.zero;
                    audioSources[1].PlayOneShot(monsterAudios[0]);
                    StartCoroutine(Delay(2f));
                }
                else if(hearDuration <= 0){
                    monsterState = state.Wandering;
                    noiseLocations.Clear();
                }
            }
        }
        else if(monsterState == state.Chasing)
        {
            if(!delay){
                audioSources[0].pitch = 3f;
                hearDuration = (hearDuration <= 0 || (DetectNoise(15f) && hearDuration > 0)) ? defaultHearing * 2f : CountDown(hearDuration);
                foreach(Vector3 targetPos in noiseLocations){
                    Vector3 newTargetPos = targetPos;
                    if(hasAnimations){
                        newTargetPos.y = moveBone.transform.position.y;
                    }
                    else newTargetPos.y = transform.position.y;
                    LocatePosition(newTargetPos, 4.5f);
                }
                if(DetectNoise(15f) && !(hearDuration <= 0)){
                    monsterState = state.Chasing;
                }
                else if(hearDuration <= 0){
                    monsterState = state.Wandering;
                    noiseLocations.Clear();
                }
            }
            
        }
    }
}
