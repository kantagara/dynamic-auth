using System;
using UnityEngine;

namespace DynamicSDK.Unity.Messages
{
    /// <summary>
    /// Base interface for all messages between Unity and Web
    /// </summary>
    public interface IUnityMessage
    {
        string type { get; set; }
        string action { get; set; }
        long timestamp { get; set; }
        string requestId { get; set; }
    }

    /// <summary>
    /// Generic base message class
    /// </summary>
    [Serializable]
    public abstract class BaseMessage : IUnityMessage
    {
        public string type;
        public string action;
        public long timestamp;
        public string requestId;

        string IUnityMessage.type { get => type; set => type = value; }
        string IUnityMessage.action { get => action; set => action = value; }
        long IUnityMessage.timestamp { get => timestamp; set => timestamp = value; }
        string IUnityMessage.requestId { get => requestId; set => requestId = value; }

        protected BaseMessage()
        {
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            requestId = Guid.NewGuid().ToString();
        }
    }
} 