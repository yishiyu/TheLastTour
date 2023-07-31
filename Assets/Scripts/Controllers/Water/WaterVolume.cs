using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace TheLastTour.Controller.Water
{
    [RequireComponent(typeof(BoxCollider))]
    [RequireComponent(typeof(WaterWaves))]
    public class WaterVolume : MonoBehaviour
    {
        public int density = 1;
        public int rows = 5;
        public int columns = 5;
        public float meshScale = 1f;


        private Mesh _mesh;

        private Mesh Mesh
        {
            get
            {
                if (_mesh == null)
                {
                    _mesh = GetComponent<MeshFilter>().mesh;
                }

                return _mesh;
            }
        }

        private Vector3[] _meshVerticesWorldPosition;
        private Vector3[] _meshVerticesLocalPosition;

        private void Awake()
        {
            UpdateWaterVertices();
        }

        private void Update()
        {
            UpdateWaterVertices();
        }

        private void OnDrawGizmos()
        {
            // 由于水面的顶点会波动,如果在运行时画 Gizmos,会导致 Gizmos 和水面不断重叠
            if (!Application.isPlaying)
            {
                var boxCollider = GetComponent<BoxCollider>();

                Gizmos.matrix = transform.localToWorldMatrix;

                // 半透明青色
                Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.7f);
                // 注意如果没有减去这个 0.01, 画出来的 Gizmos 会和水面叠在一起
                Gizmos.DrawCube(boxCollider.center - Vector3.up * 0.01f, boxCollider.size);
                Gizmos.color = Color.cyan - new Color(0, 0, 0, 0.2f);
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);

                Gizmos.matrix = Matrix4x4.identity;
            }
        }

        private void UpdateWaterVertices()
        {
            _meshVerticesWorldPosition = Mesh.vertices;
            _meshVerticesLocalPosition = new Vector3[_meshVerticesWorldPosition.Length];
            for (int i = 0; i < _meshVerticesWorldPosition.Length; i++)
            {
                _meshVerticesLocalPosition[i] = transform.InverseTransformPoint(_meshVerticesWorldPosition[i]);
            }
        }


        public bool IsPointUnderWater(Vector3 worldPosition)
        {
            return GetWaterLevel(worldPosition) > worldPosition.y;
        }

        public float GetWaterLevel(Vector3 worldPosition)
        {
            Vector3[] triangleVertices = GetSurroundingTriangleVertices(worldPosition);

            if (triangleVertices.Length == 3)
            {
                Vector3 triangleNormal = Vector3.Cross(
                    triangleVertices[1] - triangleVertices[0],
                    triangleVertices[2] - triangleVertices[0]);

                // 保证找到的法线向上
                if (triangleNormal.y < 0)
                {
                    triangleNormal = -triangleNormal;
                }

                // 同一个平面上两个点AB,一个法线N
                // 根据平面方程 Ax + By + Cz + D = 0
                // AN == BN = -D
                // Ax*Nx + Ay*Ny + Az*Nz = Bx*Nx + By*Ny + Bz*Nz = BN
                // Ay = (BN - Bx*Nx - Bz*Nz) / Ny
                // 可以根据已知的法线和顶点计算出指定 WorldPosition 点向下与水面交点的高度
                float waterLevel = (Vector3.Dot(triangleNormal, triangleVertices[0])
                                    - triangleNormal.x * worldPosition.x
                                    - triangleNormal.z * worldPosition.z) / triangleNormal.y;


                return waterLevel;
            }

            // 超出边界返回水平面基准值
            return transform.position.y;
        }

        private Vector3[] GetSurroundingTriangleVertices(Vector3 worldPosition)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPosition);
            int indexX = Mathf.FloorToInt(localPoint.x / meshScale);
            int indexZ = Mathf.FloorToInt(localPoint.z / meshScale);
            if (indexX < 0 || indexX >= columns || indexZ < 0 || indexZ >= rows)
            {
                return new Vector3[] { };
            }

            Vector3[] triangles = new Vector3[3];
            Vector3 distance = worldPosition - _meshVerticesWorldPosition[indexX + indexZ * columns];

            // 更靠近左下角的点 
            if (distance.x < distance.z)
            {
                triangles[0] = _meshVerticesLocalPosition[indexX + indexZ * columns];
                triangles[1] = _meshVerticesLocalPosition[indexX + (indexZ + 1) * columns];
                triangles[2] = _meshVerticesLocalPosition[indexX + 1 + indexZ * columns];
            }
            // 更靠近右上角的点
            else
            {
                triangles[0] = _meshVerticesLocalPosition[indexX + 1 + (indexZ + 1) * columns];
                triangles[1] = _meshVerticesLocalPosition[indexX + 1 + indexZ * columns];
                triangles[2] = _meshVerticesLocalPosition[indexX + (indexZ + 1) * columns];
            }


            return triangles;
        }
    }
}