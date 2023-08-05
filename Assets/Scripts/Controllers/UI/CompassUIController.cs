using System;
using System.Collections;
using System.Collections.Generic;
using TheLastTour.Manager;
using UnityEngine;

namespace TheLastTour.Controller.UI
{
    public class CompassUIController : MonoBehaviour
    {
        public RectTransform compassContainer;
        private Dictionary<Transform, CompassMarker> _compassMarkers = new Dictionary<Transform, CompassMarker>();
        public List<GameObject> markers = new List<GameObject>();

        public float visibilityAngle = 180f;
        public float minScale = 0.5f;
        public float distanceMinScale = 50f;
        private float _widthMultiplier;
        private float _heightOffset;


        private Transform _playerTransform;

        private Transform PlayerTransform
        {
            get
            {
                if (_playerTransform == null)
                {
                    // 将 CorePart 作为玩家
                    // _playerTransform =
                    //     TheLastTourArchitecture.Instance.GetManager<IMachineManager>().GetCorePart().transform;
                    // 将 Camera 作为玩家
                    if (Camera.main)
                    {
                        _playerTransform = Camera.main.transform;
                    }
                }

                return _playerTransform;
            }
        }


        void Awake()
        {
            foreach (var marker in markers)
            {
                RegisterCompassElement(marker.GetComponent<CompassMarker>());
            }
        }


        private void Update()
        {
            if (PlayerTransform)
            {
                // 每个可视角度对应的宽度
                _widthMultiplier = compassContainer.rect.width / visibilityAngle;
                _heightOffset = -compassContainer.rect.height / 2;

                foreach (var marker in _compassMarkers)
                {
                    // 目标角度
                    float distanceRatio = 0;
                    float angle = 0;

                    if (marker.Value.isDirection)
                    {
                        angle = Vector3.SignedAngle(PlayerTransform.forward,
                            marker.Key.transform.localPosition.normalized, Vector3.up);
                    }
                    else
                    {
                        Vector3 directionVector = marker.Key.transform.position - PlayerTransform.position;
                        Vector3 targetDir = directionVector.normalized;
                        targetDir = Vector3.ProjectOnPlane(targetDir, Vector3.up);

                        Vector3 playerForward = Vector3.ProjectOnPlane(PlayerTransform.forward, Vector3.up);
                        angle = Vector3.SignedAngle(playerForward, targetDir, Vector3.up);


                        // 到达最远距离后设置为最小缩放
                        distanceRatio = Mathf.Clamp01(directionVector.magnitude / distanceMinScale);
                    }


                    // 根据是否在可视角度内设置透明度
                    if (angle > -visibilityAngle / 2 && angle < visibilityAngle / 2)
                    {
                        marker.Value.canvasGroup.alpha = 1;
                        marker.Value.canvasGroup.transform.localPosition =
                            new Vector2(_widthMultiplier * angle, _heightOffset);
                        marker.Value.canvasGroup.transform.localScale =
                            Vector3.one * Mathf.Lerp(1, minScale, distanceRatio);
                    }
                    else
                    {
                        marker.Value.canvasGroup.alpha = 0;
                    }
                }
            }
        }


        public void RegisterCompassElement(CompassMarker marker)
        {
            marker.transform.SetParent(compassContainer);
            _compassMarkers.Add(marker.targetTransform, marker);
        }

        public void UnregisterCompassElement(Transform element)
        {
            if (_compassMarkers.TryGetValue(element, out CompassMarker marker) && marker.canvasGroup != null)
            {
                Destroy(marker.canvasGroup.gameObject);
            }

            _compassMarkers.Remove(element);
        }
    }
}