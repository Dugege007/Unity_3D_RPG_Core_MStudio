using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : Singleton<SaveManager>
{
    //保存Player当前所在的场景名
    private string sceneName = "";
    public string SceneName
    {
        get { return PlayerPrefs.GetString(sceneName); }
    }

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneController.Instance.TransitionToMain();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SavePlayerData();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadPlayerData();
        }
    }

    public void SavePlayerData()
    {
        Save(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void LoadPlayerData()
    {
        Load(GameManager.Instance.playerStats.characterData, GameManager.Instance.playerStats.characterData.name);
    }

    public void Save(Object data,string key)    //Object 最根本的基类
    {
        //Json 可以将数据序列化，以字符串形式保存，可以保存ScriptObject
        var jsonData = JsonUtility.ToJson(data, true);

        //PlayerPrefs 是Unity中自带的一种存储数据的方法，会在硬盘上产生数据文件，会根据不同平台自动选择保存数据的位置，只能保存三种数据类型：int float string
        //PlayerPrefs.SetString() 可以将游戏数据和Json数据以键值对的形式匹配，并保存在磁盘上
        PlayerPrefs.SetString(key, jsonData);

        //保存场景名
        PlayerPrefs.SetString(sceneName, SceneManager.GetActiveScene().name);

        //保存数据
        PlayerPrefs.Save();
    }

    public void Load(Object data, string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(key), data);
        }
    }
}
