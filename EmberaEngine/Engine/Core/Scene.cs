using System;
using System.Collections.Generic;
using EmberaEngine.Engine.Components;
using EmberaEngine.Engine.Rendering;
using nkast.Aether.Physics2D.Common;

namespace EmberaEngine.Engine.Core
{
    public class Scene
    {

        public List<GameObject> GameObjects;
        public List<CameraComponent3D> Cameras;

        public PhysicsManager2D PhysicsManager2D;
        public PhysicsManager3D PhysicsManager3D;
        public bool IsPlaying = false;

        public event Action<Component> OnComponentAdded = (c) => {};
        public event Action<Component> OnComponentRemoved = (c) => {};

        public Scene()
        {
            GameObjects = new List<GameObject>();
            PhysicsManager2D = new PhysicsManager2D();
            PhysicsManager3D = new PhysicsManager3D();
        }

        public void Initialize()
        {
            Cameras = new List<CameraComponent3D>();
            PhysicsManager2D.Initialize();
            PhysicsManager3D.Initialize();
        }

        public GameObject addGameObject(string name)
        {
            GameObject gameObject = new GameObject();
            gameObject.Name = name;
            gameObject.Scene = this;
            GameObjects.Add(gameObject);
            return gameObject;
        }

        public void addGameObject(GameObject gameObject)
        {
            gameObject.Scene = this;
            GameObjects.Add(gameObject);
        }

        public T GetComponent<T>() where T : Component, new()
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                T component = GameObjects[i].GetComponent<T>();

                if (component != null)
                {
                    return component;
                }
            }
            return null;
        }

        public List<Component> GetComponents()
        {
            List<Component> components = new List<Component>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                components.AddRange(GameObjects[i].GetComponentsRecursive());
            }
            return components;
        }

        public List<T> GetComponentsOfType<T>() where T : Component, new()
        {
            List<T> components = new List<T>();

            for (int i = 0; i < GameObjects.Count; i++)
            {
                T component = GameObjects[i].GetComponent<T>();

                if (component != null)
                {
                    components.Add(component);
                }
            }
            return components;
        }

        public void removeGameObject(GameObject gameObject)
        {
            gameObject.OnDestroy();
            GameObjects.Remove(gameObject);
        }

        public void SetMainCamera(CameraComponent3D camera)
        {
            int index = this.Cameras.IndexOf(camera);
            for (int i = 0; i < this.Cameras.Count; i++)
            {
                if (i == index) { continue; }

                this.Cameras[i].isDefault = false;
            }

            Renderer3D.SetRenderCamera(camera.camera);
        }

        public void AddCamera(CameraComponent3D camera)
        {
            this.Cameras.Add(camera);
        }
        
        public void RemoveCamera(CameraComponent3D camera)
        {
            this.Cameras.Remove(camera);
        }

        public void Destroy()
        {
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.OnDestroy();
            }
            PhysicsManager3D.Dispose();
        }

        public void Play()
        {
            IsPlaying = true;
            foreach (GameObject gameObject in GameObjects)
            {
                gameObject.OnStart();
            }
        }

        public void OnUpdate(float dt)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                GameObjects[i].OnUpdate(dt);
            }

            PhysicsManager2D.Update(dt);
            PhysicsManager3D.Update(dt);
        }

        public void ComponentAdded(Component component)
        {
            OnComponentAdded.Invoke(component);

            if (IsPlaying)
            {
                component.OnStart();
                Console.WriteLine("started");
            }
        }

        public void ComponentRemoved(Component component)
        {
            OnComponentRemoved.Invoke(component);
        }

        public void OnResize(float width, float height)
        {
            //for (int i = 0; i < Cameras.Count; i++)
            //{
            //    Cameras[i].
            //}
        }
    }
}
