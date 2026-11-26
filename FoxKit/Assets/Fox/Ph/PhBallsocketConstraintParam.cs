using Fox.Core;
using Fox.Core.Utils;
using UnityEngine;
using UnityEditor;

namespace Fox.Ph
{
    public partial class PhBallsocketConstraintParam : Fox.Ph.PhConstraintParam
    {
        internal bool GetLimitedFlag() => limitedFlag;
        internal void SetLimitedFlag(bool value) => limitedFlag = value;

        // See: fox::ph::BallsocketConstraint::GetRefAForAc
        internal UnityEngine.Vector3 GetRefA() => refA;
        internal void SetRefA(UnityEngine.Vector3 value) => refA = value;

        internal UnityEngine.Vector3 GetRefB() => refB;
        internal void SetRefB(UnityEngine.Vector3 value) => refB = value;

        internal float GetLimit() => limit;
        internal void SetLimit(float value) => limit = value;

        internal bool GetSpringFlag() => springFlag;
        internal void SetSpringFlag(bool value) => springFlag = value;

        internal bool GetSpringRefCustomFlag() => springRefCustomFlag;
        internal void SetSpringRefCustomFlag(bool value) => springRefCustomFlag = value;

        internal UnityEngine.Vector3 GetSpringRef() => springRef;
        internal void SetSpringRef(UnityEngine.Vector3 value) => springRef = value;

        internal float GetSpringConstant() => springConstant;
        internal void SetSpringConstant(float value) => springConstant = value;

        internal float GetFlexibility() => flexibility;
        internal void SetFlexibility(float value) => flexibility = value;

        internal bool GetStopTwistFlag() => stopTwistFlag;
        internal void SetStopTwistFlag(bool value) => stopTwistFlag = value;

        public override void OnDeserializeEntity(TaskLogger logger)
        {
            base.OnDeserializeEntity(logger);
            
            refA = Fox.Math.FoxToUnityVector3(refA);
            refB = Fox.Math.FoxToUnityVector3(refB);
            springRef = Fox.Math.FoxToUnityVector3(springRef);
        }

        public override void OnSerializeEntity(EntityExportContext context)
        {
            base.OnSerializeEntity(context);
            
            context.OverrideProperty(nameof(refA), Fox.Math.UnityToFoxVector3(refA));
            context.OverrideProperty(nameof(refB), Fox.Math.UnityToFoxVector3(refB));
            context.OverrideProperty(nameof(springRef), Fox.Math.UnityToFoxVector3(springRef));
        }

       public override void DrawGizmos()
        {
            //if (limitedFlag && limit > 0)
            if (limitedFlag && limit > 0)
            {
                if (EditorPrefs.GetBool("FoxKit_ShowLimitCones", false)){
                    //This is the Gizmo colour for the Cone represented by the limit angle measured in degrees 
                    Gizmos.color = Color.green;

                    Vector3 axis = refA.normalized;

                    if (axis.sqrMagnitude > 0.0001f)
                    {
                        const float length = 0.1f;
                        const int segments = 32;
                        const int spokes = 8;

                        float angle = Mathf.Clamp(limit, 0f, 90f) * Mathf.Deg2Rad;

                        Vector3 reference = Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.99f
                            ? Vector3.right
                            : Vector3.up;

                        Vector3 right = Vector3.Cross(axis, reference).normalized;
                        Vector3 up = Vector3.Cross(right, axis).normalized;

                        Vector3 previousPoint = Vector3.zero;

                        for (int i = 0; i <= segments; i++)
                        {
                            float radians = i * Mathf.PI * 2f / segments;

                            Vector3 radial =
                                right * Mathf.Cos(radians) +
                                up * Mathf.Sin(radians);

                            Vector3 point = length *
                                (axis * Mathf.Cos(angle) +
                                radial * Mathf.Sin(angle));

                            if (i > 0)
                                Gizmos.DrawLine(previousPoint, point);

                            previousPoint = point;
                        }

                        for (int i = 0; i < spokes; i++)
                        {
                            float radians = i * Mathf.PI * 2f / spokes;

                            Vector3 radial =
                                right * Mathf.Cos(radians) +
                                up * Mathf.Sin(radians);

                            Vector3 point = length *
                                (axis * Mathf.Cos(angle) +
                                radial * Mathf.Sin(angle));

                            Gizmos.DrawLine(Vector3.zero, point);
                        }
                    }
                }         
            }
            //Recal the color of the Object Gizmos, to have a different colour for the ref vectors thats not a spring ref               
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Vector3.zero, refA * 0.1f);               
            if (springFlag && springConstant > 0)
            {
                
                Vector3 actualSpringRef = springRefCustomFlag ? springRef : refB;
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(Vector3.zero, actualSpringRef * 0.1f);
            }             
        }
    }
}
