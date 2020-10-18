using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefendScript : MonoBehaviour
{
    private int currentState;

    private const float SPAWN_TIME = 0.5f;//seconds
    private const float REACTIVE_TIME = 4;//seconds
    private const float NORMAL_SPEED = 1.0f;
    private const float RETURN_SPEED = 2.0f;
    private float detect_range;
    private bool detectEffectRunning;
    public GameObject westWall;
    public GameObject eastWall;
    public Material activeMat;
    public Material idleMat;
    private float lifeTime;
    private Vector3 originalPos;
    private GameObject targetAttacker;
    // Start is called before the first frame update
    void Start()
    {
        OnSpawnEffect();
        UseActiveSkin(false);
        UseArrowAndDetectRange(false,false);
        UseCollision(false);
        lifeTime = 0.0f;
        originalPos = this.transform.position;
        currentState = Globals.DEF_STATE_IDLE;
        detectEffectRunning = false;
        //detect range = 35% width of the battle field ?
        detect_range = (eastWall.transform.position.x - westWall.transform.position.x) * 35 / 100;
        Debug.Log("detect_range ? "+detect_range);

        //multiply for 5 because its parent scaled down to 0.2
        this.transform.GetChild(Globals.SOLDIER_DETECT_ZONE).transform.localScale = new Vector3(detect_range * 5, 0.01f, detect_range * 5);
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime += Time.deltaTime;
        if (currentState == Globals.DEF_STATE_IDLE)
        {
            //Debug.Log("Defender Idle");
            UseActiveSkin(false);
            UseArrowAndDetectRange(false,false);
            UseCollision(false);

            if (lifeTime > SPAWN_TIME)
                currentState = Globals.DEF_STATE_ACTIVE;
        }
        else if (currentState == Globals.DEF_STATE_ACTIVE)
        {
            //Debug.Log("Defender Active");
            UseActiveSkin(true);
            UseArrowAndDetectRange(false,true);
            UseCollision(true);
        }
        else if (currentState == Globals.DEF_STATE_CATCH_PLAYER
              && targetAttacker != null
        )
        {
            //Debug.Log("Defender Catch Player");
            UseActiveSkin(true);
            UseArrowAndDetectRange(true,false);
            UseCollision(true);
            OnDetectEffect();

            this.transform.LookAt(targetAttacker.transform);
            this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          targetAttacker.transform.position, 
                                                          NORMAL_SPEED * Time.deltaTime);
        }
        else if (currentState == Globals.DEF_STATE_REGENERATION)
        {
            //Debug.Log("Defender Regeneration");
            UseActiveSkin(false);
            UseArrowAndDetectRange(true,false);
            UseCollision(false);

            this.transform.LookAt(originalPos);
            this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          originalPos, 
                                                          RETURN_SPEED * Time.deltaTime);

            if (this.transform.position == originalPos)
            {
                UseArrowAndDetectRange(false,false);
                if (lifeTime > REACTIVE_TIME)
                    currentState = Globals.DEF_STATE_ACTIVE;
            }
        }
        else if (currentState == Globals.DEF_STATE_RETURN)
        {
            //Debug.Log("Defender return");
            UseActiveSkin(true);
            UseArrowAndDetectRange(true,false);
            UseCollision(false);

            this.transform.LookAt(originalPos);
            this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          originalPos, 
                                                          RETURN_SPEED * Time.deltaTime);
            if (this.transform.position == originalPos)
            {
                currentState = Globals.DEF_STATE_ACTIVE;
            }
        }
    }   

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("OnCollisionEnter Defender catched ? "+other.gameObject.tag.ToString());
        if ((other.gameObject.CompareTag("Player") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
        ||  (other.gameObject.CompareTag("Enemy") && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
        )
        {
            currentState = Globals.DEF_STATE_REGENERATION;
            lifeTime = 0;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter Defender catched ? "+other.gameObject.tag.ToString());
        if ((other.gameObject.CompareTag("Player") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
        ||  (other.gameObject.CompareTag("Enemy") && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
        )
        {
            currentState = Globals.DEF_STATE_REGENERATION;
            lifeTime = 0;
        }
    }

    public void PullTriggerFromDetectZone(Collider other)
    {
        //Debug.Log("Enemy catched something from detect zone ? "+other.gameObject.tag.ToString());
        if (other.gameObject != null &&
            ((other.gameObject.CompareTag("Player") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
           ||(other.gameObject.CompareTag("Enemy") && Globals.sPlayerRole == Globals.ROLE_DEFENDER))
        && other.gameObject.GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_CARRY_BALL)
        {
            targetAttacker = other.gameObject;
            currentState = Globals.DEF_STATE_CATCH_PLAYER;
        }
    }

    public int GetState()
    {
        return currentState;
    }

    public void SetState(int state)
    {
        currentState = state;
        Debug.Log("SetState state ? "+state);
    }

    private void UseActiveSkin(bool activeSkin)
    {
        //this.GetComponent<MeshRenderer>().material = activeSkin ? activeMat : idleMat;  
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_IDLE_LAYER, activeSkin ? 0.0f : 1.0f); 
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_BLUE_ACTIVE_LAYER, activeSkin && this.CompareTag("Player") ? 1.0f : 0.0f); 
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_RED_ACTIVE_LAYER, activeSkin && this.CompareTag("Enemy") ? 1.0f : 0.0f); 
    
    }

    void UseArrowAndDetectRange(bool arrowEnable,bool dectecEnable)
    {
        this.transform.GetChild(Globals.SOLDIER_ARROW).gameObject.SetActive(arrowEnable);
        //always disable highlight for defender soldier
        this.transform.GetChild(Globals.SOLDIER_HIGHLIGHT).gameObject.SetActive(false);
        this.transform.GetChild(Globals.SOLDIER_DETECT_ZONE).gameObject.SetActive(dectecEnable);
    }

    void UseCollision(bool hasCollsion)
    {
        this.GetComponent<Rigidbody>().isKinematic = true;
        this.GetComponent<Rigidbody>().detectCollisions = hasCollsion;
        this.GetComponent<Rigidbody>().useGravity = hasCollsion;
    }

    void OnSpawnEffect()
    {
        this.transform.GetChild(Globals.SOLDIER_PS_RING).GetComponent<ParticleSystem>().Play();
    }

    void OnDetectEffect()
    {
        if (!detectEffectRunning)
        {
            this.transform.GetChild(Globals.SOLDIER_PS_EMBER).GetComponent<ParticleSystem>().Play();
            detectEffectRunning = true;
        }
    }
}
