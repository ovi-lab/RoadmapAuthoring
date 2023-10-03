using MixedReality.Toolkit.SpatialManipulation;
using MixedReality.Toolkit.UX;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace ubc.ok.ovilab.roadmap
{
    /// <summary>
    /// An object that can be placed in the scene on an ARPlane by the PlaceablesManager.
    /// Will register with and be parented to a PlaceablesGroup.
    /// </summary>
    [RequireComponent(typeof(BoundsControl))]
    public class PlaceableObject : MonoBehaviour
    {
        private string prefabIdentifier;

        /// <summary>
        /// Initialze the PlaceableObject.
        /// </summary>
        public void Init(string prefabIdentifier)
        {
            this.prefabIdentifier = prefabIdentifier;
        }

        public void SetLocalPose(Pose pose, Vector3 scale)
        {
            SetLocalPose(pose.position, pose.rotation, scale);
        }

        public void SetLocalPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        internal PlaceableObjectData GetPlaceableObjectData()
        {
            return new PlaceableObjectData(prefabIdentifier, transform.localPosition, transform.localRotation, transform.localScale);
        }

        /// <summary>
        /// Enable/disable manipulation related functions of a placeable object.
        /// </summary>
        public void SetObjectManipulationEnabled(bool enabled)
        {
            GetComponent<BoundsControl>().enabled = enabled;
            GetComponent<ObjectManipulator>().enabled = enabled;
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform t = transform.GetChild(i);
                if (t.name.Contains(RoadmapApplicationConfig.boundingBoxWithHandlesName))
                {
                    t.gameObject.SetActive(enabled);
                }
            }
        }

        #region Factory methods
        public static HandleType handleTypeToUse = HandleType.Rotation | HandleType.Scale | HandleType.Translation;

        public static PlaceableObject SetupPlaceableObject(string identifier, Transform parent)
        {
            GameObject placeableGameObject = PlaceablesManager.Instance.applicationConfig.GetPleaceableGameObject(identifier, parent);
            AddBoundsToAllChildren(placeableGameObject.transform.GetChild(0).gameObject);

            SetupMRTKControls(placeableGameObject);

            PlaceableObject placeableObject = placeableGameObject.GetComponent<PlaceableObject>();
            if (placeableObject == null)
            {
                placeableObject = placeableGameObject.AddComponent<PlaceableObject>();
            }

            placeableObject.Init(identifier);

            return placeableObject;
        }

        // From https://gamedev.stackexchange.com/questions/129116/how-to-create-a-box-collider-that-surrounds-an-object-and-its-children
        /// <summary>
        /// Creates a collider that encapsulates all the objects in `newObject`
        /// </summary>
        private static void AddBoundsToAllChildren(GameObject newObject)
        {
            Collider collider;
            collider = newObject.GetComponent<Collider>();
            if (collider != null)
            {
                return;
            }
            else
            {
                collider = newObject.AddComponent<BoxCollider>();   
            }
            BoxCollider boxCol = (BoxCollider)collider;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer thisRenderer = newObject.transform.GetComponent<Renderer>();
            if (thisRenderer != null)
            {
                bounds.Encapsulate(thisRenderer.bounds);
                boxCol.center = bounds.center - newObject.transform.position;
                boxCol.size = bounds.size;
            }

            var allDescendants = newObject.GetComponentsInChildren<Transform>();
            foreach (Transform desc in allDescendants)
            {
                Renderer childRenderer = desc.GetComponent<Renderer>();
                if (childRenderer != null)
                {
                    bounds.Encapsulate(childRenderer.bounds);
                }
                boxCol.center = bounds.center - newObject.transform.position;
                boxCol.size = bounds.size;
            }
        }

        // TODO: SetupMRTKControls
        private static void SetupMRTKControls(GameObject boundsControlObj)
        {
            boundsControlObj.AddComponent<ConstraintManager>().AutoConstraintSelection = true;
            MinMaxScaleConstraint minMaxScaleConstraint = boundsControlObj.AddComponent<MinMaxScaleConstraint>();
            minMaxScaleConstraint.ProximityType = ManipulationProximityFlags.Near | ManipulationProximityFlags.Far;
            minMaxScaleConstraint.HandType = ManipulationHandFlags.OneHanded | ManipulationHandFlags.TwoHanded;
            minMaxScaleConstraint.RelativeToInitialState = true;

            boundsControlObj.AddComponent<UGUIInputAdapterDraggable>();

            ObjectManipulator objectManipulator = boundsControlObj.AddComponent<ObjectManipulator>();
            objectManipulator.selectMode = InteractableSelectMode.Multiple;

            BoundsControl boundsControl = boundsControlObj.AddComponent<BoundsControl>();
            boundsControl.BoundsVisualsPrefab = RoadmapApplicationConfig.boundingBoxWithHandlesPrefab;
            boundsControl.BoundsCalculationMethod = BoundsCalculator.BoundsCalculationMethod.RendererOverCollider;
            boundsControl.HandlesActive = true;
            boundsControl.EnabledHandles = handleTypeToUse;
        }
        #endregion
    }
}
