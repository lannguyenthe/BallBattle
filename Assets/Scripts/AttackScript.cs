using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackScript : MonoBehaviour
{
    private const float SPAWN_TIME = 0.5f;//seconds
    private const float REACTIVE_TIME = 2.5f;//seconds
    private const float NORMAL_SPEED = 1.5f;
    private const float CARRYING_SPEED = 0.75f;
    private int currentState;

    public GameObject targetGate;
    public GameObject targetFence;
    private GameObject targetBall;
    private GameObject targetFriend;
    public Material activeMat;
    public Material idleMat;
    private float lifeTime;
    //AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        OnSpawnEffect();
        UseActiveSkin(false);
        UseArrowAndHighLight(false,false);
        currentState = Globals.ATK_STATE_IDLE;
        lifeTime = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        lifeTime += Time.deltaTime;

        if (currentState == Globals.ATK_STATE_IDLE)
        {
            //Debug.Log("Attacker Idle");
            UseCollision(false);
            UseArrowAndHighLight(false,false);
            UseActiveSkin(false);
            
            if (lifeTime > SPAWN_TIME)
                currentState = Globals.ATK_STATE_ACTIVE;
        }
        else if (currentState == Globals.ATK_STATE_ACTIVE)
        {
            //Debug.Log("Attacker Active");
            UseCollision(false);
            UseArrowAndHighLight(true,false);
            UseActiveSkin(true);
           
            Vector3 fencePos = new Vector3(this.transform.position.x, targetFence.transform.position.y + 0.1f, targetFence.transform.position.z);
            this.transform.LookAt(fencePos);
            this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          fencePos, 
                                                          NORMAL_SPEED * Time.deltaTime); 
                 
            if ((this.transform.position.z >= fencePos.z && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
             || (this.transform.position.z <= fencePos.z && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
            )
            {
                this.GetComponent<Animator>().SetBool("OnDestroy",true);
                currentState = Globals.ATK_STATE_DESTROY;
                //Destroy(this.gameObject);
            }
        }
        else if (currentState == Globals.ATK_STATE_CATCH_THE_BALL
            &&   targetBall != null
        )
        {
            //Debug.Log("Attacker Catch Ball");
            UseCollision(true);
            UseArrowAndHighLight(true,false);
            UseActiveSkin(true);

            this.transform.GetChild(0).gameObject.SetActive(true);
            this.transform.LookAt(targetBall.transform);
            this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          targetBall.transform.position, 
                                                          NORMAL_SPEED * Time.deltaTime);
        }
        else if (currentState == Globals.ATK_STATE_CARRY_BALL)
        {
            //Debug.Log("Attacker carry Ball");
            UseCollision(true);
            UseArrowAndHighLight(true,true);
            UseActiveSkin(true);

            if (targetBall != null)
            {
                targetBall.transform.parent = this.transform;
                targetBall.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.2f, this.transform.position.z);
                this.transform.LookAt(targetGate.transform);
                this.transform.position = Vector3.MoveTowards(this.transform.position, 
                                                          targetGate.transform.position, 
                                                          (Globals.sIsRushGame ? NORMAL_SPEED : CARRYING_SPEED) * Time.deltaTime);                                                                       
            }
        }
        else if (currentState == Globals.ATK_STATE_PASS_THE_BALL)
        {
            //Debug.Log("Attacker pass Ball");
            UseCollision(false);
            UseArrowAndHighLight(false,false);
            UseActiveSkin(false);
        }
        else if (currentState == Globals.ATK_STATE_REGENERATION)
        {
            UseCollision(false);
            UseArrowAndHighLight(false,false);
            UseActiveSkin(false);

            if (lifeTime > REACTIVE_TIME)
                currentState = Globals.ATK_STATE_ACTIVE;
        }
    }

    void OnCollisionEnter(Collision other)
    {
        //Debug.Log("OnCollisionEnter Attacker catched ? "+other.gameObject.tag.ToString());
        if ((other.gameObject.CompareTag("Enemy") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
        ||  (other.gameObject.CompareTag("Player") && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
        )
        {
            currentState = Globals.ATK_STATE_PASS_THE_BALL;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnTriggerEnter Attacker catched ? "+other.gameObject.tag.ToString());
        if (other.gameObject.CompareTag("Ball"))
        {
            currentState = Globals.ATK_STATE_CARRY_BALL;
        }
        else if ((other.gameObject.CompareTag("Enemy") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
             ||  (other.gameObject.CompareTag("Player") && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
        )
        {
            currentState = Globals.ATK_STATE_PASS_THE_BALL;
        }
        else if ((other.gameObject.CompareTag("EnemyGate") && Globals.sPlayerRole == Globals.ROLE_ATTACKER)
             ||  (other.gameObject.CompareTag("PlayerGate") && Globals.sPlayerRole == Globals.ROLE_DEFENDER)
        )
        {
            if (targetBall != null && this.transform.childCount > Globals.SOLDIER_PS_SMOKE + 1)
            {
                Debug.Log("OnTriggerEnter childCount ? "+this.transform.childCount);
                Globals.sGameState = Globals.GAME_OVER;
                Globals.sMessage = Globals.STR_REACH_GATE;
            }
            /*
            if (Globals.sHasSfx)
            {
               audio = this.GetComponent<AudioSource>();
                audio.Play();
            }
            */
            Destroy(this.gameObject);
        }
    }

    public void TryCatchBall(GameObject ball)
    {
        if (currentState == Globals.ATK_STATE_ACTIVE)
        {
            targetBall = ball;
            currentState = Globals.ATK_STATE_CATCH_THE_BALL;
        }
    }

    public void ReleaseTheBall()
    {
        //Debug.Log("Attacker ReleaseTheBall");
        currentState = Globals.ATK_STATE_REGENERATION;
        lifeTime = 0;
        if (targetBall != null)
        {
            targetBall.transform.parent = null;
            targetBall = null;
        }
    }

    public int GetState()
    {
        return currentState;
    }

    public void SetState(int state)
    {
        currentState = state;
    }

    void UseCollision(bool hasCollsion)
    {
        this.GetComponent<Rigidbody>().isKinematic = !hasCollsion;
        this.GetComponent<Rigidbody>().detectCollisions = hasCollsion;
        this.GetComponent<Rigidbody>().useGravity = hasCollsion;
    }

    void UseArrowAndHighLight(bool arrowEnable, bool highlightEnable)
    {
        this.transform.GetChild(Globals.SOLDIER_ARROW).gameObject.SetActive(arrowEnable);
        this.transform.GetChild(Globals.SOLDIER_HIGHLIGHT).gameObject.SetActive(highlightEnable);
        //always disable detect zone for attacker soldier
        this.transform.GetChild(Globals.SOLDIER_DETECT_ZONE).gameObject.SetActive(false);
    }

    private void UseActiveSkin(bool activeSkin)
    {
        //this.GetComponent<MeshRenderer>().material = activeSkin ? activeMat : idleMat;
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_IDLE_LAYER, activeSkin ? 0.0f : 1.0f); 
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_BLUE_ACTIVE_LAYER, activeSkin && this.CompareTag("Player") ? 1.0f : 0.0f); 
        this.GetComponent<Animator>().SetLayerWeight(Globals.ON_RED_ACTIVE_LAYER, activeSkin && this.CompareTag("Enemy") ? 1.0f : 0.0f); 
    }

    void OnSpawnEffect()
    {
        this.transform.GetChild(Globals.SOLDIER_PS_RING).GetComponent<ParticleSystem>().Play();
    }

    public void OnDestroyEffect()
    {
        this.transform.GetChild(Globals.SOLDIER_PS_SMOKE).GetComponent<ParticleSystem>().Play();
    }

    public void OnDestroySoldier()
    {
        Destroy(this.gameObject);
    }
}
