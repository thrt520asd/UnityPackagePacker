using UnityEngine;
using UnityEditor;
using System;

namespace ChannelBuilder
{
    [InitializeOnLoad]
    public class EditorUpdater
    {
        private float delay;
        private Action callback;
        private float _startupTime;
        public void DelayedCall(float delay, Action callback)
        {
            this.delay = delay;
            this.callback = callback;
            _startupTime = Time.realtimeSinceStartup;
            EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(Update));
        }

        private void Update()
        {
            if (Time.realtimeSinceStartup - _startupTime >= delay)
            {
                if (EditorApplication.update != null)
                {
                    EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(Update));
                }
                if (callback != null)
                {
                    callback();
                }
            }
        }
    }
}