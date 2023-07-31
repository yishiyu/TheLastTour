using System;
using System.Collections;
using System.Collections.Generic;
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

        BoxCollider _boxCollider;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
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
    }
}