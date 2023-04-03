using System;
using UnityEngine;

namespace RK.Retales.Utility {
    [AddComponentMenu("_RKRetales/Utility/GizmoDrawer")]
    public class GizmoDrawer : MonoBehaviour {
        [SerializeField] private GizmoType type = GizmoType.WireCube;
        [SerializeField] private Vector3 size = new(1, 1, 1);
        [SerializeField] private Vector3 offset;
        [SerializeField] private Color color = Color.red;

        protected virtual void OnDrawGizmos() {
            Gizmos.color = color;
            Gizmos.matrix = Matrix4x4.TRS(transform.position + offset, transform.rotation,
                transform.localScale);

            switch(type) {
                case GizmoType.WireCube:
                    Gizmos.DrawWireCube(Vector3.zero, size);
                    break;

                case GizmoType.Cube:
                    Gizmos.DrawCube(Vector3.zero, size);
                    break;

                case GizmoType.WireSphere:
                    Gizmos.DrawWireSphere(Vector3.zero, size.x / 2f);
                    break;

                case GizmoType.Sphere:
                    Gizmos.DrawSphere(Vector3.zero, size.x / 2f);
                    break;

                default:
                    throw new NotImplementedException("How did you even get here?");
            }
        }

        private enum GizmoType {
            WireCube, Cube, WireSphere,
            Sphere
        }
    }
    
    public static class StaticGizmoDrawer {
        public static void DrawGizmoArrow(Vector3 origin, Vector3 direction, Color color, float arrowHeadLength = 3f, float arrowHeadAngle = 25f)
        {
            Gizmos.color = color;
            Gizmos.DrawRay(origin, direction);
	       
            DrawArrowEnd(true, origin, direction, color, arrowHeadLength, arrowHeadAngle);
        }
        
        private static void DrawArrowEnd (bool drawGizmos, Vector3 arrowEndPosition, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 40.0f)
        {
            if(direction == Vector3.zero) return;

            var right = Quaternion.LookRotation (direction) * Quaternion.Euler (arrowHeadAngle, 0, 0) * Vector3.back;
            var left = Quaternion.LookRotation (direction) * Quaternion.Euler (-arrowHeadAngle, 0, 0) * Vector3.back;
            var up = Quaternion.LookRotation (direction) * Quaternion.Euler (0, arrowHeadAngle, 0) * Vector3.back;
            var down = Quaternion.LookRotation (direction) * Quaternion.Euler (0, -arrowHeadAngle, 0) * Vector3.back;
            if (drawGizmos) 
            {
                Gizmos.color = color;
                Gizmos.DrawRay (arrowEndPosition + direction, right * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, left * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, up * arrowHeadLength);
                Gizmos.DrawRay (arrowEndPosition + direction, down * arrowHeadLength);
            }
            else
            {
                Debug.DrawRay (arrowEndPosition + direction, right * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, left * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, up * arrowHeadLength, color);
                Debug.DrawRay (arrowEndPosition + direction, down * arrowHeadLength, color);
            }
        }

    }
}