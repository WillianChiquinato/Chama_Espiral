using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-1000)]
public class LevelTransicaoController : MonoBehaviour
{
    public Animator animator;

    public void Transicao(string sceneName)
    {
        StartCoroutine(loadScene(sceneName));
    }

    IEnumerator loadScene(string sceneName)
    {
        animator.SetTrigger("start");

        yield return new WaitForSeconds(0.8f);

        SceneManager.LoadScene(sceneName);
    }
}
