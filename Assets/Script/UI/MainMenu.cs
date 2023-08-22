using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

public class MainMenu : MonoBehaviour
{
    private Button newGameBtn;
    private Button continueBtn;
    private Button quitBtn;

    PlayableDirector director;

    private void Awake()
    {
        newGameBtn = transform.GetChild(1).GetComponent<Button>();
        continueBtn = transform.GetChild(2).GetComponent<Button>();
        quitBtn = transform.GetChild(3).GetComponent<Button>();

        newGameBtn.onClick.AddListener(PlayTimeLine);
        continueBtn.onClick.AddListener(ContinueGame);
        quitBtn.onClick.AddListener(QuitGame);

        director = FindObjectOfType<PlayableDirector>();
        director.played += NewGame;
    }

    private void PlayTimeLine()
    {
        director.Play();
    }

    private void NewGame(PlayableDirector obj)
    {
        //清除所有游戏记录
        PlayerPrefs.DeleteAll();
        //转换场景
        SceneController.Instance.TransitionToFirstLevel();
    }

    private void ContinueGame()
    {
        //转换场景 读取进度
        SceneController.Instance.TransitionToLoadGame();
    }

    private void QuitGame()
    {
        //退出程序
        Application.Quit();
        Debug.Log("退出游戏");
    }

}
