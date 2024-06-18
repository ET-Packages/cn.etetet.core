using System;
using System.Collections.Generic;

namespace ET
{
    public class EntitySystem
    {
        private readonly Dictionary<Type, Queue<EntityRef<Entity>>> queues = new();
        
        private Queue<EntityRef<Entity>> GetQueue(Type type)
        {
            if (!this.queues.TryGetValue(type, out var queue))
            {
                queue = new Queue<EntityRef<Entity>>();
                this.queues.Add(type, queue);
            }

            return queue;
        }
        
        public virtual void RegisterSystem(Entity component)
        {
            Type type = component.GetType();

            TypeSystems.OneTypeSystems oneTypeSystems = EntitySystemSingleton.Instance.TypeSystems.GetOneTypeSystems(type);
            if (oneTypeSystems == null)
            {
                return;
            }

            foreach (Type queueType in oneTypeSystems.ClassType)
            {
                var queue = this.GetQueue(queueType);
                queue.Enqueue(component);
            }
        }
        
        public void Publish<T>(T t) where T: struct
        {
            Type systemType = typeof(AClassEventSystem<T>);
            Queue<EntityRef<Entity>> queue = this.GetQueue(systemType);
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }
                
                if (component is not AClassEventSystem<T>)
                {
                    continue;
                }

                try
                {
                    List<SystemObject> systems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), systemType);
                    if (systems == null)
                    {
                        continue;
                    }

                    queue.Enqueue(component);

                    foreach (AClassEventSystem<T> classSystem in systems)
                    {
                        try
                        {
                            classSystem.Run(component, t);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", e);
                }

            }
        }
        

        public void Update()
        {
            Queue<EntityRef<Entity>> queue = this.GetQueue(typeof(IUpdateSystem));
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }
                
                if (component is not IUpdate)
                {
                    continue;
                }

                try
                {
                    List<SystemObject> iUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (IUpdateSystem));
                    if (iUpdateSystems == null)
                    {
                        continue;
                    }

                    queue.Enqueue(component);

                    foreach (IUpdateSystem iUpdateSystem in iUpdateSystems)
                    {
                        try
                        {
                            iUpdateSystem.Run(component);
                        }
                        catch (Exception e)
                        {
                            Log.Error(e);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"entity system update fail: {component.GetType().FullName}", e);
                }

            }
        }

        public void LateUpdate()
        {
            Queue<EntityRef<Entity>> queue = this.GetQueue(typeof(ILateUpdateSystem));
            int count = queue.Count;
            while (count-- > 0)
            {
                Entity component = queue.Dequeue();
                if (component == null)
                {
                    continue;
                }

                if (component.IsDisposed)
                {
                    continue;
                }
                
                if (component is not ILateUpdate)
                {
                    continue;
                }

                List<SystemObject> iLateUpdateSystems = EntitySystemSingleton.Instance.TypeSystems.GetSystems(component.GetType(), typeof (ILateUpdateSystem));
                if (iLateUpdateSystems == null)
                {
                    continue;
                }

                queue.Enqueue(component);

                foreach (ILateUpdateSystem iLateUpdateSystem in iLateUpdateSystems)
                {
                    try
                    {
                        iLateUpdateSystem.Run(component);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }
    }
}