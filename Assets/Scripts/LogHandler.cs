using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

namespace RK.Retales.Utility {
    [AddComponentMenu("_RKRetales/Utility/Logger")]
    public class LogHandler : MonoBehaviour {
        [Header("Settings")]
        [SerializeField] private bool showLogs;
        [SerializeField] private string prefix;
        [SerializeField] private Color prefixColor;

        private string _hexColor;

        protected virtual void OnValidate() {
            _hexColor = ColorUtility.ToHtmlStringRGB(prefixColor);
        }

        public virtual void Log(object message, Object sender) {
            if(!showLogs) return;

            Debug.Log($"<color=#{_hexColor}>{prefix}: {message}</color>", sender);
        }
        
        public static void StaticLog(object message, Color color, Object sender) {
            var hexColor = ColorUtility.ToHtmlStringRGB(color);
            
            Debug.Log($"<color=#{hexColor}>: {message}</color>", sender);
        }
    }
}