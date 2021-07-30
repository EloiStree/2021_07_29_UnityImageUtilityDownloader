using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

public class SaveAndLoadImagesUtility 
{

    public static IEnumerator TryToLoadimageFromDataOrURI(string dataOrUri, ImageLoaderCallback downloadInfo)
    {
        dataOrUri = dataOrUri.Trim();
       

        if (Base64ToImage.IsDataBased(dataOrUri)) { 
            Base64ToImage.TryBase64StringToImage(dataOrUri, out bool converted, out Texture2D img);
            downloadInfo.m_downloaded = img;
            if (img != null)
            {
                downloadInfo.SetAsDownloaded(img);
                downloadInfo.NotifyAsDownloadedOrNot();
                yield break;
            }
            else { 
                downloadInfo.SetAsNotDownloaded("Did not convert the image as base64 but is base64");
                downloadInfo.NotifyAsDownloadedOrNot();
                yield break;
            }
        }

        if (File.Exists(dataOrUri))
        {
            LoadTextureFromFile(dataOrUri, out bool found, out Texture2D image);
            if (found)
            {
                downloadInfo.SetAsDownloaded(image);
                downloadInfo.NotifyAsDownloadedOrNot();
                yield break;
            }
            else
            {
                downloadInfo.SetAsNotDownloaded("Is a file but was not converted");
                downloadInfo.NotifyAsDownloadedOrNot();
                yield break;
            }
        }

        yield return TryToLoadimageFromWeb(dataOrUri, downloadInfo);
    
    }
        public static IEnumerator TryToLoadimageFromWeb(string url, ImageLoaderCallback downloadInfo)
    {
        url= url.Trim();
        if (url == null || url.Length <= 0) {
            downloadInfo.m_pathOrUrlUsed = "";
            downloadInfo.SetAsNotDownloaded("No uri given.");
            downloadInfo.NotifyAsDownloadedOrNot();
            yield break ;
        }
        if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {

            downloadInfo.m_pathOrUrlUsed = "";
            downloadInfo.SetAsNotDownloaded("URI not well formed.");
            downloadInfo.NotifyAsDownloadedOrNot();
            yield break;
        }
        downloadInfo.m_pathOrUrlUsed = url;
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                downloadInfo.SetAsNotDownloaded(uwr.error);
                downloadInfo.NotifyAsDownloadedOrNot();
            }
            else
            {
                downloadInfo.SetAsDownloaded(DownloadHandlerTexture.GetContent(uwr));
                downloadInfo.NotifyAsDownloadedOrNot();
            }
         }
        
    }

   

    public static void LoadTextureFromFile(string uriPath, out bool loadedSuccessfully, out Texture2D texture)
    {
        uriPath = uriPath.Trim();
        texture = null;
        loadedSuccessfully = false;
        if (!File.Exists(uriPath))
        {
            return;
        }
        try
        {
            byte[] bytes;
            bytes = System.IO.File.ReadAllBytes(uriPath);
            texture = new Texture2D(1, 1);
            texture.LoadImage(bytes);
            loadedSuccessfully = true;

        }
        catch (Exception) { }
    }


    public static void TryToLoadPNGNearExecutable(string nameNoExtension, out bool hasBeenFound, out Texture2D image)
    {
        GetPathNearExecutableOfAsPNG(nameNoExtension, out string path);
        LoadTextureFromFile(path, out hasBeenFound, out image);
    }

    public static void SaveNearExe(string nameNoExtension, Texture2D texture) {
        GetPathNearExecutableOfAsPNG(nameNoExtension, out string path);
        SaveTextureAsPNG(path, texture);
    }
    public static void SaveTextureAsPNG(string filePath , Texture2D texture)
    {
        string dirPath = Path.GetDirectoryName(filePath);
        if(!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());
    }

    public static void GetPathNearExecutableOfAsPNG(string nameNoExtension, out string path) {
        path = Directory.GetCurrentDirectory() + "\\" + (nameNoExtension) + ".png";
    }
}



public class ImageLoaderCallback
{
    public string m_pathOrUrlUsed;
    public bool m_finishDownloading;
    public Texture2D m_downloaded;
    public string m_error;

    public void SetAsNotDownloaded(string error)
    {

        m_finishDownloading = true;
        m_error = error;
        NotifyAsDownloadedOrNot();
    }

    public bool HadError()
    {
        return m_error != null && m_error.Length > 0;
    }

    public void SetAsDownloaded(Texture2D texture)
    {
        m_finishDownloading = true;
        m_downloaded = texture;
        NotifyAsDownloadedOrNot();
    }
    

    internal void NotifyAsDownloadedOrNot()
    {
        if (m_toDoWhenDownloaded != null)
            m_toDoWhenDownloaded(this);
    }

    public CallBack m_toDoWhenDownloaded;
    public delegate void CallBack(ImageLoaderCallback info);
}

