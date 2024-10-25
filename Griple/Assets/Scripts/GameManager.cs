using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Net;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform albumContainer;
    [SerializeField] private Button deleteButton;
    [SerializeField] private AlbumEntryPrefab albumEntryPrefab;
    [SerializeField] private GameObject loadingGameObject;

    private int currentAlbumID = 1;

    private AlbumEntryPrefab selectedAlbum;
    private Coroutine loadingCoroutine;
    private UnityWebRequest webRequest;

    private List<AlbumData> albumDataList = new List<AlbumData>();
    private Dictionary<int, AlbumEntryPrefab> albumPrefabsDict = new Dictionary<int, AlbumEntryPrefab>();

    private const string albumURL = "https://jsonplaceholder.typicode.com/albums/";
    private const string photoEndpoint = "/photos";

    public static GameManager Instance;

    [System.Serializable]
    public class AlbumDataArray
    {
        public AlbumData[] albums;
    }

    [System.Serializable]
    public class AlbumData
    {
        public int albumId;
        public int id;
        public string title;
        public string url;
        public string thumbnailUrl;
    }

    private void Awake()
    {
        deleteButton.interactable = false;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogError("Two instances of GameManager exist, deleting this one.");
            Destroy(gameObject);
        }
    }  

    private void Start ()
    {
        loadingGameObject.SetActive(true);

        string url = $"{albumURL}{currentAlbumID}{photoEndpoint}";

        loadingCoroutine = StartCoroutine(GetAlbumResponse(url));
    }

    private IEnumerator GetAlbumResponse (string url)
    {
        using (webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(webRequest.downloadHandler.text);
                albumDataList = JsonConvert.DeserializeObject<List<AlbumData>>(webRequest.downloadHandler.text);

                loadingGameObject.SetActive(false);

                for (int i = 0; i < albumDataList.Count; i++)
                {
                    AlbumEntryPrefab entry = Instantiate(albumEntryPrefab, albumContainer);
                    entry.Initialize(albumDataList[i]);
                    albumPrefabsDict.Add(albumDataList[i].id, entry);
                }
            }
            else
            {
                Debug.LogError($"Error receiving data: {webRequest.error}");
            }
        }

        webRequest = null;
        loadingCoroutine = null;
    }   

    public void SelectAlbumEntry (int id)
    {
        if (loadingCoroutine != null || (selectedAlbum != null && selectedAlbum.Data.id == id && selectedAlbum.SpriteLoaded))
        {
            return;
        }

        if (selectedAlbum != null)
        {
            selectedAlbum.SetState(false);
        }

        if (albumPrefabsDict.TryGetValue(id, out selectedAlbum))
        {
            selectedAlbum.SetState(true);
            selectedAlbum.SetLoadingText(true);
            deleteButton.interactable = true;

            if (selectedAlbum.SpriteLoaded == false)
            {
                loadingCoroutine = StartCoroutine(LoadImage());
            }
        }
    }

    private IEnumerator LoadImage()
    {
        using (webRequest = UnityWebRequestTexture.GetTexture(selectedAlbum.Data.url))
        {
            webRequest.timeout = 10;
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(webRequest);
                if (selectedAlbum != null)
                {
                    selectedAlbum.SetImage(Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
                }
            }
            else
            {
                Debug.LogError($"Error receiving data: {webRequest.error}");
            }

            selectedAlbum.SetLoadingText(false);
        }

        webRequest = null;
        loadingCoroutine = null;
    }

    public void DeleteSelectedAlbumEntry()
    {
        if (selectedAlbum != null)
        {
            CancelWebRequest();

            Destroy(selectedAlbum.gameObject);
            deleteButton.interactable = true;
        }
    }

    private void CancelWebRequest()
    {
        if (webRequest != null)
        {
            webRequest.Abort();
            webRequest = null;
        }

        if (loadingCoroutine != null)
        {
            StopCoroutine(loadingCoroutine);
            loadingCoroutine = null;
        }
    }

    private void OnDisable()
    {
        CancelWebRequest();
    }

    private void OnDestroy()
    {
        CancelWebRequest();
    }
}