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
    public const int GAME_INIT = 0;
    public const int GAME_MENU = GAME_INIT + 1;
    public const int GAME_OPTION = GAME_MENU + 1;
    public const int GAME_RUNNING = GAME_OPTION + 1;
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
    public static bool sIsRushGame = false;
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
    public const float RUSH_TIME_PER_MATCH = 15;//seconds

    //regeneration bar
    public const float ENERGY_REGERENATION = 0.5f;//0.5 per seccond
    public const float RUSH_ENERGY_REGERENATION = 2.0f;//0.5 per seccond
    public const float DEF_ENERGY_COST = 3;//points
    public const float ATK_ENERGY_COST = 2;//points
    public const float MAX_ENERGY_POINT = 6;

    //canvas define
    public const int MENU = 0;
    public const int PLAYER_INFO = MENU + 1;
    public const int ENEMY_INFO = PLAYER_INFO + 1;
    public const int TIME_INFO = ENEMY_INFO + 1;
    public const int POPUP = TIME_INFO + 1;
    public const int AR = POPUP + 1;
    public const int IGM = AR + 1;
    public const int OPTION = IGM + 1;
    public static float sAttackerEnergyPoint;
    public static float sDefenderEnergyPoint;
    public static float sPlayerSpendEnergyPercent;
    public static float sEnemySpendEnergyPercent;
    public static float sMatchTimeLeft;
    public static bool sHasSound = false;
    public static bool sHasSfx = false;
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
    private GameObject wallContainer;
    private int friendId;
    public List<GameObject> attackerList;
    public List<GameObject> defenderList;

    //audio
    public AudioSource audioSoundAP;
    
    // Start is called before the first frame update
    void Start()
    {
        Globals.sGameState = Globals.GAME_INIT;
        //InitGlobalValue();
        //InitNewGameValue(Globals.ROLE_ATTACKER);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Globals.sGameState ? "+Globals.sGameState);
        if (Globals.sGameState == Globals.GAME_PAUSE)
        {
            if (Globals.sHasSound)
                audioSoundAP.Stop();
            return;
        }
        else if (Globals.sGameState == Globals.GAME_RESUME)
        {
            if (Globals.sHasSound)
                audioSoundAP.Play();
            Globals.sGameState = Globals.GAME_RUNNING;
        }

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
    public void PlayNormalGame()
    {
        InitGlobalValue();
        InitNewGameValue(Globals.ROLE_ATTACKER);
        Globals.sIsRushGame = false;
    }

    public void PlayRushGame()
    {
        InitGlobalValue();
        InitNewGameValue(Globals.ROLE_ATTACKER);
        Globals.sIsRushGame = true;
    }

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
        Globals.sMatchTimeLeft = Globals.sIsRushGame ? Globals.RUSH_TIME_PER_MATCH : Globals.TIME_PER_MATCH;
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

        if (Globals.sGameState == Globals.GAME_RUNNING
        &&  Globals.sHasSound
        )
        {
            audioSoundAP.Play(0);
        }
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
        audioSoundAP.Stop();
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
        else if (value == 6 && Globals.sGameState == Globals.GAME_MAZE_INIT)
        {
            ResetMaze();
        }
        //Debug.Log("Cheat attackList count ? "+attackerList.Count);
        //Debug.Log("Cheat defenderList count ? "+defenderList.Count);
    }

//=================================================================
//                      maze game implement
//=================================================================
    float leftBounder;
    float rightBounder;
    float upBounder;
    float downBounder;
    float mazeWidth;
    float mazeHeight;
    float cellSize;
    const int MAZE_COLUMN = 7;
    const int MAZE_ROW = 7;
    //int MAZE_ROW;
    Vector3[,] mazeMatrix;
    GameObject[] rowContainer;
    GameObject[] colContainer;
    bool[,] cellVisited;
    const string gcTag = "gcObj";
    const string wallTag = "Wall";
    GameObject[] gcObjects;
    GameObject mazePlayer;
    void InitMazeGame()
    {
        Debug.Log("InitMazeGame");
        Globals.sGameState = Globals.GAME_MAZE_INIT;
        InitGlobalValue();
        InitNewGameValue(Globals.ROLE_ATTACKER);

        wallContainer = new GameObject();
        rowContainer = new GameObject[MAZE_ROW];
        for (int i = 0; i < MAZE_ROW; i++)
        {
            rowContainer[i] = new GameObject();
        }
        colContainer = new GameObject[MAZE_COLUMN];
        for (int i = 0; i < MAZE_COLUMN; i++)
        {
            colContainer[i] = new GameObject();
        }
        cellVisited = new bool[MAZE_COLUMN,MAZE_ROW];
        for (int i = 0; i < MAZE_COLUMN; i++)
            for (int j = 0; j < MAZE_ROW; j++)
                cellVisited[i,j] = false;

        leftBounder = westWall.transform.position.x;
        rightBounder = eastWall.transform.position.x;
        upBounder = enemyFence.transform.position.z;
        downBounder = playerFence.transform.position.z;
        mazeWidth = rightBounder - leftBounder;
        mazeHeight = upBounder - downBounder;
        cellSize = mazeHeight / 9;
        //MAZE_ROW = (int)(mazeHeight / cellSize);

        Debug.Log("InitMazeGame MAZE_COLUMN ? "+MAZE_COLUMN);
        Debug.Log("InitMazeGame MAZE_ROW ? "+MAZE_ROW);
        Debug.Log("InitMazeGame mazeWidth ? "+mazeWidth);
        Debug.Log("InitMazeGame mazeHeight ? "+mazeHeight);
        Debug.Log("InitMazeGame cellSize ? "+cellSize);

        Vector3[,] tempMaze = new Vector3[7,9];
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                tempMaze[i,j] = GetPositon(i,j,cellSize,7,9);
            }
        }

        mazeMatrix = new Vector3[MAZE_COLUMN,MAZE_ROW];
        for (int col = 0; col < 7; col++)
        {
            for (int row = 1; row < 8; row++)
            {
                mazeMatrix[col,row - 1] = tempMaze[col,row];
            }
        }

        //test matrix
        /*
        foreach (Vector3 pos in mazeMatrix)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = pos;
            cube.transform.localScale = new Vector3(0.1f,0.1f,0.1f);    
        }
        */

        CreateColumnCellContainer();
        CreateRowCellContainer();
        GenerateRandomMaze();
    }

    void ResetMaze()
    {
        DestroyAllObjects(wallTag);
        InitMazeGame();
    }

    void DestroyAllObjects(string tag)
    {
        gcObjects = GameObject.FindGameObjectsWithTag (tag);
        
        for(var i = 0 ; i < gcObjects.Length ; i ++)
        {
            Destroy(gcObjects[i]);
        }
    }

    void GenerateRandomMaze()
    {
        Debug.Log("-----------Walk And Kill Init -------------");
        int xCell = Random.Range(0, MAZE_COLUMN-2);
        int yCell = Random.Range(0, MAZE_ROW-2);
        WalkAndKill(xCell, yCell);
        Destroy(rowContainer[0]);
        Destroy(rowContainer[MAZE_ROW - 1]);
        Destroy(colContainer[0]);
        Destroy(colContainer[MAZE_COLUMN - 1]);
        DestroyAllObjects(gcTag);

        //create player
        //ballInstance = (GameObject)Instantiate(ball, 
        //new Vector3(mazeMatrix[3,0].x, mazeMatrix[3,0].y , mazeMatrix[3,0].z - cellSize)
        //, Quaternion.identity);

        mazePlayer = (GameObject)Instantiate(playerSoldier, new Vector3(mazeMatrix[3,0].x, mazeMatrix[3,0].y , mazeMatrix[3,0].z - cellSize), Quaternion.identity);
        mazePlayer.SetActive(false);
    }

    void WalkAndKill(int xCell, int yCell)
    {
        bool meetDeadEnd = false;
        List<int> listDir = new List<int>();
        int step = 0;
        do 
        {
            step++;
            Debug.Log("xCell ? "+xCell+" yCell ? "+yCell);
            GameObject leftObj = new GameObject();
            leftObj.gameObject.tag = gcTag;
            if (LeftAvailable(xCell,yCell))
            {
                leftObj = colContainer[xCell].transform.GetChild(yCell).gameObject;
                listDir.Add(0);
            }

            GameObject rightObj = new GameObject();
            rightObj.gameObject.tag = gcTag;
            if (RightAvailable(xCell,yCell))
            {
                rightObj = colContainer[xCell+1].transform.GetChild(yCell).gameObject;
                listDir.Add(1);
            }

            GameObject upObj = new GameObject();
            upObj.gameObject.tag = gcTag;
            if (UpAvailable(xCell,yCell))
            {
                upObj = rowContainer[yCell+1].transform.GetChild(xCell).gameObject;
                listDir.Add(2);
            }

            GameObject downObj = new GameObject();
            downObj.gameObject.tag = gcTag;
            if (DownAvailable(xCell,yCell))
            {
                downObj = rowContainer[yCell].transform.GetChild(xCell).gameObject;
                listDir.Add(3);
            }

            if (listDir.Count > 0)
            {
                int dir = listDir[Random.Range(0,listDir.Count)];
                Debug.Log("dir ?"+dir);

                if (dir == 0 /*LEFT*/&& LeftAvailable(xCell,yCell))
                {
                    Debug.Log("LEFT DIR");
                    cellVisited[xCell-1,yCell] = true;
                    Destroy(leftObj);
                    //if (!CheckDeadEnd(xCell-1,yCell))
                        xCell--;
                }
                else if (dir == 1 /*RIGHT*/&& RightAvailable(xCell,yCell))
                {
                    Debug.Log("RIGHT DIR");
                    cellVisited[xCell+1,yCell] = true;
                    Destroy(rightObj);   
                    //if (!CheckDeadEnd(xCell+1,yCell))   
                        xCell++;
                }                   
                else if (dir == 2 /*UP*/&& UpAvailable(xCell,yCell))
                {
                    Debug.Log("UP DIR");
                    cellVisited[xCell,yCell+1] = true;
                    Destroy(upObj);
                    //if (!CheckDeadEnd(xCell,yCell+1))  
                        yCell++;
                }                     
                else if (dir == 3 /*DOWN*/&& DownAvailable(xCell,yCell))
                {
                    Debug.Log("DOWN DIR");
                    cellVisited[xCell,yCell-1] = true;
                    Destroy(downObj);
                    //if (!CheckDeadEnd(xCell,yCell-1))  
                        yCell--;
                }
            }
            else
            {
                if (step <= 1)
                {
                    if (xCell > 1)
                        listDir.Add(0);
                    if (xCell < MAZE_COLUMN - 1)
                        listDir.Add(1);
                    if (yCell > 1)
                        listDir.Add(3);
                    if (xCell < MAZE_ROW - 1)
                        listDir.Add(2);
                    int dir = listDir[Random.Range(0,listDir.Count)];
                    GameObject obj = new GameObject();
                    if (dir == 0)
                        obj = colContainer[xCell].transform.GetChild(yCell).gameObject;
                    else if (dir == 1)
                        obj = colContainer[xCell+1].transform.GetChild(yCell).gameObject;
                    else if (dir == 2)
                        obj = rowContainer[yCell+1].transform.GetChild(xCell).gameObject;
                    else if (dir == 3)
                        obj = rowContainer[yCell].transform.GetChild(xCell).gameObject;
                    cellVisited[xCell,yCell] = true;     
                    Destroy(obj);
                }    
                meetDeadEnd = true;
            }
            listDir.Clear();    
        } while (!meetDeadEnd);

        for (int i = 0; i < MAZE_COLUMN - 1; i++)
            for (int j = 0; j < MAZE_ROW - 1; j++)
                if (cellVisited[i,j] == false)
                {
                    WalkAndKill(i,j);
                    break;
                }
    }

    Vector3 GetPositon(int col, int row, float cellSize, int mazeWidth, int mazeHeight)
    {
        Vector3 pos;
        int colOffset = col - mazeWidth / 2;
        int rowOffset = row - mazeHeight / 2;
        pos.y = 0;
        pos.x = colOffset * cellSize;
        pos.z = rowOffset * cellSize;
        //Debug.Log("Pos for cell["+col+"]["+row+"] ? ("+pos.x+", "+pos.y+", "+pos.z+")");
        return pos;
    }
    void CreateWall(Vector3 fromPos, Vector3 endPos, bool horizon, int index)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 direction = endPos - fromPos;
        float distance = direction.magnitude;
        if (horizon)
        {
            cube.transform.localScale = new Vector3(distance,0.1f,0.1f);
            cube.transform.position = new Vector3(fromPos.x + distance / 2, 0.0f, fromPos.z);
            cube.transform.parent = rowContainer[index].transform;
        }
        else
        {
            cube.transform.localScale = new Vector3(0.1f,0.1f,distance);
            cube.transform.position = new Vector3(fromPos.x, 0.0f, fromPos.z + distance / 2);
            cube.transform.parent = colContainer[index].transform;
        }
        cube.GetComponent<MeshRenderer>().material.color = Color.red;
        cube.gameObject.tag = wallTag;
    }
    bool isValidCell(int col, int row)
    {
        return col >= 0 && col < MAZE_COLUMN - 1 && row >= 0 && row < MAZE_ROW - 1;
    }

    void CreateColumnCellContainer()
    {
        for(int i = 0; i < MAZE_COLUMN; i++)
        {
            for(int j = 0; j < MAZE_COLUMN - 1; j++)
            {
                CreateWall(mazeMatrix[i,j],mazeMatrix[i,j+1],false,i);              
            }
        }
    }

    void CreateRowCellContainer()
    {
        for(int i = 0; i < MAZE_COLUMN - 1; i++)
        {
            for(int j = 0; j < MAZE_ROW; j++)
            {
                CreateWall(mazeMatrix[i,j],mazeMatrix[i+1,j],true,j);              
            }
        }
    }

    bool CheckDeadEnd(int xCell, int yCell)
    {
        bool leftAvailable = LeftAvailable(xCell,yCell);
        bool rightAvailable = LeftAvailable(xCell,yCell);
        bool upAvailable = UpAvailable(xCell,yCell);
        bool downAvailable = DownAvailable(xCell,yCell);
        Debug.Log("CheckDeadEnd for cell x ? "+xCell+" y ? "+yCell);
        Debug.Log("leftAvailable ? "+leftAvailable);
        Debug.Log("rightAvailable ? "+rightAvailable);
        Debug.Log("upAvailable ? "+upAvailable);
        Debug.Log("downAvailable ? "+downAvailable);
        return !leftAvailable&&!rightAvailable&&!upAvailable&&!downAvailable;
    }

    bool LeftAvailable(int xCell, int yCell)
    {
        return isValidCell(xCell,yCell) && (xCell - 1 >= 0) && cellVisited[xCell - 1,yCell] == false;
    }

    bool RightAvailable(int xCell, int yCell)
    {
        return isValidCell(xCell,yCell) && (xCell + 1 < MAZE_COLUMN - 1) && cellVisited[xCell + 1,yCell] == false;
    }
    bool DownAvailable(int xCell, int yCell)
    {
        return isValidCell(xCell,yCell) && (yCell - 1 >= 0) && cellVisited[xCell,yCell-1] == false;
    }
    bool UpAvailable(int xCell, int yCell)
    {
        return isValidCell(xCell,yCell) && (yCell + 1 < MAZE_ROW - 1) && cellVisited[xCell,yCell+1] == false;
    }
    bool HasUnVisitCell()
    {
        for (int i = 0; i < MAZE_COLUMN - 1; i++)
            for (int j = 0; j < MAZE_ROW; j++)
                if (cellVisited[i,j] == false)
                    return true;
        return false;
    }
}
