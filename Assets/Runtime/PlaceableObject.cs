using System;
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
    [RequireComponent(typeof(ObjectManipulator))]
    public class PlaceableObject : MonoBehaviour
    {
        /// <summary>
        /// Event raised when a new placeable object is created.
        /// </summary>
        public static EventHandler<PlaceableObjectEventArgs> onPlaceableInstantiated;

        /// <summary>
        /// Event raised when the <see cref="PlaceableObject"/> is
        /// clicked while in modify mode.
        /// </summary>
        public EventHandler<PlaceableObjectEventArgs> onClickedCallback;

        private string prefabIdentifier;
        private string identifier;
        // TODO: Properly update lastUpdate
        private long lastUpdate;
        private PlaceablesGroup placeablesGroup;


        /// <summary>
        /// Initialze the PlaceableObject.
        /// </summary>
        internal void Init(string prefabIdentifier, string identifier, long lastUpdate, PlaceablesGroup placeablesGroup)
        {
            this.prefabIdentifier = prefabIdentifier;
            if (string.IsNullOrEmpty(identifier))
            {
                this.identifier = $"{prefabIdentifier} {System.Guid.NewGuid().ToString()}";
            }
            else
            {
                this.identifier = identifier;
            }
            this.lastUpdate = lastUpdate;
            this.placeablesGroup = placeablesGroup;
            placeablesGroup.AddPlaceable(this);
            ObjectManipulator objectManipulator = GetComponent<ObjectManipulator>();
            objectManipulator.OnClicked.AddListener(OnClickCallback);
            objectManipulator.selectExited.AddListener(UpdateLastUpdate);
            GetComponent<BoundsControl>().ManipulationEnded.AddListener(UpdateLastUpdate);
        }

        /// <summary>
        /// Set the local position, rotation and scale.
        /// </summary>
        public void SetLocalPose(Pose pose, Vector3 scale)
        {
            SetLocalPose(pose.position, pose.rotation, scale);
        }

        /// <summary>
        /// Set the local position, rotation and scale.
        /// </summary>
        public void SetLocalPose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            transform.localPosition = position;
            transform.localRotation = rotation;
            transform.localScale = scale;
        }

        /// <summary>
        /// Get PlaceableObjectDta of this PlaceableObject.
        /// </summary>
        internal PlaceableObjectData GetPlaceableObjectData()
        {
            return new PlaceableObjectData(prefabIdentifier, identifier, transform.localPosition, transform.localRotation, transform.localScale, lastUpdate);
        }

        /// <summary>
        /// Enable/disable manipulation related functions of a placeable object.
        /// When the modification is enabled, it will disable all
        /// other interactables associated with the object. Similarly,
        /// when this is disabled, they will be reenabled.
        /// </summary>
        internal void SetObjectManipulationEnabled(bool enabled)
        {
            // NOTE: Any registered XR Interactables will need to be
            // disabled before the objectmanipulator is enabled. and
            // vice verse.
            if (enabled)
            {
                SetObjectOtherManipulationEnabled(false);
                SetObjectLocalManipulationEnabled(true);
            }
            else
            {
                SetObjectLocalManipulationEnabled(false);
                SetObjectOtherManipulationEnabled(true);
            }
        }

        /// <summary>
        /// Enable/disable other manipulation related functions of a placeable object.
        /// </summary>
        internal void SetObjectOtherManipulationEnabled(bool enabled)
        {
            XRBaseInteractable[] otherInteractables = gameObject.GetComponentsInChildren<XRBaseInteractable>();
            foreach (XRBaseInteractable interactable in otherInteractables)
            {
                if (interactable.transform != this.transform)
                {
                    interactable.enabled = enabled;
                }
            }
        }

        /// <summary>
        /// Enable/disable manipulation related functions of a placeable object.
        /// </summary>
        private void SetObjectLocalManipulationEnabled(bool enabled)
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

        /// <summary>
        /// Wrapper for the onClickedCallback event.
        /// </summary>
        public void OnClickCallback()
        {
            onClickedCallback?.Invoke(this, new PlaceableObjectEventArgs(this, PlaceableObjectEvent.Modified));
        }

        /// <summary>
        /// Callback for the manipulation handles to set the lastUpdate
        /// </summary>
        private void UpdateLastUpdate(SelectExitEventArgs arg0)
        {
            lastUpdate = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// Deinit and destroy self.
        /// </summary>
        public void DeleteThySelf()
        {
            placeablesGroup.RemovePlaceable(this);
            if (PlaceablesManager.Instance.ActivePlaceableObject == this)
            {
                PlaceablesManager.Instance.ActivePlaceableObject = null;
            }
            Destroy(gameObject);
        }

        #region Factory methods
        private static HandleType handleTypeToUse = HandleType.Rotation | HandleType.Scale | HandleType.Translation;

        /// <summary>
        /// Instantiate and configure a placeable object.
        /// `identifier` and `lastUpdate` are used when populating application state from saved/streamed data.
        /// <param name="prefabIdentifier">The identifier of the prefa/model to instantiate</param>
        /// <param name="identifier">string to use to uniquely identify the placeable object. If null or empty string, in generate a unique identifier.</param>
        /// <param name="placeablesGroup">The PlaceablesGroup the new placeable object would belong to.</param>
        /// <param name="lastUpdate">The timestamp of when this object was last updated.</param>
        /// </summary>
        internal static PlaceableObject SetupPlaceableObject(string prefabIdentifier, string identifier, PlaceablesGroup placeablesGroup, long lastUpdate=-1)
        {
            GameObject placeableGameObject = RoadmapApplicationConfig.activeApplicationConfig.GetPleaceableGameObject(prefabIdentifier, placeablesGroup.transform);
            if (placeableGameObject == null)
            {
                return null;
            }

            AddBoundsToAllChildren(placeableGameObject.transform.GetChild(0).gameObject);

            SetupMRTKControls(placeableGameObject);

            PlaceableObject placeableObject = placeableGameObject.GetComponent<PlaceableObject>();
            if (placeableObject == null)
            {
                placeableObject = placeableGameObject.AddComponent<PlaceableObject>();
            }

            if (lastUpdate == -1)
            {
                lastUpdate = System.DateTime.Now.Ticks;
            }

            placeableObject.Init(prefabIdentifier, identifier, lastUpdate, placeablesGroup);
            onPlaceableInstantiated?.Invoke(null, new PlaceableObjectEventArgs(placeableObject, PlaceableObjectEvent.Created));

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

        /// <summary>
        /// Setup MRTK related compoenents.
        /// </summary>
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
            boundsControl.BoundsVisualsPrefab = RoadmapApplicationConfig.activeApplicationConfig.boundingBoxWithHandlesPrefab;
            boundsControl.BoundsCalculationMethod = BoundsCalculator.BoundsCalculationMethod.RendererOverCollider;
            boundsControl.HandlesActive = true;
            boundsControl.EnabledHandles = handleTypeToUse;
        }
        #endregion
    }

    /// <summary>
    /// Event data associated with <see cref="PlaceableObject"/>.
    /// </summary>
    public class PlaceableObjectEventArgs
    {
        public PlaceableObject placeableObject;
        public PlaceableObjectEvent placeableObjectEvent;

        public PlaceableObjectEventArgs(PlaceableObject placeableObject, PlaceableObjectEvent placeableObjectEvent)
        {
            this.placeableObject = placeableObject;
            this.placeableObjectEvent = placeableObjectEvent;
        }
    }

    /// <summary>
    /// Events associated with <see cref="PlaceableObject">
    /// </summary>
    public enum PlaceableObjectEvent
    {
        /// <summary>
        /// A new <see cref="PlaceableObject"/> was created.
        /// </summary>
        Created,

        /// <summary>
        /// The <see cref="PlaceableObject"/> was modified
        /// </summary>
        Modified
    }
}
