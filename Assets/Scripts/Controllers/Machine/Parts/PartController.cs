using System;
using System.Collections.Generic;
using TheLastTour.Controller.Water;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLastTour.Controller.Machine
{
    public enum EEditType
    {
        Default,
        MouseOver,
        Selected,
        PreviewEnable,
        PreviewDisable,
    }

    [RequireComponent(typeof(Collider))]
    public abstract class PartController : MonoBehaviour, ISimulator
    {
        // 可设置属性
        public string partName = "Part";
        public float mass = 10;
        public Vector3 centerOfMass = Vector3.zero;
        public bool isCorePart = false;
        public long PartId = 0;

        private Renderer _partRenderer;

        public Renderer PartRenderer
        {
            get
            {
                if (_partRenderer == null)
                {
                    _partRenderer = GetComponentInChildren<Renderer>();
                }

                return _partRenderer;
            }
        }


        private EEditType editType = EEditType.Default;

        public EEditType EditType
        {
            get { return editType; }
            set
            {
                if (editType != value)
                {
                    // 状态转移控制
                    switch (value)
                    {
                        case EEditType.Default:

                            PartRenderer.material.SetInt("_IsMouseOver", 0);
                            PartRenderer.material.SetInt("_IsSelected", 0);
                            break;

                        case EEditType.MouseOver:
                            PartRenderer.material.SetInt("_IsMouseOver", 1);
                            PartRenderer.material.SetInt("_IsSelected", 0);
                            break;

                        case EEditType.Selected:
                            PartRenderer.material.SetInt("_IsMouseOver", 0);
                            PartRenderer.material.SetInt("_IsSelected", 1);
                            break;

                        // 特殊的编辑状态
                        case EEditType.PreviewEnable:
                            if (editType != EEditType.PreviewDisable)
                            {
                                // 首次进入 Preview 状态
                                TurnOnSimulation(false);
                                TurnOnJointCollision(false);
                                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                            }

                            gameObject.SetActive(true);

                            break;
                        case EEditType.PreviewDisable:
                            if (editType != EEditType.PreviewEnable)
                            {
                                // 首次进入 Preview 状态
                                TurnOnSimulation(false);
                                TurnOnJointCollision(false);
                                gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                            }

                            gameObject.SetActive(false);

                            break;
                    }

                    editType = value;
                }
            }
        }


        public virtual float Mass
        {
            get { return mass; }
        }

        public virtual Vector3 CenterOfMass
        {
            get { return centerOfMass; }
        }

        private PropertyValue<float> _massProperty = new PropertyValue<float>(1);

        public List<PartJointController> joints = new List<PartJointController>();
        public PartJointController rootJoint = null;

        public PartJointController ConnectedJoint
        {
            get
            {
                if (rootJoint)
                {
                    return rootJoint.ConnectedJoint;
                }

                return null;
            }
        }

        // 可以设置的属性
        public readonly List<MachineProperty> Properties = new List<MachineProperty>();

        // 组件 Attach 时绕 Z 轴的旋转角度
        public float RotateAngleZ { get; set; } = 0f;

        public int RootJointId { get; private set; } = -1;

        public void SetRootJoint(int id)
        {
            if (id >= 0 && id < joints.Count)
            {
                RootJointId = id;
                rootJoint = joints[id];
            }
        }

        public void IterRootJoint()
        {
            if (rootJoint != null && rootJoint.IsAttached)
            {
                rootJoint.Detach();
            }

            if (joints.Count > 0)
            {
                SetRootJoint((RootJointId + 1) % joints.Count);
                Debug.Log("Root Joint: " + (RootJointId + 1) % joints.Count);
            }
        }

        public void IterRotateAngleZ()
        {
            if (rootJoint != null && rootJoint.IsAttached)
            {
                rootJoint.Detach();
            }

            RotateAngleZ = (RotateAngleZ + 90) % 360;
        }

        public int GetJointId(PartJointController joint)
        {
            for (int i = 0; i < joints.Count; i++)
            {
                if (joints[i] == joint)
                {
                    return i;
                }
            }

            return -1;
        }

        public PartJointController GetJointById(int id)
        {
            if (id >= 0 && id < joints.Count)
            {
                return joints[id];
            }

            return null;
        }

        public virtual void Awake()
        {
            foreach (var joint in GetComponentsInChildren<PartJointController>())
            {
                joints.Add(joint);
            }

            if (rootJoint != null)
            {
                RootJointId = GetJointId(rootJoint);
                if (RootJointId < 0 || RootJointId > joints.Count)
                {
                    rootJoint = null;
                }
            }
            else if (!isCorePart && joints.Count > 0)
            {
                RootJointId = 0;
                rootJoint = joints[0];
            }

            InitProperties();

            if (!_calculatedDensity)
            {
                _calculatedDensity = true;
                // CalculateDensity();
                CutIntoVoxels();
            }
        }

        protected virtual void InitProperties()
        {
            _massProperty.Value = mass;
            _massProperty.OnValueChanged += (newMass) =>
            {
                mass = newMass;
                // UpdateSimulatorMass();
            };

            Properties.Add(new MachineProperty("Mass", _massProperty));
        }

        /// <summary>
        /// 附着到另一个 Joint 上,包括设置双方状态,根据对方位置推算自己位置.
        /// </summary>
        /// <param name="joint">另一个 joint</param>
        /// <param name="ignoreOthers">是否允许对方再被检测到</param>
        /// <param name="addToMachine">是否将自己添加到对方所在 machine</param>
        public void Attach(PartJointController joint, bool ignoreOthers = true, bool addToMachine = false)
        {
            if (joint == null || joint.IsAttached || rootJoint == null)
            {
                return;
            }

            // 通过根组件与其连接
            rootJoint.Attach(joint, ignoreOthers);

            // 计算当前根组件与 rootJoint 的相对位置
            var rootJointTransform = rootJoint.transform;
            var rootModelTransform = rootJointTransform.parent;
            Vector3 rootJointRelativePosition = rootModelTransform.localRotation * rootJointTransform.localPosition +
                                                rootModelTransform.localPosition;
            Quaternion rootJointRelativeRotation = rootModelTransform.localRotation * rootJointTransform.localRotation;

            // 根据 rootJoint 的位置反推当前组件的位置
            transform.rotation = rootJoint.InferredRotation *
                                 Quaternion.Euler(0f, 0f, RotateAngleZ) *
                                 Quaternion.Inverse(rootJointRelativeRotation);
            transform.position = rootJoint.InferredPosition - transform.rotation * rootJointRelativePosition;

            // 将当前组件添加到对应 joint 所在的 machine 中
            if (addToMachine)
            {
                ISimulator simualtor = joint.Owner;
                if (simualtor != null)
                {
                    simualtor.AddPart(this);
                }
            }
        }

        public void Detach()
        {
            if (rootJoint != null && rootJoint.IsAttached)
            {
                rootJoint.Detach();
            }
        }

        public virtual void TurnOnSimulation(bool isOn)
        {
            foreach (var joint in joints)
            {
                joint.TurnOnJointCollision(!isOn);
            }
        }

        public void TurnOnJointCollision(bool isOn)
        {
            foreach (var joint in joints)
            {
                joint.TurnOnJointCollision(isOn);
            }
        }

        public abstract Rigidbody GetSimulatorRigidbody();

        public MachineController GetOwnerMachine()
        {
            // 递归向上级查询,直到查询到 MachineController
            // GetComponentInParent 会查询自身,所以需要跳过自身
            // return GetComponentInParent<ISimulator>().GetOwnerMachine();
            return transform.parent.GetComponent<ISimulator>().GetOwnerMachine();
        }

        public abstract void OnAttached(ISimulator simulator);
        public abstract void AddPart(PartController part);
        public abstract void RemovePart(PartController part);
        public abstract void UpdateSimulatorMass();

        /// <summary>
        /// 用于判断是否为叶子节点, 一些操作需要将某个组件及其附属组件视为一个整体
        /// 例如 MovablePart 需要将该函数重载(始终为true)
        /// </summary>
        /// <returns></returns>
        public virtual bool IsLeafNode()
        {
            // 仅有一个 joint 时为叶子节点
            return joints.Count == 1;
        }

        public JsonPart Serialize()
        {
            JsonPart jsonPart = new JsonPart
            {
                partName = partName,
                rotateAngleZ = RotateAngleZ,
                rootJointId = RootJointId,
                isCorePart = isCorePart,
                partId = PartId,
                attachedJointId = (rootJoint == null || rootJoint.ConnectedJoint == null)
                    ? -1
                    : rootJoint.ConnectedJoint.JointIdInPart,
                attachedPartId = (rootJoint == null || rootJoint.ConnectedJoint == null)
                    ? -1
                    : rootJoint.ConnectedJoint.Owner.PartId,
                partProperties = new List<JsonMachineProperty>()
            };

            foreach (var property in Properties)
            {
                jsonPart.partProperties.Add(property.Serialize());
            }

            return jsonPart;
        }

        public void Deserialize(JsonPart jsonPart)
        {
            partName = jsonPart.partName;
            RotateAngleZ = jsonPart.rotateAngleZ;
            RootJointId = jsonPart.rootJointId;
            isCorePart = jsonPart.isCorePart;
            rootJoint = GetJointById(RootJointId);
            for (int i = 0; i < jsonPart.partProperties.Count; i++)
            {
                Properties[i].Deserialize(jsonPart.partProperties[i]);
            }
        }

        // 水面浮力系统
        private bool _calculatedDensity = false;
        private float desity = 1f;
        public Collider floatingCollider;
        public MeshFilter floatingMesh;
        public float normalizedVoxelSize = 0.5f;
        public Vector3 voxelSize = Vector3.one;
        private Vector3[] _voxels;

        private Collider FloatingCollider
        {
            get
            {
                if (floatingCollider == null)
                {
                    floatingCollider = GetComponent<Collider>();
                }

                return floatingCollider;
            }
        }


        private WaterVolume _waterVolume;

        // mass: 该组件自身的质量
        // Mass: 该组件及其附属组件的总质量
        // 每个 Part 都只需要负责自身的浮力,因此用 mass 和 mesh 来计算浮力

        // private void CalculateDensity()
        // {
        //     Mesh mesh = floatingMesh.mesh;
        //     int[] triangles = mesh.triangles;
        //     Vector3[] vertices = mesh.vertices;
        //
        //     float volume = 0f;
        //     for (int i = 0; i < triangles.Length; i += 3)
        //     {
        //         // 拿着三角形的三个顶点与 Transform 中心 得到三个向量
        //         // 用这三个向量组成的四面体估算体积
        //         // 根据三角形法线方向判断体积正负
        //         // 凹多面体空的部分由于负法线四面体的存在,体积会被抵消
        //         Vector3 v1 = vertices[triangles[i + 0]];
        //         Vector3 v2 = vertices[triangles[i + 1]];
        //         Vector3 v3 = vertices[triangles[i + 2]];
        //
        //         // 以 v1 表示的顶点为交点,三个向量组成四面体
        //         // 其中 t1 t2 从 v1 指向其他顶点, t3 从其他顶点指向 v1
        //         Vector3 t1 = v1 - v2;
        //         Vector3 t2 = v1 - v3;
        //         Vector3 t3 = v1;
        //
        //         // 四面体体积计算公式 (底面积 * 高 / 3) = (底面向量叉乘/2)点乘第三个向量/3
        //         volume = (Vector3.Dot(t1, Vector3.Cross(t2, t3))) / 6f;
        //     }
        //
        //     desity = mass / volume;
        // }

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
                        if (!IsVoxelInCollider(voxelCenter, boundsMagnitude))
                        {
                            continue;
                        }

                        _voxels[i * voxelCountAlongAxis * voxelCountAlongAxis + j * voxelCountAlongAxis + k] =
                            voxelCenter;
                    }
                }
            }

            transform.rotation = rotation;
        }

        private bool IsVoxelInCollider(Vector3 voxel, float length)
        {
            Ray ray = new Ray(voxel, floatingCollider.transform.position - voxel);
            if (!Physics.Raycast(ray, out var hit, length, -1))
            {
                return true;
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
                    Gizmos.DrawSphere(voxel, 0.1f);
                }
            }
        }
    }

    [Serializable]
    public struct JsonPart
    {
        public string partName;
        public float rotateAngleZ;
        public int rootJointId;
        public bool isCorePart;

        // 重建该 Part 时需要提供为 PartManager 的 ID
        // 用于重建不同 Part 的联系
        public long partId;

        // 用于确定附着关系
        public int attachedJointId;
        public long attachedPartId;

        public List<JsonMachineProperty> partProperties;
    }
}