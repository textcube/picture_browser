using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PictureBrowser : MonoBehaviour {
    public GameObject itemPrefab;
    public Transform grid;
    public GameObject popup;
    public RawImage popupImage;

    int maxWidth = 80;
    int maxHeight = 60;
    string[] extFilters = { ".jpg", ".png", ".bmp", ".tif", ".gif" };

    DirectoryInfo rootDir;
    DirectoryInfo nowDir;

    void Start()
    {
        InitPictureList();
        popup.SetActive(false);
    }

    public void OnClickPopupClose()
    {
        popup.SetActive(false);
    }

    public void OnClickItemPicked(PictureItem picItem)
    {
        FileSystemInfo fInfo = picItem.info;
        if (fInfo is DirectoryInfo)
        {
            DrawPictureList((DirectoryInfo)fInfo);
        }
        else
        {
            //LoadImageFile(fInfo.FullName, popupImage, 280, 280);
            LoadBitmapFile(fInfo.FullName, popupImage, 280, 280);
            popup.SetActive(true);
        }
    }

    void InitPictureList()
    {
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
        DirectoryInfo dir = new DirectoryInfo(folder);
        rootDir = dir.Parent;
        DrawPictureList(dir);
    }

    public void DrawPictureList(DirectoryInfo dir)
    {
        nowDir = dir;
        FileSystemInfo[] list = dir.GetFileSystemInfos();

        System.Array.Sort(list, delegate (FileSystemInfo f1, FileSystemInfo f2)
        {
            if (f1 is DirectoryInfo && f2 is FileInfo) return -1;
            return string.Compare(f1.Name, f2.Name);
        });

        int total = list.Length;

        foreach (Transform tf in grid)
            Destroy(tf.gameObject);

        if (!rootDir.FullName.Equals(dir.Parent.FullName))
            CreatePictureItem(dir.Parent);

        for (int i = 0; i < total; i++)
        {
            FileSystemInfo fInfo = list[i];
            CreatePictureItem(fInfo);
        }
    }

    public void CreatePictureItem(FileSystemInfo fInfo)
    {
        if (fInfo is FileInfo)
        {
            string ext = fInfo.Extension.ToLower();
            if (System.Array.IndexOf(extFilters, ext) == -1) return;
        }

        GameObject go = Instantiate<GameObject>(itemPrefab);
        Transform tf = go.transform;
        tf.SetParent(grid);
        tf.localScale = Vector3.one;
        PictureItem script = go.GetComponent<PictureItem>();
        script.manager = this;
        script.info = fInfo;
        string fname = fInfo.Name;

        if (fInfo is FileInfo)
        {
            //LoadImageFile(fInfo.FullName, script.image, maxWidth, maxHeight, true);
            LoadBitmapFile(fInfo.FullName, script.image, maxWidth, maxHeight, true);
            string str = Path.GetFileNameWithoutExtension(fname);
            if (str.Length > 9) fname = str.Substring(0, 8) + ".." + fInfo.Extension;
        }
        else
        {
            if (fInfo.FullName.Equals(nowDir.Parent.FullName)) fname = "..";
            else if (fname.Length > 13) fname = fname.Substring(0, 12) + "...";
            fname = "[" + fname + "]";
        }
        script.label.text = fname;
    }

    void LoadImageFile(string path, RawImage image, int width, int height, bool isResized = false)
    {
        if (!File.Exists(path)) return;
        byte[] bytes = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(bytes);
        Debug.Log(texture.width + ":" + texture.height + ":" + path);
        Vector2 size = FixImageSize(texture, width, height);
        if (isResized) texture = ResizeTexture(texture, (int)size.x, (int)size.y);
        image.texture = texture;
        image.rectTransform.sizeDelta = size;
    }

    void LoadBitmapFile(string path, RawImage image, int width, int height, bool isResized = false)
    {
        if (!File.Exists(path)) return;
        byte[] oldBytes = File.ReadAllBytes(path);
        byte[] newBytes;
        MemoryStream oldStream = new MemoryStream(oldBytes);
        MemoryStream newStream = new MemoryStream();
        System.Drawing.Image img = null;
        try
        {
            img = System.Drawing.Bitmap.FromStream(oldStream);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        try
        {
            if (img != null) img.Save(newStream, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
        }
        newBytes = newStream.ToArray();

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(newBytes);
        Debug.Log(texture.width + ":" + texture.height + ":" + path);
        Vector2 size = FixImageSize(texture, width, height);
        if (isResized) texture = ResizeTexture(texture, (int)size.x, (int)size.y);
        image.texture = texture;
        image.rectTransform.sizeDelta = size;
    }

    Vector2 FixImageSize(Texture2D source, int width, int height)
    {
        int w, h;
        if (source.width < width && source.height < height)
            return new Vector3(source.width, source.height);
        w = Mathf.FloorToInt(source.width * height / source.height);
        h = height;
        if (w > width)
        {
            w = width;
            h = Mathf.FloorToInt(source.height * width / source.width);
        }
        return new Vector2(w, h);
    }

    Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)width);
        float incY = (1.0f / (float)height);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % width), incY * ((float)Mathf.Floor(px / width)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;
    }

 }
