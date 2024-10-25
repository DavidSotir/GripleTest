using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlbumEntryPrefab : MonoBehaviour
{
    [HideInInspector] public GameManager.AlbumData Data;
    [HideInInspector] public bool SpriteLoaded;

    [SerializeField] private GameObject selectedState;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private Image photo;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private TextMeshProUGUI id;


    public void Initialize (GameManager.AlbumData albumData)
    {
        Data = albumData;

        title.text = Data.title;
        id.text = $"ID: {Data.id}";
    }

    public void SetImage (Sprite inSprite)
    {
        SpriteLoaded = true;
        photo.color = Color.white;
        photo.sprite = inSprite;
    }

    public void Select()
    {
        GameManager.Instance.SelectAlbumEntry(Data.id);
    }

    public void SetState (bool state)
    {
        selectedState.SetActive(state);
    }

    public void SetLoadingText(bool state)
    {
        loadingText.SetActive(state);
    }
}
