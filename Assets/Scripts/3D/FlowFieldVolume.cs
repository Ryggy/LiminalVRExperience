using System;
using System.IO;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
public class FlowFieldVolume : MonoBehaviour
{
    public bool SizeFromFile = true;
    public float SizeX;
    public float SizeY;
    public float SizeZ;

    public string VFFilename;

    [SerializeField, HideInInspector]
    private int vectorSizeX, vectorSizeY, vectorSizeZ;

    [SerializeField, HideInInspector]
    private Vector3 minCorner, maxCorner, boxSize;

    public Vector3 BoxSize
    {
        get { return boxSize; }
    }
    
    private static List<FlowFieldVolume> flowFieldsList = new List<FlowFieldVolume>();
    
    [SerializeField]
    private Vector3[] vectorData;

    [SerializeField, HideInInspector]
    private Vector3[] worldBounds;
    [SerializeField, HideInInspector]
    private Vector3[] localBounds;

    private float globalScale = 0.01f;
    
    public enum AutoAnimationType
    {
        Forward,
        Backward,
        PingPong
    }
    
    public bool Animate = false;
    private float AnimIntensity = 1.0f;
    public AutoAnimationType AnimationType = AutoAnimationType.Forward;
    public AnimationCurve Curve = new AnimationCurve();
    [Range(0.0f, 10.0f)]
    public float Duration = 1.0f;
    
    public float Intensity = 1.0f;
    
    public bool ShowGizmos = true;
    [Range(0.1f, 10.0f)]
    public float GizmoScale = 1.0f;
    public Color VectorFieldColor = new Color(1f, 1f, 0f, 0.5f);
    
    public enum BoundsType
    {
        Border,
        Repeat,
        Closed,
        Mirror
    }
    public BoundsType Bounds = BoundsType.Closed;
    public bool PerAxisBounds = false;
    public BoundsType BoundsX = BoundsType.Closed;
    public BoundsType BoundsY = BoundsType.Closed;
    public BoundsType BoundsZ = BoundsType.Closed;
    [Range(0.0f, 1.0f)]
    public float ClosedBoundRamp = 0.0f;
    private void OnEnable()
    {
#if UNITY_EDITOR
        EditorApplication.update += Update;
#endif
        if (!flowFieldsList.Contains(this))
        {
            flowFieldsList.Add(this);
        }
    }
    private void OnDisable()
    {
#if UNITY_EDITOR
        EditorApplication.update -= Update;
#endif
        if (flowFieldsList.Contains(this))
            flowFieldsList.Remove(this);
    }

    private void Update()
    {
        if (vectorData == null)
            return;

        if (transform.hasChanged)
        {
            ComputeWorldBounds();
            transform.hasChanged = false;
        }
        
        float localTime;

#if UNITY_EDITOR
        localTime = (float)(EditorApplication.timeSinceStartup%10000.0);
#else
			localTime = Time.time;
#endif
        if (Animate)
        {
            switch (AnimationType)
            {
                case AutoAnimationType.Forward:
                {
                    float relativeTimePos = (localTime%Duration) / Duration;
                    AnimIntensity = Curve.Evaluate(relativeTimePos);
                }
                    break;
                case AutoAnimationType.Backward:
                {
                    float relativeTimePos = (localTime%Duration) / Duration;
                    AnimIntensity = Curve.Evaluate(1.0f - relativeTimePos);
                }
                    break;
                case AutoAnimationType.PingPong:
                {
                    float relativeTimePos = (localTime%(Duration*2)) / Duration;
                    if (relativeTimePos>1.0f)
                        relativeTimePos = 2.0f-relativeTimePos;
						
                    AnimIntensity = Curve.Evaluate(relativeTimePos);
                }
                    break;
            }
        }
        else
        {
            AnimIntensity = 1.0f;
        }
    }

    public void ClearFF()
    {
        vectorData = null;
        minCorner = maxCorner = boxSize = Vector3.zero;
        vectorSizeX = vectorSizeY = vectorSizeZ = 0;
        worldBounds = null;
        localBounds = null;
    }
    public void ReadFGA(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("FGA file not found: " + path);
            return;
        }
        
        using (StreamReader reader = new StreamReader(path))
        {
            // Size
            var sizeParts = reader.ReadLine()?.Split(',');
            
            vectorSizeX = (int)float.Parse(sizeParts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            vectorSizeY = (int)float.Parse(sizeParts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            vectorSizeZ = (int)float.Parse(sizeParts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture);
            
            // Min corner
            var minParts = reader.ReadLine()?.Split(',');
            minCorner.x = float.Parse(minParts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;
            minCorner.y = float.Parse(minParts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;
            minCorner.z = float.Parse(minParts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;
                
            // Max corner
            var maxParts = reader.ReadLine()?.Split(',');
            maxCorner.x = float.Parse(maxParts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;
            maxCorner.y = float.Parse(maxParts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;
            maxCorner.z = float.Parse(maxParts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale;

            boxSize = maxCorner - minCorner;

            if (SizeFromFile)
            {
                SizeX = boxSize.x;
                SizeY = boxSize.y;
                SizeZ = boxSize.z;
            }

            vectorData = new Vector3[vectorSizeX * vectorSizeY * vectorSizeZ];

            for (int z = 0; z < vectorSizeZ; z++)
            {
                for (int y = 0; y < vectorSizeY; y++)
                {
                    for (int x = 0; x < vectorSizeX; x++)
                    {
                        var vecParts = reader.ReadLine()?.Split(',');
                        Vector3 dir = new Vector3(
                            float.Parse(vecParts[0].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale,
                            float.Parse(vecParts[1].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale,
                            float.Parse(vecParts[2].Trim(), System.Globalization.CultureInfo.InvariantCulture) * globalScale
                        );
                        vectorData[x + y * vectorSizeX + z * vectorSizeX * vectorSizeY] = dir;
                    }
                }
            }
            
            reader.Close();
            
            if (SizeFromFile)
                ComputeBounds();
            else
                ResizeBox();
        }
    }
    private void ComputeBounds()
    {
        localBounds = new Vector3[8];
        worldBounds = new Vector3[8];

        localBounds[0] = new Vector3(minCorner.x, minCorner.y, minCorner.z);
        localBounds[1] = new Vector3(maxCorner.x, minCorner.y, minCorner.z);
        localBounds[2] = new Vector3(minCorner.x, maxCorner.y, minCorner.z);
        localBounds[3] = new Vector3(maxCorner.x, maxCorner.y, minCorner.z);
        localBounds[4] = new Vector3(minCorner.x, minCorner.y, maxCorner.z);
        localBounds[5] = new Vector3(maxCorner.x, minCorner.y, maxCorner.z);
        localBounds[6] = new Vector3(minCorner.x, maxCorner.y, maxCorner.z);
        localBounds[7] = new Vector3(maxCorner.x, maxCorner.y, maxCorner.z);

        ComputeWorldBounds();
    }
    private void ComputeWorldBounds()
    {
        if (worldBounds == null)
            return;

        for (int i = 0; i<8; i++)
            worldBounds[i] = transform.TransformPoint(localBounds[i]);
    }
    public void ResizeBox()
    {
        boxSize.x = SizeX;
        boxSize.y = SizeY;
        boxSize.z = SizeZ;

        minCorner = -boxSize*0.5f;
        maxCorner = boxSize*0.5f;

        ComputeBounds();
    }
    void DisplayBox(Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(worldBounds[0], worldBounds[1]);
        Gizmos.DrawLine(worldBounds[1], worldBounds[3]);
        Gizmos.DrawLine(worldBounds[3], worldBounds[2]);
        Gizmos.DrawLine(worldBounds[2], worldBounds[0]);

        Gizmos.DrawLine(worldBounds[4], worldBounds[5]);
        Gizmos.DrawLine(worldBounds[5], worldBounds[7]);
        Gizmos.DrawLine(worldBounds[7], worldBounds[6]);
        Gizmos.DrawLine(worldBounds[6], worldBounds[4]);

        Gizmos.DrawLine(worldBounds[0], worldBounds[4]);
        Gizmos.DrawLine(worldBounds[1], worldBounds[5]);
        Gizmos.DrawLine(worldBounds[2], worldBounds[6]);
        Gizmos.DrawLine(worldBounds[3], worldBounds[7]);
			
    }
    private void DisplayFF()
    {
        if (vectorData == null)
            return;

        Vector3 position = Vector3.zero;
        Vector3 worldPosition, worldDirection;
        
        Color gColor = VectorFieldColor;
        Gizmos.color = gColor;

        float baseScale = Mathf.Max(Mathf.Min(Mathf.Min(
            Vector3.Distance(worldBounds[0], worldBounds[1]) / vectorSizeX,
            Vector3.Distance(worldBounds[0], worldBounds[2]) / vectorSizeY),
            Vector3.Distance(worldBounds[0], worldBounds[4]) / vectorSizeZ), 10.0f);

        baseScale *= GizmoScale * 0.01f;

        for (int y = 0; y < vectorSizeY; y++)
        {
            float rY = (y + 0.5f) / vectorSizeY;
            position.y = Mathf.Lerp(minCorner.y, maxCorner.y, rY);
            for (int x = 0; x < vectorSizeX; x++)
            {
                float rX = (x + 0.5f) / vectorSizeX;
                position.x = Mathf.Lerp(minCorner.x, maxCorner.x, rX);
                for (int z = 0; z < vectorSizeZ; z++)
                {
                    float rZ = (z + 0.5f) / vectorSizeZ;
                    position.z = Mathf.Lerp(minCorner.z, maxCorner.z, rZ);

                    Vector3 localDirection = vectorData[x + y * vectorSizeX + z * vectorSizeX * vectorSizeY] * Intensity * AnimIntensity;

                    worldPosition = transform.TransformPoint(position);
                    worldDirection = transform.TransformDirection(localDirection * baseScale);
                    
                    if (localDirection.sqrMagnitude < 0.00001f)
                        continue;

                    Gizmos.color = gColor;
                    
                    Gizmos.DrawRay(worldPosition, worldDirection.normalized * baseScale * 3.0f);
                            
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (vectorData == null)
            return;

        if (!ShowGizmos)
            return;
			
        DisplayBox(Color.gray);
        
        DisplayFF();

    }

    #region Bounds Get Functions
    public void GetRandomVector(out Vector3 position, out Vector3 direction)
    {
        if (vectorData == null)
            position = direction = Vector3.zero;
			
        int x = Random.Range(0, vectorSizeX);
        int y = Random.Range(0, vectorSizeY);
        int z = Random.Range(0, vectorSizeZ);

        position = new Vector3();
        direction = new Vector3();
			
        float rY = (float)(y+0.5f)/(float)vectorSizeY;
        position.y = Mathf.Lerp(minCorner.y, maxCorner.y, rY);
        float rX = (float)(x+0.5f)/(float)vectorSizeX;
        position.x = Mathf.Lerp(minCorner.x, maxCorner.x, rX);
        float rZ = (float)(z+0.5f)/(float)vectorSizeZ;
        position.z = Mathf.Lerp(minCorner.z, maxCorner.z, rZ);
        position = transform.TransformPoint(position);

        direction = vectorData[x+y*vectorSizeX+z*vectorSizeX*vectorSizeY]*Intensity*AnimIntensity;
        direction = transform.TransformDirection(direction);
    }
    public Vector3 GetPointInField(float invcoverage)
    {
        if (worldBounds==null)
            return Vector3.zero;

        float x = Random.Range(minCorner.x + boxSize.x*0.5f*invcoverage, maxCorner.x - boxSize.x*0.5f*invcoverage);
        float y = Random.Range(minCorner.y + boxSize.y*0.5f*invcoverage, maxCorner.y - boxSize.y*0.5f*invcoverage);
        float z = Random.Range(minCorner.z + boxSize.z*0.5f*invcoverage, maxCorner.z - boxSize.z*0.5f*invcoverage);

        return transform.TransformPoint(new Vector3(x, y, z));
    }
    public Vector3 GetRandomCorner()
    {
        if (worldBounds==null)
            return Vector3.zero;

        return worldBounds[Random.Range(0, worldBounds.Length)];
    }
    public Vector3 GetPointOnEdge(float invcoverage)
    {
        if (worldBounds==null)
            return Vector3.zero;

        int edgeID = Random.Range(0, 12);
        float relativePosition = Random.Range(0.5f*invcoverage, 1.0f-0.5f*invcoverage);

        switch (edgeID)
        {
            case 0:
                return Vector3.Lerp(worldBounds[0], worldBounds[1], relativePosition);
            case 1:
                return Vector3.Lerp(worldBounds[1], worldBounds[3], relativePosition);
            case 2:
                return Vector3.Lerp(worldBounds[3], worldBounds[2], relativePosition);
            case 3:
                return Vector3.Lerp(worldBounds[2], worldBounds[0], relativePosition);
            case 4:
                return Vector3.Lerp(worldBounds[4], worldBounds[5], relativePosition);
            case 5:
                return Vector3.Lerp(worldBounds[5], worldBounds[7], relativePosition);
            case 6:
                return Vector3.Lerp(worldBounds[7], worldBounds[6], relativePosition);
            case 7:
                return Vector3.Lerp(worldBounds[6], worldBounds[4], relativePosition);
            case 8:
                return Vector3.Lerp(worldBounds[0], worldBounds[4], relativePosition);
            case 9:
                return Vector3.Lerp(worldBounds[1], worldBounds[5], relativePosition);
            case 10:
                return Vector3.Lerp(worldBounds[2], worldBounds[6], relativePosition);
            case 11:
                return Vector3.Lerp(worldBounds[3], worldBounds[7], relativePosition);
        }
        return Vector3.zero;
    }		
    public Vector3 GetPointOnVolume(float invcoverage)
    {
        int faceID = Random.Range(0, 6);
        return GetPointOnFace(faceID, invcoverage);
    }	
    public Vector3 GetPointOnFace(int faceID, float invcoverage)
    {
        if (worldBounds==null)
            return Vector3.zero;

        float relativePositionX = Random.Range(0.5f*invcoverage, 1.0f-0.5f*invcoverage);
        float relativePositionY = Random.Range(0.5f*invcoverage, 1.0f-0.5f*invcoverage);
			
        switch (faceID)
        {
            case 0:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[0], worldBounds[1], relativePositionX),
                    Vector3.Lerp(worldBounds[2], worldBounds[3], relativePositionX), relativePositionY); 
            case 1:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[4], worldBounds[5], relativePositionX),
                    Vector3.Lerp(worldBounds[6], worldBounds[7], relativePositionX), relativePositionY); 
            case 2:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[0], worldBounds[1], relativePositionX),
                    Vector3.Lerp(worldBounds[4], worldBounds[5], relativePositionX), relativePositionY); 
            case 3:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[2], worldBounds[0], relativePositionX),
                    Vector3.Lerp(worldBounds[6], worldBounds[4], relativePositionX), relativePositionY); 
            case 4:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[3], worldBounds[2], relativePositionX),
                    Vector3.Lerp(worldBounds[7], worldBounds[6], relativePositionX), relativePositionY); 
            case 5:
                return Vector3.Lerp(
                    Vector3.Lerp(worldBounds[1], worldBounds[3], relativePositionX),
                    Vector3.Lerp(worldBounds[5], worldBounds[7], relativePositionX), relativePositionY); 
        }
			
        return Vector3.zero;
    }		
    public Vector3 GetVector(Vector3 worldPosition)
	{
		if (vectorData==null)
			return Vector3.zero;
		
		Vector3 localPosition = transform.InverseTransformPoint(worldPosition);
		Vector3 value = Vector3.zero;
		Vector3 relativeLocalPosition = (localPosition-minCorner);
		relativeLocalPosition.x /= boxSize.x;
		relativeLocalPosition.y /= boxSize.y;
		relativeLocalPosition.z /= boxSize.z;

		float borderIntensity = 1.0f;
        Vector3 clampedBorderIntensity = Vector3.one;
        
        if (ClosedBoundRamp > 0.0f)
        {
            clampedBorderIntensity.x = Mathf.Min(clampedBorderIntensity.x, 0.5f - Mathf.Abs(0.5f - relativeLocalPosition.x));
            clampedBorderIntensity.y = Mathf.Min(clampedBorderIntensity.y, 0.5f - Mathf.Abs(0.5f - relativeLocalPosition.y));
            clampedBorderIntensity.z = Mathf.Min(clampedBorderIntensity.z, 0.5f - Mathf.Abs(0.5f - relativeLocalPosition.z));

            clampedBorderIntensity = 2.0f * clampedBorderIntensity;

            clampedBorderIntensity.x = Mathf.Clamp01(clampedBorderIntensity.x / ClosedBoundRamp);
            clampedBorderIntensity.y = Mathf.Clamp01(clampedBorderIntensity.y / ClosedBoundRamp);
            clampedBorderIntensity.z = Mathf.Clamp01(clampedBorderIntensity.z / ClosedBoundRamp);
        }
        
		int cX, cY, cZ;
        
        if (!PerAxisBounds) 
            BoundsX = BoundsY = BoundsZ = Bounds;
		
        switch (BoundsX)
			{
				case BoundsType.Closed:
					if ((relativeLocalPosition.x < -0.001f) || (relativeLocalPosition.x > 1.001f))
						return value;
					borderIntensity *= clampedBorderIntensity.x;
					break;
				case BoundsType.Repeat:
					relativeLocalPosition.x -= Mathf.Floor(relativeLocalPosition.x);
					break;
				case BoundsType.Border:
					relativeLocalPosition.x = Mathf.Clamp(relativeLocalPosition.x, 0.0f, 0.9999f);
					break;
				case BoundsType.Mirror:
					relativeLocalPosition.x = (relativeLocalPosition.x*.5f - Mathf.Floor(relativeLocalPosition.x*.5f))*2.0f;
					if (relativeLocalPosition.x > 1.0f)
						relativeLocalPosition.x = 2f - relativeLocalPosition.x;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			switch (BoundsY)
			{
				case BoundsType.Closed:
					if ((relativeLocalPosition.y < -0.001f) || (relativeLocalPosition.y > 1.001f))
						return value;
					borderIntensity *= clampedBorderIntensity.y;
					break;
				case BoundsType.Repeat:
					relativeLocalPosition.y -= Mathf.Floor(relativeLocalPosition.y);
					break;
				case BoundsType.Border:
					relativeLocalPosition.y = Mathf.Clamp(relativeLocalPosition.y, 0.0f, 0.9999f);
					break;
				case BoundsType.Mirror:
					relativeLocalPosition.y = (relativeLocalPosition.y*0.5f - Mathf.Floor(relativeLocalPosition.y*0.5f))*2.0f;
					if (relativeLocalPosition.y > 1.0f)
						relativeLocalPosition.y = 2f - relativeLocalPosition.y;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			switch (BoundsZ)
			{
				case BoundsType.Closed:
					if ((relativeLocalPosition.z < -0.001f) || (relativeLocalPosition.z > 1.001f))
						return value;
					borderIntensity *= clampedBorderIntensity.z;
					break;
				case BoundsType.Repeat:
					relativeLocalPosition.z -= Mathf.Floor(relativeLocalPosition.z);
					break;
				case BoundsType.Border:
					relativeLocalPosition.z = Mathf.Clamp(relativeLocalPosition.z, 0.0f, 0.9999f);
					break;
				case BoundsType.Mirror:
					relativeLocalPosition.z = (relativeLocalPosition.z*0.5f - Mathf.Floor(relativeLocalPosition.z*0.5f))*2.0f;
					if (relativeLocalPosition.z > 1.0f)
						relativeLocalPosition.z = 2f - relativeLocalPosition.z;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
        
		cX = Mathf.FloorToInt(relativeLocalPosition.x * vectorSizeX);
		cY = Mathf.FloorToInt(relativeLocalPosition.y * vectorSizeY);
		cZ = Mathf.FloorToInt(relativeLocalPosition.z * vectorSizeZ);
		cX = Mathf.Clamp(cX, 0, vectorSizeX - 1);
		cY = Mathf.Clamp(cY, 0, vectorSizeY - 1);
		cZ = Mathf.Clamp(cZ, 0, vectorSizeZ - 1);
		value = vectorData[cX + cY * vectorSizeX + cZ * vectorSizeX * vectorSizeY] * Intensity * AnimIntensity * borderIntensity * borderIntensity * borderIntensity;
		value = transform.TransformDirection(value);

		return value;
	}
    public static Vector3 GetCombinedVectors(Vector3 worldPosition)
    {
        if (flowFieldsList.Count == 0)
            return Vector3.zero;

        Vector3 combined=Vector3.zero;

        for (int i = 0; i<flowFieldsList.Count; i++)
            combined += flowFieldsList[i].GetVector(worldPosition);

        return combined;
    }
    public static Vector3 GetCombinedVectorsRestricted(Vector3 worldPosition, List<FlowFieldVolume> ffList)
    {
        if (ffList.Count == 0)
            return Vector3.zero;

        Vector3 combined=Vector3.zero;

        for (int i = 0; i<ffList.Count; i++)
        {
            if (ffList[i]!=null)
                combined += ffList[i].GetVector(worldPosition);
        }

        return combined;
    }
    #endregion
}