using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TerminalFile
{
    public string fileName;
    [TextArea] public string content;
    public Item itemToGive;
}

public class TerminalUI : MonoBehaviour
{
    public Transform fileListParent;
    public GameObject fileButtonPrefab;
    public TMP_Text contentText;
    public Button downloadButton;
    public TerminalFile[] files;

    private TerminalFile currentFile;
    private GameObject player;

    private void Start()
    {
        gameObject.SetActive(false);
        PopulateFileList();
        downloadButton.onClick.AddListener(DownloadCurrentFile);
    }

    private void PopulateFileList()
    {
        foreach (Transform child in fileListParent)
            Destroy(child.gameObject);

        foreach (var f in files)
        {
            GameObject btnGO = Instantiate(fileButtonPrefab, fileListParent);
            btnGO.GetComponentInChildren<TMP_Text>().text = f.fileName;
            TerminalFile fileRef = f;
            btnGO.GetComponent<Button>().onClick.AddListener(() => ShowFile(fileRef));
        }
    }

    private void ShowFile(TerminalFile file)
    {
        currentFile = file;
        if (contentText != null)
            contentText.text = file.content;
    }

    private void DownloadCurrentFile()
    {
        if (currentFile == null || currentFile.itemToGive == null) return;

        if (player == null)
            player = GameObject.FindWithTag("Player");

        Inventory inv = player.GetComponent<Inventory>();
        if (inv != null)
            inv.AddItem(currentFile.itemToGive, 1);

        Debug.Log($"File '{currentFile.fileName}' added to inventory!");
    }
}