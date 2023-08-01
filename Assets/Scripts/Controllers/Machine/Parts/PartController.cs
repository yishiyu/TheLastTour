using System;
using System.Collections.Generic;
using System.Numerics;
using TheLastTour.Controller.Water;
using TheLastTour.Manager;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

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

    public abstract class PartController : MonoBehaviour, ISimulator
    {
        // 可设置属性
        public string partName = "Part";
        public float mass = 10;
        public Vector3 centerOfMass = Vector3.zero;
        public bool isCorePart = false;
        public long PartId = 0;
        public FloatComponent partFloatComponent;

        // 三个方向的阻力系数
        // 水体阻力系数倍数,水密度/空气密度 = 775
        public Vector3 airResistance = new Vector3(0.001f, 0.001f, 0.001f);
        public const float WaterResistanceMultiple = 775;

        public FloatComponent PartFloatComponent
        {
            get
            {
                if (partFloatComponent == null)
                {
                    partFloatComponent = GetComponentInChildren<FloatComponent>();
                }

                return partFloatComponent;
            }
        }

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

        public virtual void FixedUpdate()
        {
            bool isUnderwater = false;

            Rigidbody simulatorRigidbody = GetSimulatorRigidbody();
            if (simulatorRigidbody)
            {
                // 计算浮力
                if (PartFloatComponent)
                {
                    {
                        isUnderwater = PartFloatComponent.GetFloatingForce(
                            simulatorRigidbody.worldCenterOfMass,
                            out var force,
                            out var torque);
                        if (isUnderwater)
                        {
                            // 力矩的效果被单独剥离到后面了,无需指定作用点位置
                            simulatorRigidbody.AddForce(
                                force,
                                ForceMode.Impulse
                            );

                            // 将 torque 转换为世界坐标系下的 torque
                            simulatorRigidbody.AddTorque(
                                torque,
                                ForceMode.Impulse
                            );

                            // Debug.Log(
                            //     "FloatingForce: " + force +
                            //     "\nFloatingTorque" + torque);
                        }
                    }
                }


                // 计算阻力
                // 参考 Unity 刚体本身对空气阻力的处理
                // https://forum.unity.com/threads/how-is-angular-drag-applied-to-a-rigidbody-in-unity-how-is-angular-damping-applied-in-physx.369599/
                // 但 Unity 本身将阻力系数设定为一个常量,产生的阻力与速度成正比,而不是速度的平方
                // 对此进行一些修正(因为飞机等运动是需要高速运动的)
                // 阻力参考资料: https://zh.wikipedia.org/zh-cn/%E9%98%BB%E5%8A%9B%E6%96%B9%E7%A8%8B
                // F = (1/2)p*v^2*C*A = v^2 * airResistance
                // 
                // 1. 需要注意,当v超过一定阈值时,产生的阻力会瞬间阻止该物体运动(现实中不会出现这种情况是因为 deltaTime 是无穷小的)
                // 此时 v^2 * airResistance * deltaTime >= mv
                // 此时 V = m/(deltaTime * airResistance)
                // 
                // 2. 另一种情况,瞬间的冲量使得力反向并变得更大,这是不合理的,而且这会导致游戏崩溃
                // 此时 v^2 * airResistance * deltaTime >= 2mv
                // 此时 V = 2m/( * deltaTime * airResistance)
                // 
                // 为此需要做一个阈值处理:该冲量不能使速度在一帧内变化为原本的20%以下
                // 此时 F = 0.8mv/deltaTime
                Vector3 velocity = simulatorRigidbody.GetPointVelocity(transform.position);
                Vector3 resistance = transform.rotation * airResistance;
                Vector3 resistanceForce = -new Vector3(
                    Mathf.Abs(velocity.x * resistance.x) * velocity.x,
                    Mathf.Abs(velocity.y * resistance.y) * velocity.y,
                    Mathf.Abs(velocity.z * resistance.z) * velocity.z) * Time.deltaTime;


                if (isUnderwater)
                {
                    resistanceForce *= WaterResistanceMultiple;
                }

                // // 阻力不能超过一定的阈值
                float maxResistance = 0.8f * simulatorRigidbody.mass * velocity.magnitude / Time.deltaTime;
                resistanceForce = Vector3.ClampMagnitude(resistanceForce, maxResistance);

                // 假设阻力总是作用于零件中心
                // 该零件也会产生力矩效果
                // 直接粗略的将该力矩效果等同于对于旋转的阻尼
                // (这样会导致当某个轴上只有一个方块时,该轴没有任何角速度阻尼)
                // 但是实际的游戏中谁有会只造一个方块或者一个横条呢?(逃
                // 为了防止这种情况,可以给 Rigidbody 的 Angular Drag 设一个较小的值
                simulatorRigidbody.AddForceAtPosition(
                    resistanceForce,
                    transform.position,
                    ForceMode.Impulse
                );

                // Debug.DrawLine(transform.position, transform.position + resistanceForce, Color.red);
                // Debug.Log(
                //     "velocity: " + velocity + "\n" +
                //     "resistanceForce: " + resistanceForce
                    // "maxResistance: " + maxResistance
                // );
                // Debug.Log("velocity: " + velocity + "\n" +
                //           "resistance: " + resistance + "\n" +
                //           "resistanceForce: " + resistanceForce);
            }
        }

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