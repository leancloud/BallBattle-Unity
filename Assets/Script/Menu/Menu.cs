using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LeanCloud.Play;

public class Menu : MonoBehaviour
{
    public InputField roomIdInputField;

    void Start() {
        LeanCloud.Play.Logger.LogDelegate = (level, info) => {
            Debug.Log($"[{level}] {info}");
        };
    }

    public async void OnJoinBtnClicked() {
        var roomId = roomIdInputField.text;
        if (string.IsNullOrEmpty(roomId)) {
            return;
        }
        var random = Random.Range(1, 1000000);
        var userId = random.ToString();
        var client = LeanCloudUtils.InitClient(userId);
        await client.Connect();
        await client.JoinOrCreateRoom(roomId);
        client.PauseMessageQueue();
        SceneManager.LoadScene("Battle");
    }
}
