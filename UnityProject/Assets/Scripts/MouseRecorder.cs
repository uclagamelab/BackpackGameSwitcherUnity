using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseRecorder : MonoBehaviour {

    
    public float timer = -1;
    MouseStartUpOptions _toPopulate;
    List<AutoMouseEvent> _events = new List<AutoMouseEvent>();

    public static MouseRecorder instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        instance = this;
    }

    void Start () {
        BackgroundKeyboardInput.Events.onBackgroundMouseClick += OnBGMouseClick;
        BackgroundKeyboardInput.Events.onBackgroundKeyCombo += OnQuitComboReceived;

    }

    public void StartRecording(MouseStartUpOptions toPopulate)
    {
        _toPopulate = toPopulate;
        timer = 0;
        _events.Clear();
    }

    public void StopRecording()
    {
        timer = -1;
        _toPopulate.startUpRoutine = _events.ToArray();
    }
    bool recording => timer >= 0;

    [ContextMenu("Reperform")]
    public void RePerform()
    {
        _toPopulate.Perform();
    }

    void Update ()
    {
        if (!recording)
        {
            #if UNITY_EDITOR
            if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.E))
            {
                StartRecording(null);
            }
            #endif
        }
        else
        {
            #if UNITY_EDITOR
            if (Input.GetKey(KeyCode.R) && Input.GetKeyDown(KeyCode.E))
            {
                StopRecording();
            }
            #endif



            timer += Time.deltaTime;
        }
	}

    void OnBGMouseClick()
    {
        if (recording)
        {
            _events.Add(new AutoMouseEvent(BackgroundKeyboardInput.GetCursorPosition(), AutoMouseEvent.ClickEventType.leftClick, timer));
            timer = 0;
        }
    }

    void OnQuitComboReceived()
    {
        if (recording)
        {
            StopRecording();
        }
    }
}
