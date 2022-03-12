using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGeneratorManager : MonoBehaviour
{
    private static LevelGeneratorManager instance;

    public static LevelGeneratorManager Instance { get { return instance; } }

    public int levelsCompleted = 0;
    public int initalNrRooms;
    public int nrRoomsIncrement;

    private LevelGenerator lg;

    private void Awake()
    {
        if (instance != null && instance != this)
            Destroy(this.gameObject);
        else instance = this;

        DontDestroyOnLoad(this.gameObject);

        
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }

    public void LevelCompleted()
    {
        ++levelsCompleted;
        ScoreManager.Instance.LevelCompleted(levelsCompleted);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void StartNewLevelGeneration()
    {
        lg = GameObject.FindObjectOfType<LevelGenerator>();

        int nrRooms = initalNrRooms + (levelsCompleted * nrRoomsIncrement);

        lg.StartGeneration(nrRooms);
    }

    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        StartNewLevelGeneration();
    }

    public void ResetLevelGeneratorSettings()
    {
        levelsCompleted = 0;
    }
}
