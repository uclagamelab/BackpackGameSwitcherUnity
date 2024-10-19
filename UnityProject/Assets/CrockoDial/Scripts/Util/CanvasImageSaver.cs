using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasImageSaver 
{
    public static void RenderCanvasToPng(Canvas c, string fileName)
    {
        Camera cam = new GameObject().AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        var mainCam = Camera.main;
        mainCam.enabled = false;
       
        RenderTexture temp = RenderTexture.GetTemporary(1920*2, 1080*2, 0);
        cam.targetTexture = temp;
        var oMode = c.renderMode;
        c.renderMode = RenderMode.ScreenSpaceCamera;
        c.worldCamera = cam;
        cam.Render();

        string path = $"{System.IO.Path.GetDirectoryName(Application.dataPath)}/{fileName}.png";
        SaveRTToFile(temp, path);
        c.worldCamera = null;
        c.renderMode = oMode;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(temp);
        mainCam.enabled = true;
   
        GameObject.Destroy(cam.gameObject);
    }

    public static void SaveRTToFile(RenderTexture rt, string filePath)
    {
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        RenderTexture.active = null;

        byte[] bytes;
        bytes = tex.EncodeToPNG();

        System.IO.File.WriteAllBytes(filePath, bytes);
    }
}
