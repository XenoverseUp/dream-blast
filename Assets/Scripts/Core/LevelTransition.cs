using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum ActiveScene {
    MainMenu,
    Level
};



public class LevelLoader : MonoBehaviour {
    [SerializeField] private string mainSceneName;
    [SerializeField] private string levelSceneName;
    [SerializeField] private Animator animator;
    [SerializeField] private float transitionDuration = 1.0f;


    private void Start() {

    }


    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetActiveScene(ActiveScene scene) {
        
        StartCoroutine(this.LoadLevel(scene));
        
    }

    IEnumerator LoadLevel(ActiveScene scene) {
        animator.SetTrigger("Start");
        yield return new WaitForSeconds(transitionDuration);

        switch (scene) {
            case ActiveScene.MainMenu: 
                SceneManager.LoadScene(mainSceneName);
                break;
            case ActiveScene.Level:
                SceneManager.LoadScene(levelSceneName);
                break;
        }
        
    }
}
