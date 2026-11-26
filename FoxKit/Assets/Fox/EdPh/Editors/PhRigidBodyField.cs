
using Fox.Core;
using Fox.EdCore;
using Fox.Ph;
using UnityEditor;
using UnityEngine.UIElements;
using FloatField = Fox.EdCore.FloatField;

namespace Fox.EdPh
{
    [CustomEntityField]
    public class PhRigidBodyField : EntityField<PhRigidBody>
    {
        static PhRigidBodyField()
        {
            CustomEntityFieldDesc desc = new CustomEntityFieldDesc
            {
                Constructor = () => new PhRigidBodyField(),
                BodyOverrideBehavior = BuildBodyOverrideBehavior.SpecificClassOverride,
                BuildBody = BuildBody
            };
            UnityEngine.Debug.Log("PhRigidBodyField static constructor executed");            

            CustomEntityFieldManager.Register(PhRigidBody.ClassInfo, desc);
        }

        private static void BuildBody(EntityFieldBuildContext context)
        {
            context.BodyElement.Add(new HelpBox(
                "CUSTOM PhRigidBodyField IS ACTIVE",
                HelpBoxMessageType.Info
            ));            
            PhRigidBody rigidBody = context.Object.targetObject as PhRigidBody;

            if (rigidBody == null)
                return;

            FloatField massField = new FloatField("mass");
            massField.SetValueWithoutNotify(rigidBody.mass);

            massField.RegisterValueChangedCallback(evt =>
            {
                PhRigidBodyParam param = rigidBody.GetParam();

                if (param == null)
                    return;

                Undo.RecordObject(param, "Change Rigid Body Mass");
                rigidBody.mass = evt.newValue;
            });

            context.BodyElement.Add(massField);

        }
    }
}
