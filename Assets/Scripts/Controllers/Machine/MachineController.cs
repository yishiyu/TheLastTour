using System;
using System.Collections.Generic;
using System.Linq;
using TheLastTour.Event;
using TheLastTour.Manager;
using Unity.Mathematics;
using UnityEngine;

namespace TheLastTour.Controller.Machine
{
    public interface ISimulator
    {
        /// <summary>
        /// 获取控制该物体的 Rigidbody
        /// 对于 MovablePart,返回自身的 Rigidbody
        /// 对于 FixedPart,返回父级的 Rigidbody
        /// 对于 Machine,返回自身的 Rigidbody
        /// </summary>
        /// <returns></returns>
        Rigidbody GetSimulatorRigidbody();

        /// <summary>
        /// 获取控制该物体的 MachineController
        /// </summary>
        /// <returns></returns>
        MachineController GetOwnerMachine();

        /// <summary>
        /// 该 Simulator 连接到了新的主物体上,更新自身的信息
        /// 包括一些约束的对象切换,质量更新等
        /// </summary>
        /// <param name="simulator"></param>
        void OnAttached(ISimulator simulator);


        /// <summary>
        /// 该 Simulator 从主物体上分离,更新自身的信息
        /// 不一定是自己被分离,也可能是上级被分离
        /// </summary>
        void OnDetached();

        /// <summary>
        /// 将一个新的 Part 添加到自身,包括挂载 Transform 和更新质量
        /// 对于 Machine,将 Part 添加到 machineParts 列表中,将 Part 挂载到自身,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,将 Part 挂载到自身并添加到 attachedParts 列表中,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        /// <param name="part"></param>
        public void AddPart(PartController part);

        /// <summary>
        /// 将一个已有的 Part 从自身移除,包括解除挂载 Transform 和更新质量
        /// 该函数需要在 Part 销毁前调用,同时该函数不会销毁 Part 所在 GameObject
        /// 
        /// 对于 Machine, 尝试将所有与 Part 连接的物体分离出去(成为独立的Machine),然后销毁自身
        /// (无论参数是 Fixed 的还是 Movable 的都是合理的)
        /// (分离出去时,对于被分离的 Fixed Part 没有影响,对于被分离的 Movable Part,更新其约束所连接的 Rigidbody 为新的 Machine 的 Rigidbody
        /// 对于 Fixed Part,将事件转发到上层 Simulator,保持参数不变(通常是自己)
        /// 对于 Movable Part,如果 Part 是自身,则向上转发,保持参数不变; 否则先断开自身与该 Part 与父级的连接,然后尝试将所有与 Part 连接的物体分离出去(成为独立的Machine)
        /// </summary>
        /// <param name="part">被移除的对象,只有是该 Simulator 自身或直接子节点才有效(通常是自身调用并将自身作为参数)</param>
        /// <param name="destroyPart">是否销毁该Part</param>
        public void RemovePart(PartController part, bool destroyPart = true);

        /// <summary>
        /// 更新 Simulator 的质量
        /// 对于 Machine,更新自身质量
        /// 对于 Fixed Part,将事件转发到上层 Simulator
        /// 对于 Movable Part,更新自身质量,同时触发所在上层 Simulator 更新质量
        /// </summary>
        public void UpdateSimulatorMass();

        /// <summary>
        /// 从 Joint 处断开连接
        /// </summary>
        /// <param name="joint"></param>
        /// <returns></returns>
        public ISimulator DetachJoint(PartJointController joint);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isOn"></param>
        public void TurnOnSimulation(bool isOn);

        public bool DrawDebugShapes { get; }
    }


    [RequireComponent(typeof(Rigidbody))]
    public class MachineController : MonoBehaviour, ISimulator
    {
        public List<PartController> machineParts = new List<PartController>();
        
        private IPartManager _partManager;

        private Rigidbody _rigidbody;

        public Rigidbody MachineRigidBody
        {
            get
            {
                if (_rigidbody == null)
                {
                    _rigidbody = GetComponent<Rigidbody>();
                }

                return _rigidbody;
            }
        }

        public bool isRestoreFromArchive = false;

        // 最高速度
        // 加这个的原因是通过 impulse 来做阻力,在速度过大的时候会不稳定
        public float maxSpeed = 100f;
        public float maxAngularSpeed = 5f;

        private IGameStateManager _gameStateManager;

        private void Start()
        {
            _gameStateManager = TheLastTourArchitecture.Instance.GetManager<IGameStateManager>();
            _partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();

            if (!isRestoreFromArchive)
            {
                machineParts.AddRange(GetComponentsInChildren<PartController>());
                UpdateSimulatorMass();
            }
        }


        public void OnAttached(ISimulator simulator)
        {
            Debug.LogError("MachineController.OnAttached() is not implemented");
        }

        public void OnDetached()
        {
            throw new NotImplementedException();
        }

        public void AddPart(PartController part)
        {
            machineParts.Add(part);
            part.transform.SetParent(transform);
            part.OnAttached(this);
            UpdateSimulatorMass();
        }

        public void RemovePart(PartController part, bool destroyPart)
        {
            if (!machineParts.Contains(part) || part.isCorePart)
            {
                return;
            }

            // 先切断该 Part 与自身起始方块的连接
            part.Detach();
            machineParts.Remove(part);

            // 拆分该 Part 的所有连接,分别形成多个 Machine
            foreach (var joint in part.joints)
            {
                DetachJoint(joint);
            }


            // 除了该被删除的传入 Part,该 Machine 已无任何 Part,销毁该 Machine
            // TheLastTourArchitecture.Instance.GetManager<IMachineManager>().DestroyMachine(this);
            // 因为已经将该 Part 与自身起始方块断开了,所以不需要销毁自身,销毁该方块即可
            if (destroyPart)
            {
                _partManager.DestroyPart(part);
            }
            else
            {
                var machine = TheLastTourArchitecture.Instance.GetManager<IMachineManager>().CreateEmptyMachine();
                machine.AddPart(part);
            }

            if (machineParts.Count == 0)
            {
                TheLastTourArchitecture.Instance.GetManager<IMachineManager>().DestroyMachine(this);
            }
        }

        public ISimulator DetachJoint(PartJointController joint)
        {
            if (joint != null && joint.IsAttached)
            {
                if (joint == joint.Owner.rootJoint)
                {
                    joint = joint.ConnectedJoint;
                }
                
                // 递归搜索所有与该 Joint 相连的 Part
                List<PartController> parts = joint.GetConnectedPartsRecursively();
                MachineController machine =
                    TheLastTourArchitecture.Instance.GetManager<IMachineManager>().CreateEmptyMachine();

                foreach (var part in parts)
                {
                    machineParts.Remove(part);
                    part.Detach();
                    machine.machineParts.Add(part);

                    part.transform.SetParent(machine.transform);
                    part.OnAttached(machine);
                }

                joint.Detach();

                UpdateSimulatorMass();
                machine.UpdateSimulatorMass();
                return machine;
            }

            return null;
        }

        public void TurnOnSimulation(bool isOn)
        {
            if (MachineRigidBody)
            {
                if (isOn)
                {
                    MachineRigidBody.useGravity = true;
                    MachineRigidBody.isKinematic = false;
                    foreach (var part in machineParts)
                    {
                        part.TurnOnSimulation(true);
                    }

                    MachineRigidBody.maxLinearVelocity = maxSpeed;
                    MachineRigidBody.maxAngularVelocity = maxAngularSpeed;
                }
                else
                {
                    MachineRigidBody.useGravity = false;
                    MachineRigidBody.isKinematic = true;
                    foreach (var part in machineParts)
                    {
                        part.TurnOnSimulation(false);
                    }
                }
            }
        }

        public bool DrawDebugShapes
        {
            get { return _gameStateManager != null && _gameStateManager.DebugMode; }
        }

        private void FixedUpdate()
        {
            // // 获取合力在速度方向上的投影
            // Vector3 force = MachineRigidBody.GetAccumulatedForce();
            // float speedDirectionForce = Vector3.Dot(force, MachineRigidBody.velocity.normalized);
            //
            // // 根据最大速度计算加速度衰减,并产生反方向的力抵消加速效果
            // float decayRate = MachineRigidBody.velocity.magnitude / maxSpeed;
            // if (speedDirectionForce > 0)
            // {
            //     speedDirectionForce *= decayRate;
            // }
            //
            // MachineRigidBody.AddForce(-speedDirectionForce * MachineRigidBody.velocity.normalized);
            //
            //
            // Debug.Log("speed:" + MachineRigidBody.velocity.magnitude +
            //           "angular speed:" + MachineRigidBody.angularVelocity.magnitude);

            // 将阻力分散到每个 Part 上计算

            // 空气产生的阻力
            // float speed = MachineRigidBody.velocity.magnitude;
            // float resistanceForce = speed * speed * airResistanceCoefficient;
            // Vector3 resistanceForceDirection = -MachineRigidBody.velocity.normalized;
            //
            // Vector3 resistanceForcePosition = transform.position + (transform.rotation * MachineRigidBody.centerOfMass);
            //
            // MachineRigidBody.AddForceAtPosition(
            //     resistanceForce * resistanceForceDirection,
            //     resistanceForcePosition,
            //     ForceMode.Impulse
            // );
            // MachineRigidBody.AddRelativeForce(
            //     -MachineRigidBody.velocity.normalized * resistanceForce,
            //     ForceMode.Impulse);

            // Debug.DrawLine(
            //     resistanceForcePosition,
            //     resistanceForcePosition +
            //     resistanceForce * resistanceForceDirection,
            //     Color.yellow);

            // 空气产生的阻力力矩
            // 使用力矩模拟空气阻力可能在旋转速度过快时震,改为直接控制旋转速度
            // Vector3 angularVelocity = MachineRigidBody.angularVelocity * airResistanceCoefficient / 15;
            // Vector3 angularVelocity = MachineRigidBody.angularVelocity;
            // Vector3 resistanceTorque = -angularVelocity * (angularVelocity.magnitude * airResistanceCoefficient * 1000);
            //
            // MachineRigidBody.AddRelativeTorque(
            //     resistanceTorque,
            //     ForceMode.Impulse
            // );
            //
            // Debug.DrawLine(
            //     resistanceForcePosition,
            //     resistanceForcePosition +
            //     resistanceTorque,
            //     Color.green);
            //
            // Debug.Log("resistanceTorque: " + resistanceTorque);

            // 修正转动惯量方向
            // 经过测试, inertiaTensorRotation 是局部坐标系,所以不需要修正
            // MachineRigidBody.inertiaTensorRotation = transform.rotation;
        }

        public void UpdateSimulatorMass()
        {
            if (machineParts.Count == 0)
            {
                return;
            }

            float mass = 0;
            Vector3 massCenter = Vector3.zero;
            float intertiaX = 0;
            float intertiaY = 0;
            float intertiaZ = 0;

            Quaternion inversedRotation = Quaternion.Inverse(MachineRigidBody.rotation);
            foreach (var part in machineParts)
            {
                // Vector3 partMassPosition = MachineRigidBody.transform.InverseTransformPoint(part.transform.TransformPoint(part.centerOfMass));
                // 手动算坐标，否则会受到 Rigidbody scale 的影响
                Vector3 partMassPosition = part.transform.position + part.transform.rotation * part.CenterOfMass;
                partMassPosition = inversedRotation * (partMassPosition - MachineRigidBody.position);

                mass += part.Mass;
                massCenter += part.Mass * partMassPosition;
                // 通过平行轴定理修正转动惯量(否则很明显的一个错误是,当只有一个方块的时候,转动惯量为 0)
                intertiaX += part.Mass * (partMassPosition.y * partMassPosition.y +
                                          partMassPosition.z * partMassPosition.z + 0.6667f);
                intertiaY += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.z * partMassPosition.z + 0.6667f);
                intertiaZ += part.Mass * (partMassPosition.x * partMassPosition.x +
                                          partMassPosition.y * partMassPosition.y + 0.6667f);
            }

            MachineRigidBody.mass = mass;
            MachineRigidBody.centerOfMass = massCenter / mass;
            MachineRigidBody.inertiaTensor = new Vector3(intertiaX, intertiaY, intertiaZ);
        }

        public Rigidbody GetSimulatorRigidbody()
        {
            return MachineRigidBody;
        }

        public MachineController GetOwnerMachine()
        {
            return this;
        }


        public virtual void Update()
        {
            if (DrawDebugShapes)
            {
                // 绘制质心
                // DebugExtension.DebugWireSphere(
                //     MachineRigidBody.worldCenterOfMass,
                //     Color.green,
                //     math.sqrt(MachineRigidBody.mass) / 10f,
                //     0.1f,
                //     false
                // );
                Popcron.Gizmos.Sphere(
                    MachineRigidBody.worldCenterOfMass,
                    math.sqrt(MachineRigidBody.mass) / 10f,
                    Color.green
                );
            }
        }

        private void OnDrawGizmos()
        {
            if (DrawDebugShapes)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawSphere(MachineRigidBody.worldCenterOfMass, math.sqrt(MachineRigidBody.mass) / 10f);
            }
        }

        public JsonMachine Serialize(Vector3 corePartPosition, Quaternion corePartRotation)
        {
            // 首先寻找根节点
            var rootPart = machineParts[0];
            while (rootPart.rootJoint != null &&
                   rootPart.rootJoint.IsAttached &&
                   rootPart.rootJoint.ConnectedJoint.Owner != null)
            {
                rootPart = rootPart.rootJoint.ConnectedJoint.Owner;
            }


            JsonMachine jsonMachine = new JsonMachine
            {
                // (新建机器的时候,会将根节点的位置设为原点)
                relativePosition = rootPart.transform.position - corePartPosition,
                relativeRotation = rootPart.transform.rotation * Quaternion.Inverse(corePartRotation),
                machineName = gameObject.name,
                machineParts = new List<JsonPart>()
            };


            if (machineParts.Count == 0)
            {
                return jsonMachine;
            }


            jsonMachine.machineParts.Add(rootPart.Serialize());
            foreach (var joint in rootPart.joints)
            {
                // 获取该根节点该 Joint 下的所有 Part
                var parts = joint.GetConnectedPartsRecursively();
                foreach (var part in parts)
                {
                    jsonMachine.machineParts.Add(part.Serialize());
                }
            }

            // foreach (var part in machineParts)
            // {
            //     jsonMachine.machineParts.Add(part.Serialize());
            // }

            return jsonMachine;
        }

        public void Deserialize(JsonMachine jsonMachine, Vector3 corePartPosition, Quaternion corePartRotation)
        {
            IPartManager partManager = TheLastTourArchitecture.Instance.GetManager<IPartManager>();

            gameObject.name = jsonMachine.machineName;
            foreach (var jsonPart in jsonMachine.machineParts)
            {
                PartController part = partManager.CreatePart(jsonPart.partName, jsonPart.partId);
                if (part)
                {
                    part.Deserialize(jsonPart);
                    PartController target = partManager.GetPartById(jsonPart.attachedPartId);
                    if (target != null)
                    {
                        part.Attach(target.GetJointById(jsonPart.attachedJointId), true, true);
                    }
                    else
                    {
                        AddPart(part);
                    }
                }
            }

            transform.position = corePartPosition + jsonMachine.relativePosition;
            transform.rotation = corePartRotation * jsonMachine.relativeRotation;

            isRestoreFromArchive = true;
        }
    }

    [Serializable]
    public struct JsonMachine
    {
        public Vector3 relativePosition;
        public Quaternion relativeRotation;
        public string machineName;
        public List<JsonPart> machineParts;
    }
}