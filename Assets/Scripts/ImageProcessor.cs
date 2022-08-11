using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImageProcessor { 

    public static (Color,Color) DominantColors(Sprite img)
    {
        Texture2D t = img.texture;
        Color[] colors = t.GetPixels();
        Dictionary<Color,float> hashtable = new Dictionary<Color, float>();
        foreach(Color c in colors)
        {
            if(c.a > 0.5f)
            {
                if (hashtable.ContainsKey(c))
                {
                    hashtable[c] += CloseEnough(c,Color.white,0.15f) ? 0.5f : 1;
                }
                else
                {
                    hashtable.Add(c, 1);
                }
            }
            
        }
        Color dom = Color.black;
        Color dom2 = Color.white;
        float max = 0;
        foreach(KeyValuePair<Color,float> kv in hashtable)
        {
            if(kv.Value > max)
            {
                dom = kv.Key;
                max = kv.Value;
            }
        }
        hashtable.Remove(dom);
        List<Color> flagged = new List<Color>();
        foreach(KeyValuePair<Color,float> kv in hashtable)
        {
            if (ImageProcessor.CloseEnough(kv.Key, dom, 0.15f))
            {
                flagged.Add(kv.Key);
            }
        }
        foreach(Color c in flagged)
        {
            hashtable.Remove(c);
        }
        max = 0;
        foreach(KeyValuePair<Color,float> kv in hashtable)
        {
            if (kv.Value > max)
            {
                dom2 = kv.Key;
                max = kv.Value;
            }
        }
        return (dom, dom2);
    }


    public static Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {

        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        Sprite NewSprite = Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height), new Vector2(0, 0), PixelsPerUnit);

        return NewSprite;
    }

    public static Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2);           // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))           // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                 // If data = readable -> return texture
        }
        return null;                     // Return null if load failed
    }

    public static bool CloseEnough(Color a, Color b, float percentageAcceptable)
    {
        bool green = a.g < b.g + percentageAcceptable && a.g > b.g - percentageAcceptable;
        bool red = a.r < b.r + percentageAcceptable && a.r > b.r - percentageAcceptable;
        bool blue = a.b < b.b + percentageAcceptable && a.b > b.b - percentageAcceptable;
        return green && red && blue;
    }

    public static Sprite RandomSprite(Sprite[] sprites)
    {
        Sprite sprite = sprites[Random.Range(0, sprites.Length)];
        return sprite;
    }
}
