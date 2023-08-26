using System.Collections.Generic;
using Unity.FPS.AI;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Unity.FPS.EditorExt
{
    public class MiniProfiler : EditorWindow
    {
        class BoundsAndCount
        {
            public Bounds Bounds;
            public int Count;
        }

        class CellData
        {
            public Bounds Bounds;
            public int Count;
            public float Ratio;
            public Color Color;
        }

        Vector2 scrollPos;
        bool mustRepaint = false;
        bool mustLaunchHeatmapNextFrame = false;
        bool heatmapIsCalculating = false;
        float cellTransparency = 0.9f;
        float cellThreshold = 0f;
        string levelAnalysisString = "";
        List<string> suggestionStrings = new List<string>();

        static List<CellData> s_CellDatas = new List<CellData>();

        const float cellSize = 10;
        const string newLine = "\n";

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Tools/MiniProfiler")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(MiniProfiler));
        }

        void OnEnable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#elif UNITY_2018_1_OR_NEWER
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false);

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Performance Tips");
            DisplayTips();

            GUILayout.Space(20);
            EditorGUILayout.LabelField("Level Analysis");
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You must exit Play mode for this feature to be available",
                    MessageType.Warning);
            }
            else
            {
                if (GUILayout.Button("Analyze"))
                {
                    AnalyzeLevel();
                }

                if (levelAnalysisString != null && levelAnalysisString != "")
                {
                    EditorGUILayout.HelpBox(levelAnalysisString, MessageType.None);
                }

                if (suggestionStrings.Count > 0)
                {
                    EditorGUILayout.LabelField("Suggestions");
                    foreach (var s in suggestionStrings)
                    {
                        EditorGUILayout.HelpBox(s, MessageType.Warning);
                    }
                }

                if (GUILayout.Button("Clear Analysis"))
                {
                    ClearAnalysis();
                    mustRepaint = true;
                }
            }


            GUILayout.Space(20);
            EditorGUILayout.LabelField("Polygon count Heatmap");
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You must exit Play mode for this feature to be available",
                    MessageType.Warning);
            }
            else
            {
                if (mustLaunchHeatmapNextFrame)
                {
                    DoPolycountMap();
                    cellTransparency = 0.9f;
                    cellThreshold = 0f;
                    mustLaunchHeatmapNextFrame = false;
                    mustRepaint = true;
                }

                if (GUILayout.Button("Build Heatmap"))
                {
                    mustLaunchHeatmapNextFrame = true;
                    heatmapIsCalculating = true;
                }

                if (s_CellDatas.Count > 0)
                {
                    float prevAlpha = cellTransparency;
                    cellTransparency = EditorGUILayout.Slider("Cell Transparency", cellTransparency, 0f, 1f);
                    if (cellTransparency != prevAlpha)
                    {
                        mustRepaint = true;
                    }

                    float prevTreshold = cellThreshold;
                    cellThreshold = EditorGUILayout.Slider("Cell Display Threshold", cellThreshold, 0f, 1f);
                    if (cellThreshold != prevTreshold)
                    {
                        mustRepaint = true;
                    }
                }

                if (GUILayout.Button("Clear Heatmap"))
                {
                    mustRepaint = true;
                    s_CellDatas.Clear();
                }
            }

            EditorGUILayout.EndScrollView();

            if (mustRepaint)
            {
                EditorWindow.GetWindow<SceneView>().Repaint();
                mustRepaint = false;
            }

            if (heatmapIsCalculating)
                EditorUtility.DisplayProgressBar("Polygon Count Heatmap", "Calculations in progress", 0.99f);
        }

        void OnSceneGUI(SceneView sceneView)
        {
            // Draw heatmap
            foreach (CellData c in s_CellDatas)
            {
                if (c.Ratio >= cellThreshold && c.Count > 0)
                {
                    Color col = c.Color;
                    col.a = 1f - cellTransparency;
                    Handles.color = col;
                    Handles.CubeHandleCap(0, c.Bounds.center, Quaternion.identity, c.Bounds.extents.x * 2f,
                        EventType.Repaint);
                }
            }
        }

        void ClearAnalysis()
        {
            levelAnalysisString = "";
            suggestionStrings.Clear();
        }

        void DisplayTips()
        {
            EditorGUILayout.HelpBox(
                "All of your meshes that will never move (floor/wall meshes, for examples) should be placed as children of the \"Level\" GameObject in the scene. This is because the \"Mesh Combiner\" script on that object will take care of combining all meshes under it on game start, and this reduces the cost of rendering them. It is more efficient to render one big mesh than lots of small meshes, even when the number of polygons is the same.",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "Every light added to the level will have a performance cost. If you do add more lights to the level, consider making them not cast any shadows to reduce the performance impact. However, be aware that in WebGL there is a limit of 4 lights to be drawn on screen at the same time",
                MessageType.None);
            EditorGUILayout.HelpBox("Transparent objects are more expensive for performance than opaque objects",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "Animated 3D models (known as \"Skinned Meshes\") are more expensive for performance than regular meshes",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "Having a lot of enemies in the level could impact performance, due to their AI logic",
                MessageType.None);
            EditorGUILayout.HelpBox("Adding rigidbodies (physics objects) to the level could impact performance",
                MessageType.None);
            EditorGUILayout.HelpBox(
                "Open the Profiler window from the top menu bar (Window > Analysis > Profiler) to see in-depth information about your game's performance while you are playing",
                MessageType.None);
        }

        void AnalyzeLevel()
        {
            ClearAnalysis();
            EditorStyles.textArea.wordWrap = true;
            MeshCombiner mainMeshCombiner = GameObject.FindObjectOfType<MeshCombiner>();

            // Analyze
            MeshFilter[] meshFilters = GameObject.FindObjectsOfType<MeshFilter>();
            SkinnedMeshRenderer[] skinnedMeshes = GameObject.FindObjectsOfType<SkinnedMeshRenderer>();
            int skinnedMeshesCount = skinnedMeshes.Length;
            int meshCount = meshFilters.Length;
            int nonCombinedMeshCount = 0;
            int polyCount = 0;

            foreach (MeshFilter mf in meshFilters)
            {
                if (!mf.sharedMesh)
                    continue;

                polyCount += mf.sharedMesh.triangles.Length / 3;

                bool willBeCombined = false;
                if (mainMeshCombiner)
                {
                    foreach (GameObject combineParent in mainMeshCombiner.CombineParents)
                    {
                        if (mf.transform.IsChildOf(combineParent.transform))
                        {
                            willBeCombined = true;
                        }
                    }
                }

                if (!willBeCombined)
                {
                    if (!(mf.GetComponentInParent<PlayerCharacterController>() ||
                          mf.GetComponentInParent<NpcController>() ||
                          mf.GetComponentInParent<Pickup>() ||
                          mf.GetComponentInParent<Objective>()))
                    {
                        nonCombinedMeshCount++;
                    }
                }
            }

            foreach (SkinnedMeshRenderer sm in skinnedMeshes)
            {
                polyCount += sm.sharedMesh.triangles.Length / 3;
            }

            int rigidbodiesCount = 0;
            foreach (var r in GameObject.FindObjectsOfType<Rigidbody>())
            {
                if (!r.isKinematic)
                {
                    rigidbodiesCount++;
                }
            }

            int lightsCount = GameObject.FindObjectsOfType<Light>().Length;
            int enemyCount = GameObject.FindObjectsOfType<NpcController>().Length;

            // Level analysis 
            levelAnalysisString += "- Meshes count: " + meshCount;
            levelAnalysisString += newLine;
            levelAnalysisString += "- Animated models (SkinnedMeshes) count: " + skinnedMeshesCount;
            levelAnalysisString += newLine;
            levelAnalysisString += "- Polygon count: " + polyCount;
            levelAnalysisString += newLine;
            levelAnalysisString += "- Physics objects (rigidbodies) count: " + rigidbodiesCount;
            levelAnalysisString += newLine;
            levelAnalysisString += "- Lights count: " + lightsCount;
            levelAnalysisString += newLine;
            levelAnalysisString += "- Enemy count: " + enemyCount;

            // Suggestions
            if (nonCombinedMeshCount > 50)
            {
                suggestionStrings.Add(nonCombinedMeshCount +
                                        " meshes in the scene are not setup to be combined on game start. Make sure that all the meshes " +
                                        "that will never move, change, or be removed during play are under the \"Level\" gameObject in the scene, so they can be combined for greater performance. \n \n" +
                                        "Note that it is always normal to have a few meshes that will not be combined, such as pickups, player meshes, enemy meshes, etc....");
            }
        }

        void DoPolycountMap()
        {
            s_CellDatas.Clear();
            List<BoundsAndCount> meshBoundsAndCount = new List<BoundsAndCount>();
            Bounds levelBounds = new Bounds();
            Renderer[] allRenderers = GameObject.FindObjectsOfType<Renderer>();

            // Get level bounds and list of bounds & polycount
            for (int i = 0; i < allRenderers.Length; i++)
            {
                Renderer r = allRenderers[i];
                if (r.gameObject.GetComponent<IgnoreHeatMap>())
                    continue;

                levelBounds.Encapsulate(r.bounds);

                MeshRenderer mr = (r as MeshRenderer);
                if (mr)
                {
                    MeshFilter mf = r.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh != null)
                    {
                        BoundsAndCount b = new BoundsAndCount();
                        b.Bounds = r.bounds;
                        b.Count = mf.sharedMesh.triangles.Length / 3;

                        meshBoundsAndCount.Add(b);
                    }
                }
                else
                {
                    SkinnedMeshRenderer smr = (r as SkinnedMeshRenderer);
                    if (smr)
                    {
                        if (smr.sharedMesh != null)
                        {
                            BoundsAndCount b = new BoundsAndCount();
                            b.Bounds = r.bounds;
                            b.Count = smr.sharedMesh.triangles.Length / 3;

                            meshBoundsAndCount.Add(b);
                        }
                    }
                }
            }

            Vector3 boundsBottomCorner = levelBounds.center - levelBounds.extents;
            Vector3Int gridResolution = new Vector3Int(Mathf.CeilToInt((levelBounds.extents.x * 2f) / cellSize),
                Mathf.CeilToInt((levelBounds.extents.y * 2f) / cellSize),
                Mathf.CeilToInt((levelBounds.extents.z * 2f) / cellSize));

            int highestCount = 0;
            for (int x = 0; x < gridResolution.x; x++)
            {
                for (int y = 0; y < gridResolution.y; y++)
                {
                    for (int z = 0; z < gridResolution.z; z++)
                    {
                        CellData cellData = new CellData();

                        Vector3 cellCenter = boundsBottomCorner + (new Vector3(x, y, z) * cellSize) +
                                             (Vector3.one * cellSize * 0.5f);
                        cellData.Bounds = new Bounds(cellCenter, Vector3.one * cellSize);
                        for (int i = 0; i < meshBoundsAndCount.Count; i++)
                        {
                            if (cellData.Bounds.Intersects(meshBoundsAndCount[i].Bounds))
                            {
                                cellData.Count += meshBoundsAndCount[i].Count;
                            }
                        }

                        if (cellData.Count > highestCount)
                        {
                            highestCount = cellData.Count;
                        }

                        s_CellDatas.Add(cellData);
                    }
                }
            }

            for (int i = 0; i < s_CellDatas.Count; i++)
            {
                s_CellDatas[i].Ratio = (float)s_CellDatas[i].Count / (float)highestCount;
                Color col = Color.Lerp(Color.green, Color.red, s_CellDatas[i].Ratio);
                s_CellDatas[i].Color = col;
            }

            heatmapIsCalculating = false;
            EditorUtility.ClearProgressBar();
        }
    }
}