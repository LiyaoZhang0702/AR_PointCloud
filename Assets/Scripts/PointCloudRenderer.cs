using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEditor;
using GoogleARCore;
using GoogleARCore.Examples.Common;
using GoogleARCore.Examples.ObjectManipulation;
using laszip.net;

public class point
{
    public Vector3 xyz;
    public Color color;
    public float lod;
}

public class PointCloudRenderer : MonoBehaviour, IComparer<point>
{
    public int Compare(point A, point B)
    {
        return A.lod.CompareTo(B.lod);
    }

    public Camera FirstPersonCamera;
    public GameObject ManipulatorPrefab;
    public Text numText;
    public Text FrequencyText;
    public Text DensityText;
    public Text PointSizeText;

    GameObject pointObj;
    bool IsPut = false;
    bool IsTouchEnabled = true;
    List<point> list = new List<point>();
    Mesh mesh;
    Vector3[] Vertices;
    Color[] C;
    int[] I;
    float[] LoD;
    Vector3 Position;
    float deltaTime = 0.0f;
    int frame;
    float En;
    float r;
    Vector3 Center;
    Vector3 Edges;
    string filename = "/Test.laz";
    ScaleManipulator scaler;

    int UpdateFrequency = 5;
    int IdealDensity = 150000;
    float WantedDensity = 0.0f;
    int Times = 6;

    int n = 2;
    int L;

    void ShowUpdateFrequency()
    {
        FrequencyText.text = "Current update frequency is once per " + UpdateFrequency + " frame.";
    }

    public void IncreaseUpdateFrequency()
    {
        UpdateFrequency++;
    }

    public void DecreaseUpdateFrequency()
    {
        if(UpdateFrequency > 1)
        {
            UpdateFrequency--;
        }
        else
        {
            UpdateFrequency = 0;
        }
    }

    void ShowDensity()
    {
        DensityText.text = "Current wanted density at the center of point cloud is: \n" + WantedDensity + " points per square meter";
    }

    public void IncreaseDensity()
    {
        IdealDensity += 10000;
    }

    public void DecreaseDensity()
    {
        if (IdealDensity > 10000)
        {
            IdealDensity -= 10000;
        }
        else
        {
            IdealDensity = 0;
        }

    }

    void ShowPointSize()
    {
        PointSizeText.text = "Change point size (x" + Times + " ):";
    }

    public void IncreasePointSize()
    {
        Times++;
    }

    public void DecreasePointSize()
    {
        if (Times > 1)
        {
            Times--;
        }
        else
        {
            Times = 0;
        }
    }

    public void DisableTouch()
    {
        IsTouchEnabled = false;
    }

    public void EnableTouch()
    {
        IsTouchEnabled = true;
    }


    // Use this for initialization
    void Start()
    {
        ShowDensity();
        ShowUpdateFrequency();
        ShowPointSize();
        list = ReadFile();
        mesh = CreateMesh(list);

        Matrix4x4 ProjectionMatrix = FirstPersonCamera.projectionMatrix;
        float near = FirstPersonCamera.nearClipPlane;
        r = near * (ProjectionMatrix[0, 2] + 1) / ProjectionMatrix[0, 0];

        int frame = 0;
    }

    // Update is called once per frame
    void Update()
    {
        ShowDensity();
        ShowUpdateFrequency();
        ShowPointSize();

        frame++;
        var pos = FirstPersonCamera.transform.position;
        var rot = FirstPersonCamera.transform.rotation;
        if (Screen.currentResolution.width > Screen.currentResolution.height)// If rotate the device, set the position of UI text again
        {
            numText.transform.position = new Vector3(220, Screen.currentResolution.height - 35, 0);
        }
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

        if (IsPut == true && frame % UpdateFrequency == 0) // Update the current point cloud
        {
            frame = 0;
            mesh = UpdateMesh(mesh);
            // Update the mesh
            pointObj.GetComponent<MeshFilter>().mesh = mesh;

            // Update the shaders
            Material mat = new Material(Shader.Find("Custom/PointCloud"));
            mat.SetFloat("_ScreenHeight", Screen.height);
            mat.SetFloat("_tanFOV", Mathf.Tan(0.5f * FirstPersonCamera.fieldOfView * Mathf.Deg2Rad));
            mat.SetFloat("_n", FirstPersonCamera.nearClipPlane);
            mat.SetFloat("_r", r);
            mat.SetFloat("_times", Times);

            pointObj.GetComponent<MeshRenderer>().material = mat;
        }
        else if (IsPut == false)
        {
            float fps = 1.0f / deltaTime;
            numText.text = "Number of points:" + "\n" + "0" + "\n" + "Frame rate:" + "\n" + fps.ToString("0");
        }

        Touch touch;
        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
        
        //if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit) && IsTouchEnabled)
        {
            if ((hit.Trackable is DetectedPlane) && Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position, hit.Pose.rotation * Vector3.up) < 0)
            {
                Debug.Log("Ray hits the back of the detected plane！");
            }
            else
            {
                IsPut = true;
                
                pointObj = new GameObject();
                pointObj.AddComponent<MeshFilter>();
                pointObj.AddComponent<MeshRenderer>();

                pointObj.GetComponent<MeshFilter>().mesh = mesh;
                Material mat = new Material(Shader.Find("Custom/PointCloud"));
                mat.SetFloat("_ScreenHeight", Screen.height);
                mat.SetFloat("_tanFOV", Mathf.Tan(0.5f * FirstPersonCamera.fieldOfView * Mathf.Deg2Rad));
                mat.SetFloat("_n", FirstPersonCamera.nearClipPlane);
                mat.SetFloat("_r", r);
                mat.SetFloat("_times", Times);
                pointObj.GetComponent<MeshRenderer>().material = mat;

                pointObj.transform.position = hit.Pose.position;
                pointObj.transform.rotation = hit.Pose.rotation;
                Position = hit.Pose.position;

                // Add ARCore's object manipulator (enables scaling, rotation, translation)
                var manipulator = Instantiate(ManipulatorPrefab, hit.Pose.position, hit.Pose.rotation);
                scaler = manipulator.GetComponent<ScaleManipulator>();

                //scaler = manipulator.GetComponent<ScaleManipulator>();
                pointObj.transform.parent = manipulator.transform;
                
                var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                manipulator.transform.parent = anchor.transform;
                manipulator.GetComponent<Manipulator>().Select();

            }
        }
    }

    // Read the file and move the point cloud to the center, and scale it down accordingly
    // Edges: length of the edges of the bounding box, use the longest one to scale the point cloud accordingly
    // Gravity: gravity of the point cloud to move the point cloud to the center
    List<point> ReadFile()
    {
        List<point> vecList = new List<point>();

        string path = Application.persistentDataPath + filename;
        var lazReader = new laszip_dll();
        var compressed = true;
        lazReader.laszip_open_reader(path, ref compressed);

        uint num = lazReader.header.number_of_point_records;
        float minX = (float)lazReader.header.min_x;
        float minY = (float)lazReader.header.min_y;
        float minZ = (float)lazReader.header.min_z;
        float maxX = (float)lazReader.header.max_x;
        float maxY = (float)lazReader.header.max_y;
        float maxZ = (float)lazReader.header.max_z;
        
        L = (int)(Mathf.Log(num / 8000) / Mathf.Log(Mathf.Pow(2, n))) + 1;

        var coord = new double[3];

        for (int pointIndex = 0; pointIndex < num; pointIndex++)
        {
            point node = new point();

            lazReader.laszip_read_point();

            lazReader.laszip_get_coordinates(coord);
            Vector3 xyz = new Vector3((float)coord[0], (float)coord[2], (float)coord[1]);

            Color colour = new Color32((byte)(lazReader.point.rgb[0] / 256), (byte)(lazReader.point.rgb[1] / 256), (byte)(lazReader.point.rgb[2] / 256), (byte)(lazReader.point.rgb[3] / 256));

            float U = UnityEngine.Random.Range(0.0f, 1.0f);
            float lod = Mathf.Log((Mathf.Pow(2, (n - 1) * (L + 1)) - 1) * U + 1) / (n - 1) * Mathf.Log(2);

            node.xyz = xyz;
            node.color = colour;
            node.lod = lod;
            vecList.Add(node);
        }
                
        Vector3 min = new Vector3(minX, minY, minZ);
        Vector3 max = new Vector3(maxX, maxY, maxZ);
        Edges = max - min;

        lazReader.laszip_close_reader();

        vecList = vecList.OrderBy(element => element.lod).ToList();
        Vector3 bounds = (min + max) / 2;
        Center = new Vector3(bounds.x, minZ, bounds.y);

        return vecList;
    }

    Mesh CreateMesh(List<point> list)
    {
        int num = list.Count;
        // Move the point cloud to the center and for large point cloud, and scale it down
        List<float> Edge = new List<float>();
        Edge.Add(Edges.x);
        Edge.Add(Edges.y);
        Edge.Add(Edges.z);
        Edge.Sort();

        float scale = Edge[1];

        Mesh m = new Mesh();
        m.Clear(); // Update the mesh per frame 

        m.indexFormat = num > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
        Vector3[] points = new Vector3[num];
        Color[] colors = new Color[num];
        int[] indices = new int[num];
        float[] levels= new float[num];

        for (int i = 0; i < num; i++)
        {
            point node = list[i];
            Vector3 p = node.xyz;
            points[i] = (p - Center) / scale;
            list[i].xyz = points[i];
            indices[i] = i;
            colors[i] = node.color;
            levels[i] = node.lod;
        }
        En = Edge[1] * Edge[2] / (scale * scale);

        Vertices = points;
        C = colors;
        I = indices;
        LoD = levels;
        m.vertices = points;
        m.colors = colors;
        m.SetIndices(indices, MeshTopology.Points, 0);

        list.Clear();
        return m;
    }

    Mesh UpdateMesh(Mesh m)
    {
        int num = Vertices.GetLength(0);
        Vector3 CameraPosition = FirstPersonCamera.transform.position;
        Vector3 d = Position - CameraPosition;
        float distance = Mathf.Sqrt(d.x * d.x + d.y * d.y + d.z * d.z);

        float ScaleRatio = scaler.CurrentScale;
        float E = En * ScaleRatio;
        
        float l2 = ((IdealDensity * E / (Mathf.Log(distance + 1) * num)) * (Mathf.Pow(2, (n - 1) * (L + 1)) - 1) + 1) / (float)Math.Pow(2, n - 1);
        float l = (float)(Math.Log(l2) / Math.Log(2));
        
        int point_num = Mathf.Abs((int)Array.BinarySearch(LoD, l));
        
        if(point_num > num)
        {
            point_num = num - 1;
        }
                
        Vector3[] points = new Vector3[point_num];
        Color[] colors = new Color[point_num];
        int[] indices = new int[point_num];

        Array.Copy(Vertices, points, point_num);
        Array.Copy(C, colors, point_num);
        Array.Copy(I, indices, point_num);
        
        WantedDensity = IdealDensity / Mathf.Log(distance + 1);
                
        // Calculate current frame rate
        float fps = 1.0f / deltaTime;
        numText.text = "Number of points:" + "\n" + point_num.ToString("0") + "\n" + "Frame rate:" + "\n" + fps.ToString("0");
        Debug.Log(point_num);
        
        m.Clear();
        m.vertices = points;
        m.colors = colors;
        m.SetIndices(indices, MeshTopology.Points, 0);
        return m;
    }
}