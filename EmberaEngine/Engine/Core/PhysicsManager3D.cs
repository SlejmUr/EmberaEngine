using System;
using System.Collections.Generic;
using System.Numerics;
using BepuPhysics;
using BepuPhysics.Collidables;
using BepuPhysics.CollisionDetection;
using BepuPhysics.Constraints;
using BepuUtilities;
using BepuUtilities.Memory;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Utilities;

namespace EmberaEngine.Engine.Core
{
    public struct PhysicsObjectHandle
    {
        public StaticHandle StaticHandle;
        public BodyHandle BodyHandle;
        public BodyReference BodyReference;
        public bool IsStatic;
    }

    public struct PhysicsShapeInfo
    {
        public TypedIndex Shape;
        public BodyInertia Inertia;
    }

    public class PhysicsManager3D
    {
        public static Vector3 GlobalGravity = new Vector3(0, -9.8f, 0);
        private const float TimestepDuration = 1f / 60f;

        private readonly Dictionary<string, TypedIndex> shapeCache = new();
        private readonly Dictionary<BodyHandle, Transform> dynamicBodies = new();

        private BufferPool bufferPool;
        private PhysicsNarrowPhaseCallback narrowCallback;
        private PhysicsPoseIntegratorCallback integratorCallback;
        private SolveDescription solveDescription;
        private ThreadDispatcher threadDispatcher;
        private Simulation simulation;

        public void Initialize()
        {
            bufferPool = new BufferPool();
            narrowCallback = new PhysicsNarrowPhaseCallback();
            integratorCallback = new PhysicsPoseIntegratorCallback(GlobalGravity);
            solveDescription = new SolveDescription(8, 1);
            threadDispatcher = new ThreadDispatcher(Environment.ProcessorCount);
            simulation = Simulation.Create(bufferPool, narrowCallback, integratorCallback, solveDescription);
        }

        public void Update(float dt)
        {
            if (simulation != null)
                simulation.Timestep(TimestepDuration, threadDispatcher);
        }

        public bool StaticExists(StaticHandle handle) => simulation.Statics.StaticExists(handle);
        public bool DynamicExists(BodyHandle handle) => simulation.Bodies.BodyExists(handle);

        public void SetPhysicsPosition(BodyHandle handle, Transform transform)
        {
            var body = simulation.Bodies.GetBodyReference(handle);

            var position = Helper.ToNumerics3(transform.position);
            var euler = Helper.ToNumerics3(transform.rotation);
            var rotation = Quaternion.CreateFromYawPitchRoll(euler.X, euler.Y, euler.Z);

            body.Pose.Position = position;
            body.Pose.Orientation = rotation;
        }

        public void SetPhysicsPosition(StaticHandle handle, Transform transform)
        {
            var body = simulation.Statics.GetStaticReference(handle);

            var position = Helper.ToNumerics3(transform.position);
            var euler = Helper.ToNumerics3(transform.rotation);
            var rotation = Quaternion.CreateFromYawPitchRoll(euler.X, euler.Y, euler.Z);

            body.Pose.Position = position;
            body.Pose.Orientation = rotation;
        }

        public void RemovePhysicsObject(PhysicsObjectHandle handle)
        {
            if (handle.IsStatic)
            {
                simulation.Statics.Remove(handle.StaticHandle);
            }
            else
            {
                simulation.Bodies.Remove(handle.BodyHandle);
                dynamicBodies.Remove(handle.BodyHandle);
            }
        }

        public PhysicsObjectHandle AddPhysicsObject(Transform transform, RigidBody3D rigidBody, ColliderComponent3D collider)
        {
            var shapeInfo = CreateShape(rigidBody, collider);

            BodyHandle bodyHandle = default;
            StaticHandle staticHandle = default;

            var position = Helper.ToNumerics3(transform.position);
            var rotation = Helper.ToQuaternion(Helper.ToRadians(Helper.ToNumerics3(transform.rotation)));

            switch (rigidBody.Rigidbody3DType)
            {
                case Rigidbody3DType.Static:
                    staticHandle = simulation.Statics.Add(new StaticDescription(position, rotation, shapeInfo.Shape));
                    break;

                case Rigidbody3DType.Dynamic:
                    bodyHandle = simulation.Bodies.Add(BodyDescription.CreateDynamic(position, shapeInfo.Inertia, shapeInfo.Shape, 0.01f));
                    dynamicBodies[bodyHandle] = transform;
                    break;

                case Rigidbody3DType.Kinematic:
                    bodyHandle = simulation.Bodies.Add(BodyDescription.CreateKinematic(position, shapeInfo.Shape, 0.01f));
                    break;
            }

            return new PhysicsObjectHandle
            {
                StaticHandle = staticHandle,
                BodyHandle = bodyHandle,
                IsStatic = rigidBody.Rigidbody3DType == Rigidbody3DType.Static,
                BodyReference = rigidBody.Rigidbody3DType == Rigidbody3DType.Static
                    ? default
                    : simulation.Bodies.GetBodyReference(bodyHandle)
            };
        }

        public PhysicsShapeInfo CreateShape(RigidBody3D rigidBody, ColliderComponent3D collider)
        {
            TypedIndex shapeIndex;
            BodyInertia inertia;
            var key = GenerateShapeKey(collider);

            // Future reuse logic can go here if `false` is removed.
            if (shapeCache.TryGetValue(key, out shapeIndex) && false)
            {
                inertia = collider.ColliderShape switch
                {
                    ColliderShapeType.Sphere => new Sphere(collider.Radius).ComputeInertia(rigidBody.Mass),
                    ColliderShapeType.Box => new Box(collider.Size.X * 2, collider.Size.Y * 2, collider.Size.Z * 2).ComputeInertia(rigidBody.Mass),
                    _ => default
                };
            }
            else
            {
                switch (collider.ColliderShape)
                {
                    case ColliderShapeType.Sphere:
                        var sphere = new Sphere(collider.Radius);
                        shapeIndex = simulation.Shapes.Add(sphere);
                        inertia = sphere.ComputeInertia(rigidBody.Mass);
                        break;

                    case ColliderShapeType.Box:
                    default:
                        var box = new Box(collider.Size.X, collider.Size.Y, collider.Size.Z);
                        shapeIndex = simulation.Shapes.Add(box);
                        inertia = box.ComputeInertia(rigidBody.Mass);
                        break;
                }

                shapeCache[key] = shapeIndex;
            }

            return new PhysicsShapeInfo
            {
                Shape = shapeIndex,
                Inertia = inertia
            };
        }

        private static string GenerateShapeKey(ColliderComponent3D collider) =>
            collider.ColliderShape switch
            {
                ColliderShapeType.Sphere => $"Sphere:{collider.Radius:F4}",
                ColliderShapeType.Box => $"Box:{collider.Size.X:F4},{collider.Size.Y:F4},{collider.Size.Z:F4}",
                _ => "Unknown"
            };

        public void Dispose()
        {
            simulation.Dispose();
            threadDispatcher.Dispose();
            bufferPool.Clear();
        }
    }

    public struct PhysicsPoseIntegratorCallback : IPoseIntegratorCallbacks
    {
        public AngularIntegrationMode AngularIntegrationMode => AngularIntegrationMode.Nonconserving;
        public bool AllowSubstepsForUnconstrainedBodies => false;
        public bool IntegrateVelocityForKinematics => false;

        private Vector3 Gravity;
        private Vector3Wide gravityWideDt;

        public PhysicsPoseIntegratorCallback(Vector3 gravity) : this()
        {
            Gravity = gravity;
        }

        public void Initialize(Simulation simulation) { }

        public void PrepareForIntegration(float dt)
        {
            gravityWideDt = Vector3Wide.Broadcast(Gravity * dt);
        }

        public void IntegrateVelocity(Vector<int> bodyIndices, Vector3Wide position, QuaternionWide orientation,
            BodyInertiaWide localInertia, Vector<int> integrationMask, int workerIndex, Vector<float> dt,
            ref BodyVelocityWide velocity)
        {
            velocity.Linear += gravityWideDt;
        }
    }

    struct PhysicsNarrowPhaseCallback : INarrowPhaseCallbacks
    {
        public void Initialize(Simulation simulation) { }

        public bool AllowContactGeneration(int workerIndex, CollidableReference a, CollidableReference b, ref float speculativeMargin)
        {
            return a.Mobility == CollidableMobility.Dynamic || b.Mobility == CollidableMobility.Dynamic;
        }

        public bool AllowContactGeneration(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB) => true;

        public bool ConfigureContactManifold<TManifold>(int workerIndex, CollidablePair pair, ref TManifold manifold,
            out PairMaterialProperties pairMaterial) where TManifold : unmanaged, IContactManifold<TManifold>
        {
            pairMaterial = new PairMaterialProperties
            {
                FrictionCoefficient = 1f,
                MaximumRecoveryVelocity = 2f,
                SpringSettings = new SpringSettings(30, 1)
            };
            return true;
        }

        public bool ConfigureContactManifold(int workerIndex, CollidablePair pair, int childIndexA, int childIndexB,
            ref ConvexContactManifold manifold) => true;

        public void Dispose() { }
    }
}
