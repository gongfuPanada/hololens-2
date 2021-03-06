﻿using UnityEngine;
using UnityEngine.VR.WSA.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerGazeControlled: MonoBehaviour
{
  public Helicopter m_helicopter;
  public ParticleEffectsManager m_particle_fx_manager;
  public Material   m_reticle_material;
  public PlayspaceManager m_playspace_manager;
  public LevelManager m_level_manager;
  public GameObject m_cursor1;
  public GameObject m_cursor2;
  public Bullet m_bullet_prefab;

  enum State
  {
    Scanning,
    Playing
  };

  private GestureRecognizer m_gesture_recognizer = null;
  private GameObject        m_gaze_target = null;
  private RaycastHit        m_hit;
  private State             m_state;

  private void OnTapEvent(InteractionSourceKind source, int tap_count, Ray head_ray)
  {
    switch (m_state)
    {
    case State.Scanning:
      m_playspace_manager.SetMakePlanesCompleteCallback(m_level_manager.GenerateLevel);
      SetState(State.Playing);
      break;
    case State.Playing:
      if (m_gaze_target == null)
      {
      }
      else if (m_gaze_target == m_helicopter.gameObject)
      {
      }
      else
      {
      }
      break;
    }
  }

  void SetState(State state)
  {
    m_state = state;
    switch (state)
    {
    case State.Scanning:
      m_playspace_manager.StartScanning();
      break;
    case State.Playing:
      m_playspace_manager.StopScanning();
      m_helicopter.SetControlMode(Helicopter.ControlMode.Player);
      break;
    }
  }

  void Start()
  {
    m_gesture_recognizer = new GestureRecognizer();
    m_gesture_recognizer.SetRecognizableGestures(GestureSettings.Tap);
    m_gesture_recognizer.TappedEvent += OnTapEvent;
    m_gesture_recognizer.StartCapturingGestures();
    SetState(State.Scanning);
  }

  private GameObject FindGazeTarget(out RaycastHit hit, float distance, int layer_mask)
  {
    if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, distance, layer_mask))
    {
      GameObject target = hit.collider.transform.parent.gameObject;
      m_gaze_target = target;
      if (target == null)
      {
        Debug.Log("ERROR: CANNOT IDENTIFY RAYCAST OBJECT");
        return null;
      }
      m_cursor1.transform.position = hit.point + hit.normal * 0.01f;
      m_cursor1.transform.forward = hit.normal;
      return target.activeSelf ? target : null;
    }
    return null;
  }

  void Update()
  {
    if (m_state == State.Playing)
    {
      int layer_mask = Layers.Instance.collidable_layers_mask;
      GameObject target = FindGazeTarget(out m_hit, 5.0f, layer_mask);
      if (target != null && target != m_helicopter.gameObject)
      {
        int target_layer_mask = 1 << target.layer;
        if ((target_layer_mask & layer_mask) != 0)
        {
          m_cursor2.transform.position = m_hit.point + m_hit.normal * 0.5f;
          m_cursor2.transform.forward = -transform.forward;
          //m_helicopter.FlyToPosition(m_hit.point + m_hit.normal * 0.5f);
        }
      }
      else if (target == null)
      {
        // Hit nothing -- move toward point on ray, 2m out
        //m_helicopter.FlyToPosition(transform.position + transform.forward * 2.0f);
      }
    }
    UnityEditorUpdate();
  }

#if UNITY_EDITOR
  private Vector3 m_euler = new Vector3();
#endif

  private void UnityEditorUpdate()
  {
#if UNITY_EDITOR
    // Simulate air tap
    if (Input.GetKeyDown(KeyCode.Return))
    {
      Ray head_ray = new Ray(transform.position, transform.forward);
      OnTapEvent(InteractionSourceKind.Hand, 1, head_ray);
    }

    // Rotate by maintaining Euler angles relative to world
    if (Input.GetKey("left"))
      m_euler.y -= 30 * Time.deltaTime;
    if (Input.GetKey("right"))
      m_euler.y += 30 * Time.deltaTime;
    if (Input.GetKey("up"))
      m_euler.x -= 30 * Time.deltaTime;
    if (Input.GetKey("down"))
      m_euler.x += 30 * Time.deltaTime;
    if (Input.GetKey("o"))
      m_euler.x = 0;
    transform.rotation = Quaternion.Euler(m_euler);

    // Motion relative to XZ plane
    float move_speed = 5.0F * Time.deltaTime;
    Vector3 forward = transform.forward;
    forward.y = 0.0F;
    forward.Normalize();  // even if we're looking up or down, will continue to move in XZ
    if (Input.GetKey("w"))
      transform.Translate(forward * move_speed, Space.World);
    if (Input.GetKey("s"))
      transform.Translate(-forward * move_speed, Space.World);
    if (Input.GetKey("a"))
      transform.Translate(-transform.right * move_speed, Space.World);
    if (Input.GetKey("d"))
      transform.Translate(transform.right * move_speed, Space.World);

    // Vertical motion
    if (Input.GetKey(KeyCode.KeypadMinus))  // up
      transform.Translate(new Vector3(0.0F, 1.0F, 0.0F) * move_speed, Space.World);
    if (Input.GetKey(KeyCode.KeypadPlus))   // down
      transform.Translate(new Vector3(0.0F, -1.0F, 0.0F) * move_speed, Space.World);
#endif
  }
}
