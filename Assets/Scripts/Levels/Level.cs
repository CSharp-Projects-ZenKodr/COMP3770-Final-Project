﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelGenerator))]
public class Level : MonoBehaviour
{

    /*
      Contains relevant data for a level: score, how many enemies defeated, how many sections are completed..

    10 sections before level complete section. (Level 3 must have boss section first)
     */

    public int levels = 1;
    static int totalLevels = 4;

    // keeps track of players collectable score
    public Color midPointShow;
    public GameObject playerPrefab;
    public Player player;
    public int score;
    public int enemiesKilled;

    public int playerLives = 10;

    private SmoothFollowCam cam;
    private LevelGenerator levelGenerator;
    private bool keepPolling = true;
    private bool levelLoaded = false;
    private bool levelComplete = false;
    private bool isPaused = false;

    // Start is called before the first frame update
    void Awake()
    {

        levels = PlayerPrefs.GetInt("level");
        levels++;
        levelGenerator = gameObject.GetComponent<LevelGenerator>();
        LoadLevel();
    }

    // Update is called once per frame
    void Update()
    {
        if(levelLoaded && keepPolling) {

            // When the player has passed the current section's end point
            if(levelGenerator.GetActiveSection().GetRightAnchor().x < player.transform.position.x) {

                // delete the last section after camera can't see it.
                if(levelGenerator.GetActiveSection().GetRightAnchor().x + 10 < cam.transform.position.x) 
                    levelGenerator.DeleteLastSections();


                // Update player spawn position to the new section
                else levelGenerator.PlayerHitCheckpoint();

            }

            // Spawn new section when passing the midpoint of the current section
            if(player.transform.position.x > levelGenerator.GetCurrentMidpoint().x) {

                // Spawn special section (levelcomplete)
                if(levelGenerator.GetSectionCount() == 10) {
                    levelGenerator.SpawnEndingSection();

                // Regular section spawn everywhere else 
                } else if(levelGenerator.GetSectionCount() <= 10) {

                    levelGenerator.SpawnNewSection();

                }
            }

            if(levelGenerator.GetSectionCount() > 10) keepPolling = false;
        }
        
    }

    // As in start up the level...
	private void LoadLevel() {

        // Initialize camera
        cam = Camera.main.gameObject.GetComponent<SmoothFollowCam>();
        cam.gameObject.SetActive(false);

        
        
        // Initialize the first section and load the player into the scene:
        levelGenerator.SpawnNewSection();
        RespawnPlayer();
        cam.gameObject.SetActive(true);
        levelLoaded = true;
	}

    public void LevelComplete() {

        if(!levelComplete) {
            levelComplete = true;
            StartCoroutine(LoadNextLevel());
        }
	}

    public void RespawnPlayer() {
        
        playerLives--;

        // Game Over
        if(playerLives <= 0) {
            LoadMainMenu();         // can change..
		}
        GameObject newObj = Instantiate(playerPrefab);
        player = newObj.GetComponent<Player>();
        newObj.transform.position = levelGenerator.GetPlayerSpawnPosition();
        cam.SetTarget(player.transform);
	}

	public void OnDrawGizmos() {

        Gizmos.color = midPointShow;

        Gizmos.DrawSphere((Vector3)levelGenerator.GetCurrentMidpoint(), 1f);
        
	}

	private void OnValidate() {
        levelGenerator = gameObject.GetComponent<LevelGenerator>();
	}

    public Transform GetPlayer() { 
        if(player != null) return player.transform;
        else return null;
    }

    public void IncreaseScore(int newScore) { score += newScore; }

    // Load the next level after a few seconds.
    private IEnumerator LoadNextLevel() {

        yield return new WaitForSeconds(2f);

        PlayerPrefs.SetInt("level", levels);
        if (levels < totalLevels) SceneManager.LoadScene(levels);
        else SceneManager.LoadScene(0);
    }
    
    public void IncreaseKilled(int newScore) { enemiesKilled += newScore; }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("Scenes/Main Menu");
        PlayerPrefs.SetInt("coins", PlayerPrefs.GetInt("coins") + score);
    }

    public void PauseButton()
    {
        if (!isPaused)
        {
            Time.timeScale = 0;
            isPaused = true;
        }
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;
        }
    }

}
