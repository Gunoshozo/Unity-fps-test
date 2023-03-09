using TMPro;
using UnityEngine;

public class Test : MonoBehaviour {

    public TextMeshProUGUI debug;

    DialogueSystem ds;
    [SerializeField]
    public TextArchitect architect;
    string[] lines = new string[1] {
        "Long long long long long long long long long long long long test string to measure the print speed of a method and define whether it is fps dependent of not"
    };

    public TextMeshProUGUI fpsCounter;
    public float deltaTime;

    private float StartTime;
    private float EndTime;
    public int frameRate = 60;
    public TextArchitect.BuildMethod buildMethod;
    public int charsPerSecond;
    public int gradientLineSize;

    private void Awake() {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
    }

    void Start() {
        ds = DialogueSystem.instance;
        architect = new TextArchitect(ds.container.dialogueText);
        architect.buildMethod = buildMethod;
        architect.charsPerSecond = charsPerSecond;
        architect.gradientLineSize = gradientLineSize;
        //time mesearument block START
        //unity action handler for time mesearuments
        architect.action += Finish;
        //time mesearument block END
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            if (architect.isBuilding) {
                if (architect.hurryUp) {
                    architect.ForcComplete();
                } else {
                    architect.hurryUp = true;
                }
            } else {
                StartTime = Time.time;
                architect.Build(lines[0]);
            }
        }
        //else if (Input.GetKeyDown(KeyCode.A)) {
        //    StartTime = Time.time;
        //    architect.Append(lines[0]);
        //}

        //time mesearument block START
        //calculates c/s for typewriter for each frame
        if (architect.isBuilding && (architect.buildMethod == TextArchitect.BuildMethod.typewriter || architect.buildMethod == TextArchitect.BuildMethod.typewriterv2)) {
            float delta = Time.time - StartTime;
            float speed = architect.tmpro.maxVisibleCharacters / delta;
            debug.text = "Speed: " + speed.ToString() + " c/s";
            if(architect.buildMethod == TextArchitect.BuildMethod.typewriterv2)
                debug.text += "\nTheoretical speed: " + ( architect.charsPerSecond) + " c/s";
        }
        UpdateFPSCounter();
        //time mesearument block END
    }

    //time mesearument block START
    //unity action handler for time mesearuments
    void Finish() {
        Debug.Log("End");
        EndTime = Time.time;
        float delta = EndTime - StartTime;
        float speed = architect.tmpro.textInfo.characterCount/delta;
        debug.text = "Speed: " + architect.tmpro.textInfo.characterCount/delta + " c/s";
        debug.text += "\nTotal time: " + delta.ToString();
        debug.text += "\nTotal chars: " + architect.tmpro.textInfo.characterCount;
    }
    //time mesearument block END

    private void UpdateFPSCounter() {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsCounter.text = "FPS: " + Mathf.Ceil(fps).ToString();
    }
}
