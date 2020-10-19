using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Globals.sGameState == Globals.GAME_INIT)
        {
            Globals.sGameState = Globals.GAME_MENU;
        }
        else if (Globals.sGameState == Globals.GAME_MENU)
        {
            transform.GetChild(Globals.MENU).gameObject.SetActive(true);
            transform.GetChild(Globals.OPTION).gameObject.SetActive(false);
            transform.GetChild(Globals.IGM).gameObject.SetActive(false);
            transform.GetChild(Globals.POPUP).gameObject.SetActive(false);
            ShowAllUI(false);
        }
        else if (Globals.sGameState == Globals.GAME_OPTION)
        {
            transform.GetChild(Globals.OPTION).gameObject.SetActive(true);
        }
        else if (Globals.sGameState == Globals.GAME_PAUSE)
        {
            GamePause();
            return;
        }
        else if (Globals.sGameState == Globals.GAME_RESUME)
        {
            GameResume();
        }

        if (Globals.sGameState == -1)
        {
            transform.GetChild(Globals.ENEMY_INFO).GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = 0.0f;
            transform.GetChild(Globals.ENEMY_INFO).GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = 0.0f;
            transform.GetChild(Globals.PLAYER_INFO).GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = 0.0f;
            transform.GetChild(Globals.PLAYER_INFO).GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = 0.0f;
            transform.GetChild(Globals.AR).transform.GetChild(0).GetChild(0).GetComponent<Dropdown>().value = 0;
        }
        else if (Globals.sGameState == Globals.GAME_RUNNING || Globals.sGameState >= Globals.GAME_MAZE_INIT)
        {
            transform.GetChild(Globals.POPUP).gameObject.SetActive(false);
            transform.GetChild(Globals.MENU).gameObject.SetActive(false);
            transform.GetChild(Globals.OPTION).gameObject.SetActive(false);
            ShowAllUI(true);
            if (Globals.sGameState >= Globals.GAME_MAZE_INIT)
                ShowEnergyBar(false);
            UpdateEnergyBar();
            UpdateTimeBar();
            UpdatePlayerInfoBar();
        }
        else if (Globals.sGameState == Globals.GAME_OVER)
        {
            //transform.GetChild(Globals.AR).transform.GetChild(0).GetChild(0).GetComponent<Dropdown>().value = 0;
        }
        else if (Globals.sGameState == Globals.GAME_SHOW_MESSAGE)
        {
            ShowMessage();
        }
    }

    void UpdateTimeBar()
    {
        string sec = string.Format("{0}s", (int)Globals.sMatchTimeLeft);
        transform.GetChild(Globals.TIME_INFO).transform.GetChild(2).GetChild(0).GetComponent<Text>().text = sec;
    }

    void UpdateEnergyBar()
    {
        if (Globals.sAttackerEnergyPoint < Globals.MAX_ENERGY_POINT)
        {
            Globals.sAttackerEnergyPoint += (Globals.sIsRushGame ? Globals.RUSH_ENERGY_REGERENATION : Globals.ENERGY_REGERENATION) * Time.deltaTime;
            float atkPercent = Globals.sAttackerEnergyPoint / Globals.MAX_ENERGY_POINT;
            int percent = (int)(atkPercent * 100);
            float offset = ((100 / Globals.MAX_ENERGY_POINT) - (int)(100 / Globals.MAX_ENERGY_POINT)) / 100;
            //Debug.Log("attackerEnergyPoint ? "+attackerEnergyPoint);
            //Debug.Log("atkPercent ? "+atkPercent);
            if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
            {
                transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = atkPercent;
                if (percent % ((int)(100 / Globals.MAX_ENERGY_POINT)) == 0)
                    transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = atkPercent + offset ;
            }
            else
            {
                transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = atkPercent;
                if (percent % ((int)(100 / Globals.MAX_ENERGY_POINT)) == 0)
                    transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = atkPercent + offset ;
            }
        }

        if (Globals.sDefenderEnergyPoint < Globals.MAX_ENERGY_POINT)
        {
            Globals.sDefenderEnergyPoint += (Globals.sIsRushGame ? Globals.RUSH_ENERGY_REGERENATION : Globals.ENERGY_REGERENATION) * Time.deltaTime;
            float defPercent = Globals.sDefenderEnergyPoint / Globals.MAX_ENERGY_POINT;
            int percent = (int)(defPercent * 100);
            float offset = ((100 / Globals.MAX_ENERGY_POINT) - (int)(100 / Globals.MAX_ENERGY_POINT)) / 100;
            //Debug.Log("defenderEnergyPoint ? "+defenderEnergyPoint);
            //Debug.Log("defPercent ? "+defPercent);
            if (Globals.sPlayerRole == Globals.ROLE_DEFENDER)
            {
                transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = defPercent;
                if (percent % ((int)(100 / Globals.MAX_ENERGY_POINT)) == 0)
                   transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = defPercent + offset ;
            }
            else
            {
                transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = defPercent;
                if (percent % ((int)(100 / Globals.MAX_ENERGY_POINT)) == 0)
                    transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount = defPercent + offset ;
            }
           
        }

        if (Globals.sPlayerSpendEnergyPercent > 0)
        {
            transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount -= Globals.sPlayerSpendEnergyPercent;
            Globals.sPlayerSpendEnergyPercent = 0;  
        }

        if (Globals.sEnemySpendEnergyPercent > 0)
        {
            transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(0).GetChild(2).GetComponent<Image>().fillAmount -= Globals.sEnemySpendEnergyPercent;      
            Globals.sEnemySpendEnergyPercent = 0;
        }
    }

    void UpdatePlayerInfoBar()
    {
        transform.GetChild(Globals.PLAYER_INFO).transform.GetChild(1).GetComponent<Text>().text = "Player " +
                                                                        (Globals.sPlayerRole == Globals.ROLE_ATTACKER ? Globals.STR_ATTACKER : Globals.STR_DEFENDER);
        transform.GetChild(Globals.ENEMY_INFO).transform.GetChild(1).GetComponent<Text>().text = "Enemy " +
                                                                        (Globals.sPlayerRole == Globals.ROLE_ATTACKER ? Globals.STR_DEFENDER : Globals.STR_ATTACKER);
    }

    void ShowMessage()
    {
        //Debug.Log("ShowMessage sMatchCount ? "+Globals.sMatchCount);
        if (Globals.sMatchCount > Globals.MAX_MATCH)
        {
            transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_GAME_FINISHED;
            string body = "";
            if (Globals.sPlayerScore > Globals.sEnemyScore)
                body = Globals.STR_PLAYER_WIN;
            else if (Globals.sPlayerScore < Globals.sEnemyScore)
                body = Globals.STR_PLAYER_LOSE;
            else
                body = Globals.STR_GAME_DRAW;
            transform.GetChild(Globals.POPUP).transform.GetChild(1).GetComponent<Text>().text = body;
            transform.GetChild(Globals.POPUP).transform.GetChild(4).GetChild(0).GetComponent<Text>().text = Globals.STR_OK;
            string scoreInfo = string.Format("Player score : {0}  Enemy score : {1}", Globals.sPlayerScore, Globals.sEnemyScore);
            transform.GetChild(Globals.POPUP).transform.GetChild(2).GetComponent<Text>().text = scoreInfo;
            if (Globals.sPlayerScore == Globals.sEnemyScore)
            {
                transform.GetChild(Globals.POPUP).transform.GetChild(3).GetComponent<Text>().text = Globals.STR_PLAY_MAZE;
                //Globals.sGameState = Globals.GAME_MAZE_INIT;
                ShowEnergyBar(false);
                transform.GetChild(Globals.POPUP).gameObject.SetActive(true);
                return;
            }
            else
                transform.GetChild(Globals.POPUP).transform.GetChild(3).GetComponent<Text>().text = Globals.STR_PLAY_AGAIN;
        } 
        else 
        {
            if (Globals.sMessage == Globals.STR_NO_ACTIVE_FRIENDS) //Attacker Lose
            {
                if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
                {
                    transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_PLAYER_LOSE;
                }
                else
                {
                    transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_PLAYER_WIN;
                }
            } 
            else if (Globals.sMessage == Globals.STR_REACH_GATE)
            {
                if (Globals.sPlayerRole == Globals.ROLE_ATTACKER)
                {
                    transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_PLAYER_WIN;
                }
                else
                {
                    transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_PLAYER_LOSE;;
                }
            }
            else if (Globals.sMessage == Globals.STR_TIME_OUT)
            {
                transform.GetChild(Globals.POPUP).transform.GetChild(0).GetComponent<Text>().text = Globals.STR_GAME_DRAW;
            }
            transform.GetChild(Globals.POPUP).transform.GetChild(1).GetComponent<Text>().text = Globals.sMessage;
            string matchInfo = string.Format("Match #{0}", Globals.sMatchCount);
            transform.GetChild(Globals.POPUP).transform.GetChild(2).GetComponent<Text>().text = matchInfo;
            string scoreInfo = string.Format("Player score : {0}  Enemy score : {1}", Globals.sPlayerScore, Globals.sEnemyScore);
            transform.GetChild(Globals.POPUP).transform.GetChild(3).GetComponent<Text>().text = scoreInfo;
            transform.GetChild(Globals.POPUP).transform.GetChild(4).GetChild(0).GetComponent<Text>().text = Globals.sMatchCount == Globals.MAX_MATCH ? 
                                                                        Globals.STR_OK : Globals.STR_NEXT_MATCH;
        }
        transform.GetChild(Globals.POPUP).gameObject.SetActive(true);
        Globals.sGameState = -1;
    }

    public void GamePause()
    {
        transform.GetChild(Globals.IGM).gameObject.SetActive(true);
        Time.timeScale = 0.0f;
        Globals.sGameState = Globals.GAME_PAUSE;
    }

    public void GameResume()
    {
        transform.GetChild(Globals.IGM).gameObject.SetActive(false);
        Time.timeScale = 1.0f;
        Globals.sGameState = Globals.GAME_RUNNING;
    }

    void ShowEnergyBar(bool active)
    {
        transform.GetChild(Globals.PLAYER_INFO).gameObject.SetActive(active);
        transform.GetChild(Globals.ENEMY_INFO).gameObject.SetActive(active);
    }

    void ShowAllUI(bool active)
    {
        ShowEnergyBar(active);
        transform.GetChild(Globals.TIME_INFO).gameObject.SetActive(active);
        transform.GetChild(Globals.AR).gameObject.SetActive(active);
    }

    public void GameOption()
    {
        Globals.sGameState = Globals.GAME_OPTION;
    }

    public void GameOptionBack()
    {
        Time.timeScale = 1.0f;//resume for IGM case
        Globals.sGameState = Globals.GAME_MENU;
    }
}
