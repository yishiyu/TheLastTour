using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Controller.Water
{
    // 控制水面顶点波动
    public class WaterWaves : MonoBehaviour
    {
        public MeshFilter meshFilter;

        public Vector3[] waveCenters = new[]
        {
            new Vector3(3.5f, 1, 3.5f),
            new Vector3(9.5f, 1, 9.5f),
        };

        public float waveSpeed = 2f;
        public float waveHeight = 1f;
        public float waveLength = 1f;

        private Mesh _mesh;
        private Vector3[] _meshVerticesBase;
        private Vector3[] _meshVertices;

        private void Awake()
        {
            if (meshFilter != null)
            {
                _mesh = meshFilter.mesh;
                _meshVerticesBase = _mesh.vertices;
                _meshVertices = new Vector3[_meshVerticesBase.Length];
            }
        }

        private void Update()
        {
            for (int i = 0; i < _meshVertices.Length; i++)
            {
                // x,z 保持不变,只改变高度
                _meshVertices[i] = _meshVerticesBase[i];
                _meshVertices[i].y += CalculateWaveHeight(
                    _meshVerticesBase[i].x,
                    _meshVerticesBase[i].z,
                    Time.time * waveSpeed);
            }

            _mesh.vertices = _meshVertices;
            _mesh.RecalculateNormals();
        }

        /// <summary>
        /// 求多个波的叠加高度,xz单位为顶点下标
        /// </summary>
        /// <param name="centers">x,z为波的起始位置,y为波初始高度</param>
        /// <param name="x">需要计算的点x坐标</param>
        /// <param name="z">需要计算的点z坐标</param>
        /// <param name="time">时间系数</param>
        /// <returns></returns>
        private float CalculateWaveHeight(float x, float z, float offset)
        {
            // 波的高度 = sin(距离/波长+时间)*波高*距离衰减
            // 平面波的能量均匀分布在一圈上,圈与距离成正比,波能量与高度成正比,故波高与距离成反比
            // 为了防止除0错误,为距离增加初始值1
            float pointWaveHeight = 0f;
            foreach (var center in waveCenters)
            {
                float distance = Vector2.Distance(new Vector2(x, z), new Vector2(center.x, center.z));
                float distanceGama = center.y / (Mathf.Sqrt(distance) + 1);
                pointWaveHeight += Mathf.Sin(distance / waveLength - offset) * this.waveHeight * distanceGama;
            }

            return pointWaveHeight;
        }

        private void OnDrawGizmos()
        {
            foreach (var center in waveCenters)
            {
                float radius = center.y * waveHeight;
                Vector3 centerPos = new Vector3(
                    center.x * transform.localScale.x + transform.position.x,
                    transform.position.y,
                    center.z * transform.localScale.z + transform.position.z);
                Gizmos.DrawWireSphere(centerPos, radius);
            }
        }
    }
}