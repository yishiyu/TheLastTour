using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace TheLastTour.Controller.Water
{
    [RequireComponent(typeof(Collider))]
    public class FloatComponent : MonoBehaviour
    {
        // 水面浮力系统
        public Collider floatingCollider;
        public float normalizedVoxelSize = 0.5f;
        public Vector3 voxelSize = Vector3.one;


        private Vector3[] _voxels;
        private WaterVolume _waterVolume;

        private void Awake()
        {
            CutIntoVoxels();
        }

        private void CutIntoVoxels()
        {
            Quaternion rotation = transform.rotation;
            transform.rotation = Quaternion.identity;

            Bounds bounds = floatingCollider.bounds;
            voxelSize.x = bounds.size.x * normalizedVoxelSize;
            voxelSize.y = bounds.size.y * normalizedVoxelSize;
            voxelSize.z = bounds.size.z * normalizedVoxelSize;

            int voxelCountAlongAxis = Mathf.CeilToInt(1f / normalizedVoxelSize);

            _voxels = new Vector3[voxelCountAlongAxis * voxelCountAlongAxis * voxelCountAlongAxis];
            float boundsMagnitude = bounds.size.magnitude;

            for (int i = 0; i < voxelCountAlongAxis; i++)
            {
                for (int j = 0; j < voxelCountAlongAxis; j++)
                {
                    for (int k = 0; k < voxelCountAlongAxis; k++)
                    {
                        Vector3 voxelCenter = new Vector3(
                            bounds.min.x + voxelSize.x * (i + 0.5f),
                            bounds.min.y + voxelSize.y * (j + 0.5f),
                            bounds.min.z + voxelSize.z * (k + 0.5f)
                        );

                        // 用射线检测的方法判断该点是否在碰撞体内
                        // 首次碰到的点的法向量与射线方向一致,则说明该 voxel 在碰撞体内
                        if (IsPointInCollider(voxelCenter, boundsMagnitude))
                        {
                            continue;
                        }

                        _voxels[i * voxelCountAlongAxis * voxelCountAlongAxis + j * voxelCountAlongAxis + k]
                            = voxelCenter - floatingCollider.transform.position;
                    }
                }
            }

            transform.rotation = rotation;
        }

        private bool IsPointInCollider(Vector3 point, float length)
        {
            // 从物体内部向外碰撞是碰撞不到的
            // 反向射线检测的话,就有无法剔除物体内部的空洞

            Vector3 outDirection = (point - floatingCollider.transform.position).normalized;

            Ray outRay = new Ray(point, outDirection);
            bool isOutHit = Physics.Raycast(outRay, out var outHit, length, floatingCollider.gameObject.layer);


            // 向外的射线没有命中,该物体向外不会碰到凹多面体的凹面
            // 说明在多边形凸面内或者在碰撞体外
            if (!isOutHit)
            {
                Ray inRay = new Ray(point + outDirection * length, -outDirection);
                bool isInHit = Physics.Raycast(inRay, out var inHit, length, floatingCollider.gameObject.layer);

                // 向内碰撞到了,说明在碰撞体内
                if (isInHit)
                {
                    return true;
                }

                // 向内没有碰撞到,说明在碰撞体外
                return false;
            }
            // 向外的射线命中了,这种情况只有可能是碰到了凹多面体的凹面
            // 例如船体的凹陷处,这种情况下,向内的射线也会命中
            else
            {
                // 从第一次碰撞体向内检测,如果在碰到 voxel 之前没有命中,则说明 voxel 在碰撞体内
                // 否则说明 voxel 在碰撞体外(在凹陷处,如船体内部)
                Ray inRay = new Ray(outHit.point, -outDirection);
                bool isInHit = Physics.Raycast(
                    inRay, out var inHit,
                    (outHit.point - point).magnitude,
                    floatingCollider.gameObject.layer);

                // 但是其实这么做也只能解决一些简单的凹多面体
                if (isInHit)
                {
                    return true;
                }
            }

            return false;
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                _waterVolume = other.GetComponent<WaterVolume>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Water"))
            {
                _waterVolume = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (_voxels != null)
            {
                foreach (var voxel in _voxels)
                {
                    Gizmos.DrawSphere(transform.TransformPoint(voxel), 0.1f);
                }
            }
        }


        /// <summary>
        /// 获取浮力及作用点
        /// </summary>
        /// <param name="rigidbodyPosition">浮力作用刚体位置</param>
        /// <param name="force">计算得到的浮力</param>
        /// <param name="torque">浮力产生的力矩</param>
        /// <returns>是否有浮力</returns>
        public bool GetFloatingForce(Vector3 rigidbodyPosition, out Vector3 force, out Vector3 torque)
        {
            if (_waterVolume)
            {
                force = Vector3.zero;
                torque = Vector3.zero;

                foreach (var voxel in _voxels)
                {
                    Vector3 worldVoxel = transform.TransformPoint(voxel);
                    if (_waterVolume.IsPointUnderWater(worldVoxel))
                    {
                        // 力矩 T = r x F
                        var tempForce = -(_waterVolume.density * voxelSize.x * voxelSize.y * voxelSize.z *
                                          Physics.gravity);
                        force += tempForce;
                        torque += Vector3.Cross(worldVoxel - rigidbodyPosition, tempForce);
                    }
                }

                return true;
            }

            force = Vector3.zero;
            torque = Vector3.zero;
            return false;
        }
    }
}