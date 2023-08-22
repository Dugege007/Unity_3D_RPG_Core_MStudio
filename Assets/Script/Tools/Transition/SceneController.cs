using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;

public class SceneController : Singleton<SceneController>, IEndGameObserver
{
    public GameObject playerPrefab;
    public SceneFader sceneFadePrefab;
    private bool fadeFinished;
    private GameObject player;
    private NavMeshAgent playerAgent;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        GameManager.Instance.AddObserver(this);
        fadeFinished = true;
    }

    public void TransitionToDestination(TransitionPoint transitionPoint)
    {
        switch (transitionPoint.transitionType)
        {
            case TransitionPoint.TransitionType.SameScene:
                StartCoroutine(Transition(SceneManager.GetActiveScene().name, transitionPoint.desinatonTag));
                break;
            case TransitionPoint.TransitionType.DifferentScene:
                StartCoroutine(Transition(transitionPoint.sceneName, transitionPoint.desinatonTag));
                break;
        }
    }

    public void TransitionToLoadGame()
    {
        StartCoroutine(LoadLevel(SaveManager.Instance.SceneName));
    }

    public void TransitionToMain()
    {
        StartCoroutine(LoadMain());
    }

    IEnumerator Transition(string sceneName, TransitionDestination.DestinationTag destinationTag)
    {
        //TODO:保存数据
        SaveManager.Instance.SavePlayerData();

        if (SceneManager.GetActiveScene().name!=sceneName)
        {
            //可以加入fader

            //异步加载场景
            yield return SceneManager.LoadSceneAsync(sceneName);
            //加载玩家
            yield return Instantiate(playerPrefab, GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            //读取玩家数据
            SaveManager.Instance.LoadPlayerData();
            //中断协程，从协程中跳出
            yield break;
        }
        else
        {
            player = GameManager.Instance.playerStats.gameObject;
            playerAgent = player.GetComponent<NavMeshAgent>();
            playerAgent.enabled = false;
            player.transform.SetPositionAndRotation(GetDestination(destinationTag).transform.position, GetDestination(destinationTag).transform.rotation);
            playerAgent.enabled = true;
            yield return null;
        }
    }

    IEnumerator LoadLevel(string scene)
    {
        SceneFader fade = Instantiate(sceneFadePrefab);

        if (scene!="")
        {
            yield return StartCoroutine(fade.FadeOut(fade.fadeOutDuration));
            yield return SceneManager.LoadSceneAsync(scene);
            yield return player = Instantiate(playerPrefab, GameManager.Instance.GetEntrance().position, GameManager.Instance.GetEntrance().rotation);

            //保存游戏
            SaveManager.Instance.SavePlayerData();

            yield return StartCoroutine(fade.FadeIn(fade.fadeInDuration));
            yield break;
        }
    }

    IEnumerator LoadMain()
    {
        SceneFader fade = Instantiate(sceneFadePrefab);
        yield return StartCoroutine(fade.FadeOut(fade.fadeOutDuration));
        yield return SceneManager.LoadSceneAsync("MainScene");
        yield return StartCoroutine(fade.FadeIn(fade.fadeInDuration));
        yield break;
    }

    public void TransitionToFirstLevel()
    {
        StartCoroutine(LoadLevel("FirstScene"));
    }

    private TransitionDestination GetDestination(TransitionDestination.DestinationTag destinationTag)
    {
        //找到场景中所有的传送目标点
        var entrances = FindObjectsOfType<TransitionDestination>();

        for (int i = 0; i < entrances.Length; i++)
        {
            if (entrances[i].portalPointTag == destinationTag)
            {
                return entrances[i];
            }
        }
        return null;
    }

    public void EndNotify()
    {
        if (fadeFinished)
        {
            StartCoroutine(LoadMain());
            fadeFinished = false;
        }
    }
}
