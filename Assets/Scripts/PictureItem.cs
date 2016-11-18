using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PictureItem : MonoBehaviour {
    [HideInInspector]
    public PictureBrowser manager;
    public FileSystemInfo info;

    public Text label;
    public RawImage image;

    public void OnButtonClick()
    {
        manager.OnClickItemPicked(this);
    }
}
