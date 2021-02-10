using System;
using System.Collections.Generic;
using UnityEngine;

namespace IVE
{
    public class UpdateEx : MonoBehaviour
    {
        private static UpdateEx _defaultIns = null;
        private static UpdateEx defaultIns
        {
            get
            {
                if (_defaultIns == null)
                {
                    GameObject g = new GameObject("Default UpdateEx Instance");
                    _defaultIns = g.AddComponent<UpdateEx>();
                    _defaultIns.updateNodeList = new List<UpdateNode>[Enum.GetValues(typeof(UpdateType)).Length];
                    for (int i = 0; i < _defaultIns.updateNodeList.Length; i++)
                        _defaultIns.updateNodeList[i] = new List<UpdateNode>();
                }

                return _defaultIns;
            }
        }

        private static UpdateEx _dontDestroyIns = null;

        private static UpdateEx dontDestroyIns
        {
            get
            {
                if (_dontDestroyIns == null)
                {
                    GameObject g = new GameObject("Dont Destroy UpdateEx Instance");
                    DontDestroyOnLoad(g);
                    _dontDestroyIns = g.AddComponent<UpdateEx>();
                    _dontDestroyIns.updateNodeList = new List<UpdateNode>[Enum.GetValues(typeof(UpdateType)).Length];
                    for (int i = 0; i < _dontDestroyIns.updateNodeList.Length; i++)
                        _dontDestroyIns.updateNodeList[i] = new List<UpdateNode>();
                }

                return _dontDestroyIns;
            }
        }


        public enum UpdateType
        {
            Default,
            Late,
            Fixed,
        }
        public enum TimeType
        {
            Default,
            Unscaled,
        }
        public enum InstanceType
        {
            Default,
            DontDestroy,
        }
        
        
        public class Node
        {
            public float delay = 0;
            public float timeLength = 0;
            public Action beforeAction = null;
            public Action<float> updateAction = null;
            public Action afterAction = null;
            
            public UpdateType updateType = UpdateType.Default;
            public InstanceType instanceType = InstanceType.Default;
            public TimeType timeType = TimeType.Default;
        }
        
        private class UpdateNode : Node
        {
            public float timeOnAdd = -1;
            public bool hasCallBeforeAction = false;
            public bool hasCallAfterAction = false;

            public UpdateNode()
            {
            }

            public UpdateNode(Node node)
            {
                this.delay = node.delay;
                this.timeLength = node.timeLength;
                this.beforeAction = node.beforeAction;
                this.updateAction = node.updateAction;
                this.afterAction = node.afterAction;
                
                this.updateType = node.updateType;
                this.instanceType = node.instanceType;
                this.timeType = node.timeType;
            }
        }

        private List<UpdateNode>[] updateNodeList;

        public static void Add(Node node)
        {
            UpdateNode _updateNode = new UpdateNode(node);
            switch (_updateNode.timeType)
            {
                case TimeType.Unscaled:
                    _updateNode.timeOnAdd = Time.unscaledTime;
                    break;
                case TimeType.Default:
                default:
                    _updateNode.timeOnAdd = Time.time;
                    break;
            }

            switch (_updateNode.instanceType)
            {
                case InstanceType.DontDestroy:
                    dontDestroyIns.updateNodeList[(int) _updateNode.updateType].Add(_updateNode);
                    break;
                case InstanceType.Default:
                default:
                    defaultIns.updateNodeList[(int) _updateNode.updateType].Add(_updateNode);
                    break;
            }
        }

        /// <summary>
        /// 下一帧执行
        /// </summary>
        /// <param name="action"></param>
        /// <param name="instanceType"></param>
        public static void NextFrameRun(Action action, InstanceType instanceType = InstanceType.Default)
        {
            DelayRun(0.001f, action, instanceType);
        }

        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay"></param>
        /// <param name="action"></param>
        /// <param name="instanceType"></param>
        public static void DelayRun(float delay, Action action, InstanceType instanceType = InstanceType.Default)
        {
            Add(new Node()
            {
                delay = delay,
                instanceType = instanceType,
                afterAction = action,
            });
        }
        

        private void Update()
        {
            for (int i = 0; i < updateNodeList[(int) UpdateType.Default].Count; i++)
            {
                if (NodeUpdate(updateNodeList[(int) UpdateType.Default][i]))
                    updateNodeList[(int) UpdateType.Default].RemoveAt(i--);
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < updateNodeList[(int) UpdateType.Late].Count; i++)
            {
                if (NodeUpdate(updateNodeList[(int) UpdateType.Late][i]))
                    updateNodeList[(int) UpdateType.Late].RemoveAt(i--);
            }
        }

        private void FixedUpdate()
        {
            
            for (int i = 0; i < updateNodeList[(int) UpdateType.Fixed].Count; i++)
            {
                if (NodeUpdate(updateNodeList[(int) UpdateType.Fixed][i]))
                    updateNodeList[(int) UpdateType.Fixed].RemoveAt(i--);
            }
        }

        /// <summary>
        /// 节点的生命周期结束时返回true，提醒销毁这个节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private bool NodeUpdate(UpdateNode node)
        {
            float currentTime = node.timeType == TimeType.Default ? Time.time : Time.unscaledTime;
            
            if (currentTime >= node.timeOnAdd + node.delay)
            {
                if (!node.hasCallBeforeAction)
                {
                    try
                    {
                        node.beforeAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Console.WriteLine(e);
                        throw;
                    }
                    node.hasCallBeforeAction = true;
                }

                if (node.updateAction != null)
                {
                    float progress = 0;
                    if (Mathf.Abs(node.timeLength) <= 0.001f)
                        progress = 1;
                    else
                        progress = (currentTime - node.timeOnAdd - node.delay) / node.timeLength;
                    progress = Mathf.Clamp01(progress);
                    
                    try
                    {
                        node.updateAction(progress);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Console.WriteLine(e);
                        throw;
                    }
                }
                
            }

            if (currentTime >= node.timeOnAdd + node.delay + node.timeLength)
            {
                if (!node.hasCallAfterAction)
                {
                    try
                    {
                        node.afterAction?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                        Console.WriteLine(e);
                        throw;
                    }
                    node.hasCallAfterAction = true;
                }

                return true;
            }

            return false;
        }
    }
}
