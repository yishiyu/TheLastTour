using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TheLastTour.Controller.Water
{
    [CustomEditor(typeof(WaterVolume))]
    public class WaterVolumeEditor : UnityEditor.Editor
    {
        private const float ColliderHeightBase = 5f;

        private WaterVolume _waterVolume;

        private SerializedProperty density;
        private SerializedProperty rows;
        private SerializedProperty columns;
        private SerializedProperty meshScale;

        [MenuItem("GameObject/Create Water Mesh")]
        public static void CreateWaterMesh()
        {
            // 默认创建 5x5 的水面
            Mesh mesh = WaterMeshGenerator.GenerateMesh(5, 5, 1);

            AssetDatabase.CreateAsset(mesh, "Assets/Art/Model/WaterMesh.asset");
        }

        private void OnEnable()
        {
            _waterVolume = (WaterVolume)target;

            density = serializedObject.FindProperty("density");
            rows = serializedObject.FindProperty("rows");
            columns = serializedObject.FindProperty("columns");
            meshScale = serializedObject.FindProperty("meshScale");

            Undo.undoRedoPerformed += UpdateWaterVolume;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= UpdateWaterVolume;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(density);

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rows);
            EditorGUILayout.PropertyField(columns);
            EditorGUILayout.PropertyField(meshScale);
            if (EditorGUI.EndChangeCheck())
            {
                rows.intValue = Mathf.Max(1, rows.intValue);
                columns.intValue = Mathf.Max(1, columns.intValue);
                meshScale.floatValue = Mathf.Max(0.01f, meshScale.floatValue);

                UpdateWaterVolume();
            }


            serializedObject.ApplyModifiedProperties();
        }


        private void UpdateWaterVolume()
        {
            // 运行时修改无效
            if (Application.isPlaying)
            {
                return;
            }

            MeshFilter meshFilter = _waterVolume.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                return;
            }

            // 如果直接用 mesh 操作的话会报个错,让改用 sharedMesh
            // Instantiating mesh due to calling MeshFilter.mesh during edit mode. This will leak meshes. Please use MeshFilter.sharedMesh instead.
            Mesh mesh = WaterMeshGenerator.GenerateMesh(rows.intValue, columns.intValue, meshScale.floatValue);
            Mesh oldMesh = meshFilter.sharedMesh;
            meshFilter.sharedMesh = mesh;

            EditorUtility.SetDirty(meshFilter);

            // 如果是旧Mesh不是通过 GameObject 菜单创建的默认 Mesh,则删除旧 Mesh
            if (oldMesh != null && !AssetDatabase.Contains(oldMesh))
            {
                DestroyImmediate(oldMesh);
            }

            BoxCollider boxCollider = _waterVolume.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                boxCollider.size = new Vector3(
                    columns.intValue * meshScale.floatValue,
                    ColliderHeightBase,
                    rows.intValue * meshScale.floatValue);

                // 保证坐标中心位于水体中心
                Vector3 center = boxCollider.size / 2f;
                center.y = -center.y;
                boxCollider.center = center;

                EditorUtility.SetDirty(boxCollider);
            }
        }
    }


    /// <summary>
    /// 动态生成水的Mesh
    /// </summary>
    public class WaterMeshGenerator
    {
        public static Mesh GenerateMesh(int rows, int columns, float meshScale)
        {
            if (rows < 1 || columns < 1 || meshScale <= 0)
            {
                Debug.LogError("Invalid parameters");
                return null;
            }

            // 顶点数比行列数多 1
            int vertexRows = rows + 1;
            int vertexColumns = columns + 1;

            // 构建 Mesh 需要的数据
            Vector3[] meshVertices = new Vector3[vertexRows * vertexColumns];
            Vector3[] meshNormals = new Vector3[vertexRows * vertexColumns];
            Vector2[] meshUV = new Vector2[vertexRows * vertexColumns];
            int[] meshTriangles = new int[rows * columns * 6];


            int triangleIndex = 0;
            for (int r = 0; r < vertexRows; r++)
            {
                for (int c = 0; c < vertexColumns; c++)
                {
                    int index = r * vertexColumns + c;

                    // 平铺所有顶点,间隔为 meshScale
                    // 以左下角点为零坐标点
                    meshVertices[index] = new Vector3(c * meshScale, 0, r * meshScale);
                    meshNormals[index] = Vector3.up;
                    meshUV[index] = new Vector2((float)c / columns, (float)r / rows);

                    // 三角形
                    if (r < rows && c < columns)
                    {
                        // 每个四边形格子切分为两个三角形
                        int bottomLeft = r * vertexColumns + c;
                        int bottomRight = bottomLeft + 1;
                        int topLeft = bottomLeft + vertexColumns;
                        int topRight = topLeft + 1;

                        // 三角形顶点顺序
                        meshTriangles[triangleIndex] = bottomLeft;
                        meshTriangles[triangleIndex + 1] = topLeft;
                        meshTriangles[triangleIndex + 2] = bottomRight;

                        meshTriangles[triangleIndex + 3] = bottomRight;
                        meshTriangles[triangleIndex + 4] = topLeft;
                        meshTriangles[triangleIndex + 5] = topRight;

                        triangleIndex += 6;
                    }
                }
            }

            // 生成 Mesh
            Mesh mesh = new Mesh
            {
                vertices = meshVertices,
                normals = meshNormals,
                uv = meshUV,
                triangles = meshTriangles
            };

            return mesh;
        }
    }
}