using UnityEngine;
using System.Collections;

public class MikuAppearMotion : MonoBehaviour {
	public GameObject AnotherMiku = null;
	
	public Vector3 AppearPositionFrom = Vector3.zero;
	public Vector3 AppearPositionTo   = Vector3.zero;
	public Vector3 AppearScaleFrom    = Vector3.zero;
	public Vector3 AppearScaleTo      = Vector3.zero;
	
	public float   Velocity   = 0.02f;
	public float   Diff       = 30.0f;
	
	private bool endFlag_     = false;
	private bool startFlag_   = false;
	
	public void Appear() {
		if (startFlag_ == true) return;
		startFlag_ = true;
		
		// appear motion start
		transform.animation.CrossFade("Appear");
		AnotherMiku.GetComponent<MikuAppearMotion>().Appear();
	}
	
	void Awake() {
		if (AnotherMiku == null) {
			Debug.LogError("Another Miku is not set!");	
		}
		
		AppearPositionTo += transform.localPosition;
		transform.localPosition += AppearPositionFrom;
		
		AppearScaleTo += transform.localScale;
		transform.localScale += AppearScaleFrom;
	}
	
	void Update() {
		if (startFlag_ == false || endFlag_ == true) return;
		
		transform.localPosition = Vector3.Slerp(transform.localPosition, AppearPositionTo, Velocity);
		transform.localScale    = Vector3.Slerp(transform.localScale,    AppearScaleTo,    Velocity);
		float diff = (AppearPositionTo - transform.localPosition).magnitude;
		if (diff < Diff) {
			animation.CrossFade("Idle");
			endFlag_ = true;
		}
	}
}
