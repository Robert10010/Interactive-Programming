using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour
{
    //Send a GET request; onSuccess receives the response text, onError receives the error message
    public IEnumerator SendRequest(string url, Action<string> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError(request.error);
                onError?.Invoke(request.error);
            }
        }
    }

    //Download an image (e.g. a Pokémon sprite) as a Texture2D
    public IEnumerator SendTextureRequest(string url, Action<Texture2D> onSuccess, Action<string> onError)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(DownloadHandlerTexture.GetContent(request));
            }
            else
            {
                Debug.LogError(request.error);
                onError?.Invoke(request.error);
            }
        }
    }
}
