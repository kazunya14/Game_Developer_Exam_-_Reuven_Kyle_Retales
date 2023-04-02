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
}