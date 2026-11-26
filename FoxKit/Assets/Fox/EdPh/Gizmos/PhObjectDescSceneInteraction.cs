using UnityEngine;
using UnityEditor;
using Fox.Ph; 
using Fox.Core;
using Fox.Sim;

namespace Fox.EdPh
{
    public static class PhObjectDescSceneInteraction
    {

        private static string selectedBoneName;
        private static Vector3 selectedBonePosition;

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            SceneView.duringSceneGui -= OnSceneGUI;            
            SceneView.duringSceneGui += OnSceneGUI;
        }
        private static void OnSceneGUI(SceneView sceneView)
        {
            SimOnPhysics[] simulations = Object.FindObjectsByType<SimOnPhysics>(FindObjectsSortMode.None); 
          //  Debug.Log("Simulations found: " + simulations.Length);
            /*
            foreach (var bone in simulations[0].simBones)
            {
                Debug.Log(
                    "Bone: " + bone.Value.boneName +
                    " | Body: " + bone.Value.body.handle
                );
            }
*/
            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.Repaint &&
                !string.IsNullOrEmpty(selectedBoneName))
            {
                GUIStyle labelStyle = new GUIStyle(EditorStyles.whiteBoldLabel);
                labelStyle.normal.textColor = Color.black;
                labelStyle.hover.textColor = new Color(0f, 0.8f, 0.75f);
                labelStyle.active.textColor = new Color(0f, 0.8f, 0.75f);
                labelStyle.focused.textColor = new Color(0f, 0.8f, 0.75f);

                Handles.Label(selectedBonePosition, selectedBoneName, labelStyle);
            }

            if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0) return;
            Vector2 mousePosition = currentEvent.mousePosition;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(mousePosition);
            PhObjectDesc[] descriptions = Object.FindObjectsByType<PhObjectDesc>(FindObjectsSortMode.None);

            foreach (PhObjectDesc description in descriptions) 
            {
                //PhRigidBodyParam currentBody = null; 
               // PhRigidBody[] rigidBodies = Object.FindObjectsOfType<PhRigidBody>();
                PhRigidBodyParam[] rigidBodies = Object.FindObjectsByType<PhRigidBodyParam>(FindObjectsSortMode.None);
                //Debug.Log($"Found {rigidBodies.Length} PhRigidBodyComponents");
                /*
                foreach (PhRigidBodyParam rigidBody in rigidBodies)
                {
                    Debug.Log($"RigidBody GameObject: {rigidBody.gameObject.name}");
                }
                */
                PhRigidBodyParam currentBody = null;
                foreach (Entity entry in description.Bodies)
                {
                    if (entry is PhRigidBodyParam body)
                    {
                        currentBody = body;
                    }
                    else if (entry is PhPrimitiveShapeParam shape)
                    {
                        if (currentBody == null) continue;
                      //  Debug.Log($"Shape belongs to rigid body: {currentBody.name}");
                        Matrix4x4 bodyMatrix = Matrix4x4.TRS(
                            currentBody.GetDefaultPosition(),
                            currentBody.GetDefaultRotation(),
                            Vector3.one 
                        );

                        Matrix4x4 shapeMatrix = bodyMatrix * Matrix4x4.TRS(
                            shape.GetOffset(),
                            shape.GetRotation(),
                            Vector3.one
                        );
                        Ray localRay = new Ray(
                            shapeMatrix.inverse.MultiplyPoint(mouseRay.origin),
                            shapeMatrix.inverse.MultiplyVector(mouseRay.direction)
                        );
                        Vector3 origin = localRay.origin; 
                        Vector3 direction = localRay.direction;

                        float radius = shape.GetSize().x;
                        float halfHeight = shape.GetSize().y;
                        bool hit = false; 

                        if (shape.GetType() == PhPrimitiveShapeType.SPHERE){
                            float b = Vector3.Dot(origin, direction);
                            float c = Vector3.Dot(origin, origin) - radius * radius; 

                            float discriminant = b * b - c; 

                            if (discriminant >= 0){
                                float distance = -b -Mathf.Sqrt(discriminant);
                                if (distance < 0) distance = -b + Mathf.Sqrt(discriminant); 

                                hit = distance >= 0;
                            }
                        }
                        if (shape.GetType() == PhPrimitiveShapeType.CYLINDER)
                        {
                            // Curved side
                            float a = direction.x * direction.x + direction.z * direction.z;
                            float b = origin.x * direction.x + origin.z * direction.z;
                            float c = origin.x * origin.x + origin.z * origin.z - radius * radius;

                            float discriminant = b * b - a * c;

                            if (a > 0 && discriminant >= 0)
                            {
                                float distance = (-b - Mathf.Sqrt(discriminant)) / a;

                                if (distance < 0)
                                    distance = (-b + Mathf.Sqrt(discriminant)) / a;

                                if (distance >= 0)
                                {
                                    float hitY = origin.y + direction.y * distance;

                                    if (hitY >= -halfHeight && hitY <= halfHeight)
                                        hit = true;
                                }
                            }

                            // Top and bottom flat caps
                            if (Mathf.Abs(direction.y) > 0.0001f)
                            {
                                float topDistance = (halfHeight - origin.y) / direction.y;
                                Vector3 topHit = origin + direction * topDistance;

                                if (topDistance >= 0 &&
                                    topHit.x * topHit.x + topHit.z * topHit.z <= radius * radius)
                                {
                                    hit = true;
                                }

                                float bottomDistance = (-halfHeight - origin.y) / direction.y;
                                Vector3 bottomHit = origin + direction * bottomDistance;

                                if (bottomDistance >= 0 &&
                                    bottomHit.x * bottomHit.x + bottomHit.z * bottomHit.z <= radius * radius)
                                {
                                    hit = true;
                                }
                            }
                        }                                              
                        if (shape.GetType() == PhPrimitiveShapeType.CAPSULE)
                        {
                            Vector3 bottom = Vector3.down * halfHeight;
                            Vector3 top = Vector3.up * halfHeight;

                            float a = direction.x * direction.x + direction.z * direction.z;
                            float b = origin.x * direction.x + origin.z * direction.z;
                            float c = origin.x * origin.x + origin.z * origin.z - radius * radius;

                            float discriminant = b * b - a * c;

                            if (a > 0 && discriminant >= 0)
                            {
                                float distance = (-b - Mathf.Sqrt(discriminant)) / a;

                                if (distance >= 0){
                                    float hitY = origin.y + direction.y * distance;

                                    if (hitY >= -halfHeight && hitY <= halfHeight)
                                        hit = true;
                                }
                            }

                            Vector3 topOrigin = origin - Vector3.up * halfHeight;

                            float topB = Vector3.Dot(topOrigin, direction);
                            float topC = Vector3.Dot(topOrigin, topOrigin) - radius * radius; 
                            float topDiscriminant = topB * topB - topC; 

                            if (topDiscriminant >= 0)
                            {
                                float distance = -topB - Mathf.Sqrt(topDiscriminant);

                                if (distance < 0) distance = -topB + Mathf.Sqrt(topDiscriminant);

                                if (distance >= 0) hit = true;
                            }

                            Vector3 bottomOrigin = origin - Vector3.down * halfHeight; 

                            float bottomB = Vector3.Dot(bottomOrigin, direction);
                            float bottomC = Vector3.Dot(bottomOrigin, bottomOrigin) - radius * radius; 
                            float bottomDiscriminant = bottomB * bottomB - bottomC; 

                            if (bottomDiscriminant >= 0) {
                                float distance = -bottomB - Mathf.Sqrt(bottomDiscriminant);

                                if (distance < 0) distance = -bottomB + Mathf.Sqrt(bottomDiscriminant); 
                                if (distance >= 0) hit = true;
                            }

                        }
                        if (shape.GetType() == PhPrimitiveShapeType.BOX){
                            Vector3 size = shape.GetSize(); 

                            float tMin = float.NegativeInfinity; 
                            float tMax = float.PositiveInfinity; 

                            for (int axis = 0; axis < 3; axis++){
                                float originAxis = origin[axis];
                                float directionAxis = direction[axis]; 

                                if (Mathf.Abs(directionAxis) < 0.0001f){
                                    if (originAxis < -size[axis] || originAxis > size[axis]) break;
                                }
                                else{
                                    float t1 = (-size[axis] - originAxis) / directionAxis; 
                                    float t2 = (size[axis] - originAxis) / directionAxis; 

                                    if (t1 > t2) {
                                        float temp = t1; 
                                        t1 = t2;
                                        t2 = temp; 
                                    }
                                    tMin = Mathf.Max(tMin, t1);
                                    tMax = Mathf.Min(tMax, t2); 

                                    if (tMin > tMax) break; 
                                }
                            }
                            hit = tMax >= Mathf.Max(tMin, 0f);
                        }
                        /*
                        if (hit){
                            Selection.activeGameObject = currentBody.gameObject;
                            currentEvent.Use();
                        } 
                        */     
                        if (hit)
                        {
                            /*
                            var units = Object.FindObjectsByType<Sim.SimAssociationUnit>(
                                FindObjectsSortMode.None
                            );
                            Debug.Log($"Clicked body: {currentBody.name} ({currentBody.GetType().Name})");
                            foreach (var unit in units)
                            {
                                Entity linkedEntity = unit.body.handle;
                                Debug.Log(
                                    $"Association: {unit.name} | Bone: {unit.boneName} | " +
                                    $"Linked entity: {(linkedEntity == null ? "NULL" : linkedEntity.GetType().Name)} | " +
                                    $"Same reference: {ReferenceEquals(linkedEntity, currentBody)}"
                                );
                            }
*/
                            string boneName = null; 
                            foreach (var bone in simulations[0].SimBones)
                            {
                                if (bone.Value.Body.handle == currentBody)
                                {
                                    boneName = bone.Value.boneName; 
                                    break;
                                }
                            }

                            if (boneName == null) {
                                foreach (var bone in simulations[0].SimRootBones)
                                {
                                    if (bone.Value.Body.handle == currentBody)
                                    {
                                        boneName = bone.Value.boneName; 
                                        break;
                                    }
                                }
                            }
                            selectedBoneName = boneName;
                           // selectedBonePosition = currentBody.transform.position + Vector3.up * 0.15f; 
                           selectedBonePosition = shapeMatrix.MultiplyPoint(Vector3.zero) + Vector3.up * 0.15f;
                            SceneView.RepaintAll();

                            Selection.activeGameObject = currentBody.gameObject;
                            currentEvent.Use();
                        }                                          
                    }
                }
            }
        }
    }
}
