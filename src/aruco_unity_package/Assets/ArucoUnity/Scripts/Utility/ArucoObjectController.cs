﻿using ArucoUnity.Plugin.std;
using UnityEngine;
using System.Collections.Generic;

namespace ArucoUnity
{
  /// \addtogroup aruco_unity_package
  /// \{

  namespace Utility
  {
    public class ArucoObjectController : MonoBehaviour
    {
      // Editor fields

      // TODO: move to Tracker
      [SerializeField]
      [Tooltip("The default game object to place above the detected markers")]
      private GameObject defaultTrackedGameObject;

      // Properties

      // TODO: move to Tracker
      /// <summary>
      /// The default game object to place above the detected markers.
      /// </summary>
      public GameObject DefaultTrackedGameObject { get { return defaultTrackedGameObject; } set { defaultTrackedGameObject = value; } }

      // Variables

      protected HashSet<ArucoObject> arucoObjects;
      // TODO: move to Tracker
      protected Dictionary<int, Marker> trackedMarkers;
      protected Dictionary<int, GameObject> defaultTrackedMarkerObjects;

      protected ArucoCamera arucoCamera;
      private float markerSideLength;

      // MonoBehaviour methods

      /// <summary>
      /// Initialize the variables.
      /// </summary>
      protected void Awake()
      {
        trackedMarkers = new Dictionary<int, Marker>();
        defaultTrackedMarkerObjects = new Dictionary<int, GameObject>();
      }

      // Methods

      public void SetCamera(ArucoCamera arucoCamera)
      {
        this.arucoCamera = arucoCamera;
      }

      public void Add(ArucoObject arucoObject)
      {
        arucoObjects.Add(arucoObject);

        // TODO: fix

        // Return if already added
        /* foreach (var trackedMarker in trackedMarkers)
         {
           if (trackedMarker.Value == newTrackedMarker)
           {
             return;
           }
         }*/

        // Add the new tracked marker, rescale to the marker size and hide it
        /*trackedMarkers.Add(newTrackedMarker.Id, newTrackedMarker);
        UpdateLocalScales(MarkerSideLength, newTrackedMarker.gameObject.transform);
        newTrackedMarker.gameObject.SetActive(false);*/
      }

      public void Remove(ArucoObject arucoObject)
      {
        arucoObjects.Remove(arucoObject);
      }

      /// <summary>
      /// Hide all the marker objects.
      /// </summary>
      public void DeactivateMarkerObjects()
      {
        foreach (var markerObject in trackedMarkers)
        {
          markerObject.Value.gameObject.SetActive(false);
        }
        foreach (var markerObject in defaultTrackedMarkerObjects)
        {
          markerObject.Value.gameObject.SetActive(false);
        }
      }

      /// <summary>
      /// Place and orient the object to match the marker.
      /// </summary>
      /// <param name="ids">Vector of identifiers of the detected markers.</param>
      /// <param name="rvecs">Vector of rotation vectors of the detected markers.</param>
      /// <param name="tvecs">Vector of translation vectors of the detected markers.</param>
      public void UpdateTransforms(VectorInt ids, VectorVec3d rvecs, VectorVec3d tvecs)
      {
        for (uint i = 0; i < ids.Size(); i++)
        {
          int markerId = ids.At(i);
          GameObject markerObject = null;

          // Try to retrieve the associated tracked marker
          Marker trackedMarker;
          if (trackedMarkers.TryGetValue(markerId, out trackedMarker))
          {
            markerObject = trackedMarker.gameObject;
          }

          if (markerObject == null)
          {
            // Instantiate the default tracked game object for the current tracked marker
            if (!defaultTrackedMarkerObjects.TryGetValue(markerId, out markerObject))
            {
              // If not found, instantiate it
              markerObject = Instantiate(DefaultTrackedGameObject);
              markerObject.name = markerId.ToString();
              markerObject.transform.SetParent(this.transform);

              defaultTrackedMarkerObjects.Add(markerId, markerObject);
            }
          }

          // Place and orient the object to match the marker
          markerObject.transform.rotation = rvecs.At(i).ToRotation();
          markerObject.transform.position = tvecs.At(i).ToPosition();

          // Adjust the object position
          Vector3 imageCenterMarkerObject = new Vector3(0.5f, 0.5f, markerObject.transform.position.z);
          Vector3 opticalCenterMarkerObject = new Vector3(arucoCamera.CameraParameters.OpticalCenter.x, arucoCamera.CameraParameters.OpticalCenter.y, markerObject.transform.position.z);
          Vector3 opticalShift = arucoCamera.CameraImage.ViewportToWorldPoint(opticalCenterMarkerObject) - arucoCamera.CameraImage.ViewportToWorldPoint(imageCenterMarkerObject);

          Vector3 positionShift = opticalShift // Take account of the optical center not in the image center
            + markerObject.transform.up * markerObject.transform.localScale.y / 2; // Move up the object to coincide with the marker
          markerObject.transform.localPosition += positionShift;

          print(markerObject.name + " - imageCenter: " + imageCenterMarkerObject.ToString("F3") + "; opticalCenter: " + opticalCenterMarkerObject.ToString("F3")
            + "; positionShift: " + (markerObject.transform.rotation * opticalShift).ToString("F4"));

          markerObject.SetActive(true);
        }
      }
    }
  }

  /// \} aruco_unity_package
}
