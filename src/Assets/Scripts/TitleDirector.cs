using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleDirector : MonoBehaviour
{
    void Update()
    {
        if(Input.anyKey)
        {
            Invoke("ChangeScene", 1.0f);// �x�����s
        }
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("PlayScene");
    }
}
