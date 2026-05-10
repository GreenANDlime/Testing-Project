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
    private AudioSource audioSource;

    private float xPos;
    private float zPos;
    public bool delay;
    [SerializeField] private float defaultHearing;
    [SerializeField] private float hearDuration;
    [SerializeField] private List<Vector3> noiseLocations = new List<Vector3>();
    [SerializeField] private List<AudioClip> monsterAudios = new List<AudioClip>();
    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        monsterState = state.Wandering;
        xPos = transform.position.x;
        zPos = transform.position.z;
        hearDuration = defaultHearing;
        audioSource = GetComponentInChildren<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        MonsterMovements();
    }
    
    private bool DetectNoise(float hearRange)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, hearRange, targetLayer);
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
                        targetPosition.y = transform.position.y;
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
        if(transform.position.x == xPos && transform.position.z == zPos){
            xPos = Random.Range(-25, -5);
            zPos = Random.Range(0, 20);
        }

        Vector3 targetPosition = new Vector3(xPos, transform.position.y, zPos);

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

    private void MonsterMovements()
    {
        if(monsterState == state.Wandering)
        {
            if(DetectNoise(10f)){
                monsterState = state.Listening;
                StartCoroutine(Delay(3f));
            }
            else transform.position = Vector3.MoveTowards(transform.position, RandomizePosition(), Time.deltaTime * 2.5f);
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
                    transform.position = Vector3.MoveTowards(transform.position, location, Time.deltaTime * 1.5f);
                }
                if(DetectNoise(10f) && hearDuration > 0){
                    monsterState = state.Chasing;
                    audioSource.PlayOneShot(monsterAudios[0]);
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
                hearDuration = (hearDuration <= 0 || (DetectNoise(15f) && hearDuration > 0)) ? defaultHearing * 2f : CountDown(hearDuration);
                foreach(Vector3 targetPos in noiseLocations){
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 4.5f);
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
