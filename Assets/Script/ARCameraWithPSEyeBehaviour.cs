using UnityEngine;
using System;
using System.Collections;
using jp.nyatla.nyartoolkit.cs.markersystem;
using jp.nyatla.nyartoolkit.cs.core;
using NyARUnityUtils;
using System.IO;

public class ARCameraWithPSEyeBehaviour : MonoBehaviour
{
	// NyARToolKit
	private NyARUnityMarkerSystem markerSystemLeft_;
	private NyARUnityMarkerSystem markerSystemRight_;
	private NyARUnityPSEye psEyeLeft_;
	private NyARUnityPSEye psEyeRight_;
	private int markerId_;
	public string MarkerName = "MarkerHiro"; 
	public GameObject MarkerObjectLeft  = null;
	public GameObject MarkerObjectRight = null;
	private bool isDetectLeft_  = false;
	private bool isDetectRight_ = false;
	
	// PS Eye
	public int FrameRate = 60;
	
	// Camera Object
	public GameObject BackgroundLeft  = null;
	public GameObject BackgroundRight = null;
	public Camera CameraLeft  = null;
	public Camera CameraRight = null;
	
	// Background
	public int LeftLayer  = 30;
	public int RightLayer = 31;
	
	// Somooth animation parameter
	private static float INELASTIC = 0.5f;
	public GameObject FilterPositionObjectLeft  = null;
	public GameObject FilterPositionObjectRight = null; 
	private Transform transformLeft_  = null;
	private Transform transformRight_ = null; 
	
	void Awake()
	{
		// Check PS Eye number
		int psEyeCount = PSEyeTexture.CLEyeGetCameraCount();
		switch (psEyeCount) {
			case 0:
				Debug.LogError("No PS Eye found");
				return;
			case 1:
				Debug.LogError("Only one PS Eye found");
				return;
		}
		
		// Make PS Eye texture
		var leftPsEyeTexture = new PSEyeTexture(0, FrameRate, true);
		psEyeLeft_ = new NyARUnityPSEye(leftPsEyeTexture);
		var rightPsEyeTexture = new PSEyeTexture(1, FrameRate);
		psEyeRight_ = new NyARUnityPSEye(rightPsEyeTexture);
		
		// Marker system
		var configLeft     = new NyARMarkerSystemConfig(leftPsEyeTexture.Width, leftPsEyeTexture.Height);
		markerSystemLeft_  = new NyARUnityMarkerSystem(configLeft);
		var configRight    = new NyARMarkerSystemConfig(rightPsEyeTexture.Width, rightPsEyeTexture.Height);
		markerSystemRight_ = new NyARUnityMarkerSystem(configRight);
		
		// Load marker from texture
		var markerTexture = (Texture2D)(Resources.Load(MarkerName, typeof(Texture2D)));
		markerId_ = markerSystemLeft_.addARMarker(markerTexture, 16, 25, 80);
		markerId_ = markerSystemRight_.addARMarker(markerTexture, 16, 25, 80);
		
		// Marker layer
		MarkerObjectLeft.layer = LeftLayer;
		for (int i = 0; i < MarkerObjectLeft.transform.GetChildCount(); ++i) {
			MarkerObjectLeft.transform.GetChild(i).gameObject.layer = LeftLayer;
		}
		MarkerObjectRight.layer = RightLayer;
		for (int i = 0; i < MarkerObjectRight.transform.GetChildCount(); ++i) {
			MarkerObjectRight.transform.GetChild(i).gameObject.layer = RightLayer;
		}

		// Set camera background 
		// - Left
		BackgroundLeft.renderer.material.mainTexture = leftPsEyeTexture.Texture;
		BackgroundLeft.layer = LeftLayer;
		CameraLeft.cullingMask &= ~(1 << RightLayer);
		markerSystemLeft_.setARBackgroundTransform(BackgroundLeft.transform);
		markerSystemLeft_.setARCameraProjection(CameraLeft);
		// - Right
		BackgroundRight = GameObject.Find("BackgroundRight");
		BackgroundRight.renderer.material.mainTexture = rightPsEyeTexture.Texture;
		BackgroundRight.layer = RightLayer;
		CameraRight.cullingMask &= ~(1 << LeftLayer);
		markerSystemRight_.setARBackgroundTransform(BackgroundRight.transform);
		markerSystemRight_.setARCameraProjection(CameraRight);
		
		// Set transforms for animation
		transformLeft_  = FilterPositionObjectLeft.transform;
		transformRight_ = FilterPositionObjectRight.transform;
		
		// Rotate based on PS Eyes' physical direction
		// GameObject.Find("ARWorld").transform.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, -90.0f));
	}
	
	void Start()
	{
		psEyeLeft_.start();
		psEyeRight_.start();
	}
	
	void Update()
	{
		psEyeLeft_.update();
		psEyeRight_.update();
		markerSystemLeft_.update(psEyeLeft_);
		markerSystemRight_.update(psEyeRight_);
		
		// Left camera marker
		if (markerSystemLeft_.isExistMarker(markerId_)) {
			onFindLeftMaker();
		} else {
			if (isDetectLeft_) {
				onLostLeftMarker();
			}
		}
		
		// Right camera marker
		if (markerSystemRight_.isExistMarker(markerId_)) {
			onFindRightMaker();
		} else {
			if (isDetectRight_) {
				onLostRightMarker();
			}
		}
	}
	
	void onFindLeftMaker()
	{
		markerSystemLeft_.setMarkerTransform(markerId_, transformLeft_);
		transformLeft_.Rotate(new Vector3(0, 180, 180));
		if (isDetectLeft_) {
			MarkerObjectLeft.transform.localPosition = 
				Vector3.Slerp(MarkerObjectLeft.transform.localPosition, transformLeft_.localPosition, INELASTIC);
			MarkerObjectLeft.transform.localRotation = 
				Quaternion.Slerp(MarkerObjectLeft.transform.localRotation, transformLeft_.localRotation, INELASTIC);
			MarkerObjectLeft.transform.localScale = 
				Vector3.Slerp(MarkerObjectLeft.transform.localScale, transformLeft_.localScale, INELASTIC);
		} else {
			MarkerObjectLeft.transform.localPosition = transformLeft_.localPosition;
			MarkerObjectLeft.transform.localRotation = transformLeft_.localRotation;
			MarkerObjectLeft.transform.localScale    = transformLeft_.localScale;
		}
		isDetectLeft_ = true;
	}
	
	void onFindRightMaker()
	{
		markerSystemRight_.setMarkerTransform(markerId_, transformRight_);
		transformRight_.Rotate(new Vector3(0, 180, 180));
		if (isDetectRight_) {
			MarkerObjectRight.transform.localPosition = 
				Vector3.Slerp(MarkerObjectRight.transform.localPosition, transformRight_.localPosition, INELASTIC);
			MarkerObjectRight.transform.localRotation = 
				Quaternion.Slerp(MarkerObjectRight.transform.localRotation, transformRight_.localRotation, INELASTIC);
			MarkerObjectRight.transform.localScale = 
				Vector3.Slerp(MarkerObjectRight.transform.localScale, transformRight_.localScale, INELASTIC);
		} else {
			MarkerObjectRight.transform.localPosition = transformRight_.localPosition;
			MarkerObjectRight.transform.localRotation = transformRight_.localRotation;
			MarkerObjectRight.transform.localScale    = transformRight_.localScale;
		}
		isDetectRight_ = true;
	}
	
	void onLostLeftMarker()
	{
		MarkerObjectLeft.transform.localPosition = new Vector3(0, 0, -100);
		isDetectLeft_ = false;
	}
	
	void onLostRightMarker()
	{
		MarkerObjectRight.transform.localPosition = new Vector3(0, 0, -100);
		isDetectRight_ = false;
	}
}
