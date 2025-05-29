using System.Collections;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class FGAFileLoader : IFieldLoader
{
    private readonly MonoBehaviour _context;
    private readonly FlowField2DVolume _volume;

    public FGAFileLoader(MonoBehaviour context, FlowField2DVolume volume)
    {
        _context = context;
        _volume = volume;
    }
    
    public void LoadFromFile(string filename)
    {
        _context.StartCoroutine(LoadFGAAsync(filename));
    }
    
    private IEnumerator LoadFGAAsync(string filename)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filename);
        var www = UnityWebRequest.Get(path);
        yield return www.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
        if (www.result != UnityWebRequest.Result.Success)
#else
        if (www.isNetworkError || www.isHttpError)
#endif
        {
            Debug.LogError($"Failed to load FGA file: {path}\n{www.error}");
            yield break;
        }

        string[] lines = www.downloadHandler.text.Split('\n');
        if (lines.Length < 1)
        {
            Debug.LogError("FGA file is empty or malformed.");
            yield break;
        }

        // Parse field size
        var sizeParts = lines[0].Split(',');
        if (sizeParts.Length < 3)
        {
            Debug.LogError("FGA size line is malformed.");
            yield break;
        }

        int cols = (int)float.Parse(sizeParts[0], CultureInfo.InvariantCulture);
        int rows = (int)float.Parse(sizeParts[1], CultureInfo.InvariantCulture);
        int zSize = (int)float.Parse(sizeParts[2], CultureInfo.InvariantCulture);

        if (zSize != 1)
            Debug.LogWarning("3D FGA file loaded in 2D context. Only the first Z slice will be used.");

        _volume.cols = cols;
        _volume.rows = rows;
        
        int expectedCount = cols * rows;
        _volume.vectorData = new Vector2[expectedCount];
        var original = new Vector2[expectedCount];
        var working = new Vector2[expectedCount];
        
        int lineIndex = 3;
        
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (lineIndex >= lines.Length) break;

                var parts = lines[lineIndex++].Split(',');
                if (parts.Length < 3) continue;

                if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float vx) ||
                    !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float vy) ||
                    !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float vz))
                {
                    Debug.LogWarning($"Malformed vector at line {lineIndex}: {lines[lineIndex - 1]}");
                    continue;
                }

                int index = x + y * cols;
                Vector2 vec = new Vector2(vx, vy) * _volume.importVectorScale;
                _volume.vectorData[index] = vec;
                original[index] = vec;
                working[index] = vec;
            }
        }

        _volume.SetInternalBuffers(original, working);
        Debug.Log($"FGA loaded: {filename} ({cols}x{rows})");
    }
}
