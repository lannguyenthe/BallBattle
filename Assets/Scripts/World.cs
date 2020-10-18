using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//global define
public static class Globals
{
    //Game data
    public const string STR_PLAYER_WIN = "PLAYER WIN";
    public const string STR_PLAYER_LOSE = "PLAYER LOSE";
    public const string STR_GAME_DRAW = "DRAW";
    public const string STR_GAME_FINISHED = "GAME FINISHED !!!";
    public const string STR_TIME_OUT = "Time out";
    public const string STR_NEXT_MATCH = "Next Match";
    public const string STR_PLAY_AGAIN = "Play Again ?";
    public const string STR_PLAY_MAZE = "Play Penalty Maze Game ?";
    public const string STR_OK = "OK";
    public const string STR_NO_ACTIVE_FRIENDS = "No actived attacker around !!!";
    public const string STR_REACH_GATE = "Attacker reach the gate.";
    public const string STR_DEFENDER = "(Defender)";
    public const string STR_ATTACKER = "(Attacker)";

    //Game states
    public const int GAME_RUNNING = 0;
    public const int GAME_OVER = GAME_RUNNING + 1;
    public const int GAME_PAUSE = GAME_OVER + 1;
    public const int GAME_RESUME = GAME_PAUSE + 1;
    public const int GAME_SHOW_MESSAGE = GAME_RESUME + 1;
    public const int GAME_MAZE_INIT = GAME_SHOW_MESSAGE + 1;
    public const int GAME_MAZE_RUNNING = GAME_MAZE_INIT + 1;

    public static int sGameState = -1;
    public static string sMessage = "";
    public const int ROLE_ATTACKER = 0;
    public const int ROLE_DEFENDER = 1;
    public static int sPlayerRole;
    public const int MAX_MATCH = 5;
    public static int sMatchCount;
    public static int sPlayerScore;
    public static int sEnemyScore;

    //attack state
    public const int ATK_STATE_IDLE = 111;
    public const int ATK_STATE_ACTIVE = ATK_STATE_IDLE + 1;
    public const int ATK_STATE_CATCH_THE_BALL = ATK_STATE_ACTIVE + 1;
    public const int ATK_STATE_CARRY_BALL = ATK_STATE_CATCH_THE_BALL + 1;
    public const int ATK_STATE_PASS_THE_BALL = ATK_STATE_CARRY_BALL + 1;
    public const int ATK_STATE_REGENERATION = ATK_STATE_PASS_THE_BALL + 1;
    public const int ATK_STATE_DESTROY = ATK_STATE_REGENERATION + 1;
    public const float BALL_SPEED = 1.5f;

    //defence state
    public const int DEF_STATE_IDLE = 222;
    public const int DEF_STATE_ACTIVE = DEF_STATE_IDLE + 1;
    public const int DEF_STATE_CATCH_PLAYER = DEF_STATE_ACTIVE + 1;
    public const int DEF_STATE_REGENERATION = DEF_STATE_CATCH_PLAYER + 1;
    public const int DEF_STATE_RETURN = DEF_STATE_REGENERATION + 1;

    //Time info
    public const float TIME_PER_MATCH = 140;//seconds

    //regeneration bar
    public const float ENERGY_REGERENATION = 0.5f;//0.5 per seccond
    public const float DEF_ENERGY_COST = 3;//points
    public const float ATK_ENERGY_COST = 2;//points
    public const float MAX_ENERGY_POINT = 6;

    //canvas define
    public const int PLAYER_INFO = 0;
    public const int ENEMY_INFO = 1;
    public const int TIME_INFO = 2;
    public const int POPUP = 3;
    public const int AR = 4;
    public const int IGM = 5;
    public static float sAttackerEnergyPoint;
    public static float sDefenderEnergyPoint;
    public static float sPlayerSpendEnergyPercent;
    public static float sEnemySpendEnergyPercent;
    public static float sMatchTimeLeft;

    //soldier define
    public const int SOLDIER_ARROW = 0;
    public const int SOLDIER_HIGHLIGHT = 1;
    public const int SOLDIER_DETECT_ZONE = 2;
    public const int SOLDIER_PS_RING = 3;
    public const int SOLDIER_PS_EMBER = 4;
    public const int SOLDIER_PS_SMOKE = 5;

    //soldier animator layer
    public const int ON_IDLE_LAYER = 1;
    public const int ON_BLUE_ACTIVE_LAYER = 2;
    public const int ON_RED_ACTIVE_LAYER = 3;
}

//world game control
public class World : MonoBehaviour
{
    // world
    public LayerMask layerStadium;
    public GameObject westWall;
    public GameObject eastWall;
    private float halfStadiumLine;
    public GameObject enemyFence;
    public GameObject playerFence;

    // entities
    public GameObject enemySoldier;//prefab
    public GameObject playerSoldier;//prefab
    public GameObject ball;//prefab
    private GameObject ballInstance;
    private int friendId;
    public List<GameObject> attackerList;
    public List<GameObject> defenderList;
    // Start is called before the first frame update
    void Start()
    {
        InitGlobalValue();
        InitNewGameValue(Globals.ROLE_ATTACKER);
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.sGameState == Globals.GAME_PAUSE)
            return;

        Globals.sMatchTimeLeft -= Time.deltaTime;
        //Debug.Log("Update defenderList count ? "+defenderList.Count);

        if (
        #if UNITY_EDITOR
        Input.GetMouseButtonDown(0)
        #else
        Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended
        #endif
        && Globals.sGameState == Globals.GAME_RUNNING)
        {
            RaycastHit hit;
        #if UNITY_EDITOR
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        #else
            //Touch touch = Input.GetTouch(0);
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
        #endif
            if (Physics.Raycast(ray, out hit, 100, layerStadium))
                CreateSoldier(hit);
        }
        
        if (Globals.sGameState == Globals.GAME_RUNNING)
        {
            if (Globals.sMatchTimeLeft < 0)
            {
                Debug.Log("Game draw due to time out");
                Globals.sGameState = Globals.GAME_OVER;
                Globals.sMessage = Globals.STR_TIME_OUT;
            }
            else
            {
                if (attackerList != null && attackerList.Count > 0)
                    UpdateAttackerStrategy();
                if (defenderList != null && defenderList.Count > 0)
                    UpdateDefenderStrategy();
            }
        }
        else if (Globals.sGameState == Globals.GAME_OVER)
        {
            GameOver();
        }
    }

    void CreateSoldier(RaycastHit hit)
    {
        if (Globals.sGameState >= Globals.GAME_MAZE_INIT)
            return;
        //player always spawn at bottom part of stadium
        if (hit.point.z <= halfStadiumLine)
        {
            if ((Globals.sPlayerRole == Globals.ROLE_ATTACKER && Globals.sAttackerEnergyPoint >= Globals.ATK_ENERGY_COST)
            ||  (Globals.sPlayerRole == Globals.ROLE_DEFENDER && Globals.sDefenderEnergyPoint >= Globals.DEF_ENERGY_COST)
            )
            {
                GameObject newPlayer = (GameObject)Instantiate(playerSoldier, new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z), Quaternion.identity);
                if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
                {
                    Destroy(newPlayer.gameObject.GetComponent<DefendScript>());
                    attackerList.Add(newPlayer); 
                    Globals.sAttackerEnergyPoint -= Globals.ATK_ENERGY_COST;
                }
                else
                {
                    Destroy(newPlayer.gameObject.GetComponent<AttackScript>());
                    defenderList.Add(newPlayer); 
                    Globals.sDefenderEnergyPoint -= Globals.DEF_ENERGY_COST;
                }
                RefreshEnergyLightBar(true);
            }
        }
        else //enemy spawn and role setting
        {
            if ((Globals.sPlayerRole == Globals.ROLE_DEFENDER && Globals.sAttackerEnergyPoint >= Globals.ATK_ENERGY_COST)
            ||  (Globals.sPlayerRole == Globals.ROLE_ATTACKER && Globals.sDefenderEnergyPoint >= Globals.DEF_ENERGY_COST)
            )
            {
                GameObject newEnemy = (GameObject)Instantiate(enemySoldier, new Vector3(hit.point.x, hit.point.y + 0.1f, hit.point.z), Quaternion.identity);
                if (Globals.sPlayerRole == Globals.ROLE_ATTACKER/*So Enenmy is DEFENDER*/)
                {
                    Destroy(newEnemy.gameObject.GetComponent<AttackScript>());
                    defenderList.Add(newEnemy); 
                    Globals.sDefenderEnergyPoint -= Globals.DEF_ENERGY_COST;
                }
                else /*Enenmy is ATTACKER*/
                {
                    Destroy(newEnemy.gameObject.GetComponent<DefendScript>());
                    attackerList.Add(newEnemy); 
                    Globals.sAttackerEnergyPoint -= Globals.ATK_ENERGY_COST;
                }
                RefreshEnergyLightBar(false);
            }
        }
        //Debug.Log("CreateSoldier attackList count ? "+attackerList.Count);
        //Debug.Log("CreateSoldier defenderList count ? "+defenderList.Count);
    }

    // player behavior
    void UpdateAttackerStrategy()
    {
        AllAttackerChaseTheBall();
        
        //find if there is any player need to pass a ball ?
        for (int i = 0; i < attackerList.Count; i++)
        {
            if (attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_PASS_THE_BALL)
            {
                // attacker soldier need to pass ball but there are not any friends around => Defender win
                if (attackerList.Count <= 1)
                {   
                    Debug.Log("Attacker lose due to no actives friends around.");                
                    Globals.sGameState = Globals.GAME_OVER;
                    Globals.sMessage = Globals.STR_NO_ACTIVE_FRIENDS;
                    break;
                } else {
                    // if have friends then try to find for the nearest active one ?
                    float distance = 1000;
                    int index = -1;
                    for (int j = 0; j < attackerList.Count; j++)
                    {
                        if (j != i && attackerList[i] != null && attackerList[j] != null
                        && (attackerList[j].GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_ACTIVE
                          ||attackerList[j].GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_CATCH_THE_BALL)
                        ) 
                        {
                            float d = Vector3.Distance(attackerList[i].transform.position, attackerList[j].transform.position);
                            if (d < distance)
                            {
                                distance = d;
                                index = j;
                            }
                        }
                    }       
                    if (index >= 0)
                    {                        
                        attackerList[i].GetComponent<AttackScript>().ReleaseTheBall();     
                        friendId = index;
                    }
                    else //have friends but no one actived => lose
                    {
                        Debug.Log("Attacker lose due to no actives friends around.");
                        Globals.sGameState = Globals.GAME_OVER;
                        Globals.sMessage = Globals.STR_NO_ACTIVE_FRIENDS;
                        break;
                    }
                }              
            }
        }

        if (!HasAttackerHoldTheBall())
            StartCoroutine("ThrowingBall");
    }

    private bool HasAttackerHoldTheBall()
    {
        for (int i = 0; i < attackerList.Count; i++) 
        {
            if (attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_CARRY_BALL)
            {
                friendId = -1;
                return true;    
            }
        }
        return false;
    }

    void AllAttackerChaseTheBall()
    {
        if (!HasAttackerHoldTheBall())
        {
            for (int i = 0; i < attackerList.Count; i++)
                if (attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() == Globals.ATK_STATE_ACTIVE)
                    attackerList[i].GetComponent<AttackScript>().TryCatchBall(ballInstance);
        }
        else
        {
            for (int i = 0; i < attackerList.Count; i++)
                if (attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() != Globals.ATK_STATE_CARRY_BALL
                &&  attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() != Globals.ATK_STATE_REGENERATION
                &&  attackerList[i] != null && attackerList[i].GetComponent<AttackScript>().GetState() != Globals.ATK_STATE_IDLE
                )
                    attackerList[i].GetComponent<AttackScript>().SetState(Globals.ATK_STATE_ACTIVE);
        }
    }
    
    IEnumerator ThrowingBall() 
    {
        if (ballInstance != null && friendId >= 0 && attackerList[friendId] != null)
        {
            ballInstance.transform.position = Vector3.MoveTowards(ballInstance.transform.position, 
                                                        attackerList[friendId].transform.position, 
                                                        Globals.BALL_SPEED * Time.deltaTime);
        }
        yield return null;
    }

    //enemy behavior
    void UpdateDefenderStrategy()
    {
        //return all enemies to original position when we have some
        if (!HasAttackerHoldTheBall())
        {
            foreach(GameObject soldier in defenderList)
            {
                if (soldier.GetComponent<DefendScript>().GetState() == Globals.DEF_STATE_CATCH_PLAYER)
                    soldier.GetComponent<DefendScript>().SetState(Globals.DEF_STATE_RETURN);
            }
        }
    }

    //UI observe
    void InitEnergyBar()
    {
        Globals.sAttackerEnergyPoint = 0;
        Globals.sDefenderEnergyPoint = 0;
        Globals.sPlayerSpendEnergyPercent = 0;
        Globals.sEnemySpendEnergyPercent = 0;
    }

    void RefreshEnergyLightBar(bool isPlayer)
    {
        if (isPlayer)
        {
            Globals.sPlayerSpendEnergyPercent = (Globals.sPlayerRole == Globals.ROLE_ATTACKER ? Globals.ATK_ENERGY_COST / Globals.MAX_ENERGY_POINT:
                                                                            Globals.DEF_ENERGY_COST / Globals.MAX_ENERGY_POINT);
        }
        else
        {
            Globals.sEnemySpendEnergyPercent = (Globals.sPlayerRole == Globals.ROLE_ATTACKER ? Globals.DEF_ENERGY_COST / Globals.MAX_ENERGY_POINT:
                                                                            Globals.ATK_ENERGY_COST / Globals.MAX_ENERGY_POINT);
        }
    }

    //all games observer
    void InitGlobalValue()
    {
        Globals.sPlayerScore = 0;
        Globals.sEnemyScore = 0;
        Globals.sMatchCount = 0;
    }

    void InitNewGameValue(int role)
    {
        //clear all list object
        if (attackerList != null && attackerList.Count > 0)
        {
           // Debug.Log("InitNewGameValue attackList count ? "+attackerList.Count);           
            foreach(GameObject ob in attackerList)
            {
                //Debug.Log("InitNewGameValue Destroy ob in attackerList");
                Destroy(ob);
            }
        }
        else if (attackerList == null)
        {
            //Debug.Log("InitNewGameValue attackerList null");
        }

        if (defenderList != null && defenderList.Count > 0)
        {
            //Debug.Log("InitNewGameValue defenderList count ? "+defenderList.Count);
            foreach(GameObject ob in defenderList)
            {
                //Debug.Log("InitNewGameValue Destroy ob in defenderList");
                Destroy(ob);
            }
        }
        else if (defenderList == null)
        {
            //Debug.Log("InitNewGameValue defenderList null");
        }

        if (ballInstance != null)
        {
            //Debug.Log("InitNewGameValue Destroy ballInstance");
            Destroy(ballInstance);
        }

        attackerList = new List<GameObject>();
        defenderList = new List<GameObject>();

        InitEnergyBar();

        friendId = -1;
        Globals.sMatchTimeLeft = Globals.TIME_PER_MATCH;
        Globals.sPlayerRole = role;
        Debug.Log("InitNewGameValue playerRole ? "+Globals.sPlayerRole);

        halfStadiumLine = (enemyFence.transform.position.z + playerFence.transform.position.z) / 2;
        //Debug.Log("halfStadiumLine ? "+halfStadiumLine);

        if (Globals.sGameState != Globals.GAME_MAZE_INIT)
        {
            //generate ball in player field
            float offset = 0.4f;
            float zPos = Globals.sPlayerRole == Globals.ROLE_ATTACKER ?
                        Random.Range(playerFence.transform.position.z + offset, halfStadiumLine - offset) :
                        Random.Range(halfStadiumLine + offset, enemyFence.transform.position.z - offset);   
            float xPos = Random.Range(westWall.transform.position.x + offset, eastWall.transform.position.x - offset); 
            Debug.Log("InitNewGameValue creat ballInstance");
            ballInstance = (GameObject)Instantiate(ball, new Vector3(xPos, 0.1f, zPos), Quaternion.identity);
        }

        Globals.sMatchCount++;
        Debug.Log("InitNewGameValue sMatchCount ? "+Globals.sMatchCount);

        if (Globals.sMatchCount > Globals.MAX_MATCH)
            Globals.sGameState = Globals.GAME_SHOW_MESSAGE;
        else if(Globals.sGameState < Globals.GAME_MAZE_INIT)
            Globals.sGameState = Globals.GAME_RUNNING;
    }

    public void NextMatch()
    {
        Debug.Log("NextMatch attackList count ? "+attackerList.Count);
        Debug.Log("NextMatch defenderList count ? "+defenderList.Count);
        if (Globals.sMatchCount <= Globals.MAX_MATCH)
        {
            int newRole = (Globals.sPlayerRole == Globals.ROLE_ATTACKER ? Globals.ROLE_DEFENDER : Globals.ROLE_ATTACKER);
            Debug.Log("Next Match newRole ? "+newRole);
            InitNewGameValue(newRole);
        } 
        else //REPLAY
        {
            //Game maze penalty for DRAW player
            if (Globals.sEnemyScore == Globals.sPlayerScore)
            {
                InitMazeGame();
            }
            else
            {
                InitGlobalValue();
                InitNewGameValue(Globals.ROLE_ATTACKER);
            }
        }
    }

    void GameOver()
    { 
        if (Globals.sMessage == Globals.STR_NO_ACTIVE_FRIENDS) //Attacker Lose
        {
            if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
            {
                Globals.sEnemyScore++;
            }
            else
            {
                Globals.sPlayerScore++;
            }
        } 
        else if (Globals.sMessage == Globals.STR_REACH_GATE)
        {
            if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
            {
                Globals.sPlayerScore++;
            }
            else
            {
                Globals.sEnemyScore++;
            }
        }
        Globals.sGameState = Globals.GAME_SHOW_MESSAGE;
    }

    public void Cheat(int value)
    {
        Debug.Log("Cheat value ? "+value);
        if (value > 0 && value < 4)
            Globals.sGameState = Globals.GAME_OVER;
        if (value == 1)
            Globals.sMessage = Globals.STR_REACH_GATE;
        else if (value == 2)
            Globals.sMessage = Globals.STR_NO_ACTIVE_FRIENDS;
        else if (value == 3)
            Globals.sMessage = Globals.STR_TIME_OUT;
        else if (value == 4)
        {
            Globals.sAttackerEnergyPoint = 999;
            Globals.sDefenderEnergyPoint = 999;
        }
        else if (value == 5)
        {
            Globals.sGameState = Globals.GAME_OVER;
            Globals.sMessage = Globals.STR_TIME_OUT;
            Globals.sPlayerScore = 0;
            Globals.sEnemyScore = 0;
            Globals.sMatchCount = Globals.MAX_MATCH;
        }
        //Debug.Log("Cheat attackList count ? "+attackerList.Count);
        //Debug.Log("Cheat defenderList count ? "+defenderList.Count);
    }

    //maze game implement
    void InitMazeGame()
    {
        Debug.Log("InitMazeGame");
        Globals.sGameState = Globals.GAME_MAZE_INIT;
        InitGlobalValue();
        InitNewGameValue(Globals.ROLE_ATTACKER);

        GenerateRandomMaze();
    }

    void GenerateRandomMaze()
    {
        
    }
}
