using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheLastTour.Test
{
    public class TestController : MonoBehaviour
    {
        public Rigidbody rigidBody;


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            // 画出转动惯量轴
            Gizmos.DrawLine(
                transform.position,
                transform.position + rigidBody.inertiaTensorRotation * rigidBody.inertiaTensor.normalized);
        }
    }
}