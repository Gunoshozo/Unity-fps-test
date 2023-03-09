using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using Unity.VisualScripting;

[System.Serializable]
public class TextArchitect {

    public TextMeshProUGUI tmpro;

    public string currentText => tmpro.text;
    public string targetText { get; private set; } = "";
    public string preText { get; private set; } = "";

    //time mesearument block START
    //unity action for time mesearuments
    public UnityAction action;
    //time mesearument block END

    public string fullTargetText => preText + targetText;

    public enum BuildMethod { instant, typewriter, fade, typewriterv2, fadev2 }
    public BuildMethod buildMethod = BuildMethod.typewriter;

    public Color textColor { get { return tmpro.color; } set { tmpro.color = value; } }

    public bool hurryUp = false;
    public int preTextLength = 0;

    //v1 related vars
    public float speed { get { return baseSpeed * speedMultiplier; } set { speedMultiplier = value; } }
    [SerializeField]
    private const float baseSpeed = 1;
    [SerializeField]
    private float speedMultiplier = 1;
    public int charactersPerCycle { get { return ((speed <= 2f) ? characterMultiplier : (speed <= 2.5f ? characterMultiplier * 2 : characterMultiplier * 3)); } }
    [SerializeField]
    public int characterMultiplier = 1;

    //v2 related vars

    public int charsPerSecond = 20;

    public int gradientLineSize = 20;
    private float timePerCharacters { get { return 1f / (charsPerSecond); } set { charsPerSecond = Mathf.RoundToInt(1f / value); } }


    public TextArchitect(TextMeshProUGUI tmp) {
        this.tmpro = tmp;
    }

    public Coroutine Build(string text) {
        preText = "";
        targetText = text;

        Stop();

        buildProcess = this.tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    public Coroutine Append(string text) {
        preText = tmpro.text;
        targetText = text;

        Stop();

        buildProcess = this.tmpro.StartCoroutine(Building());
        return buildProcess;
    }

    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;


    public void Stop() {
        if (!isBuilding) {
            return;
        }

        tmpro.StopCoroutine(buildProcess);
    }

    private void OnComplete() {
        buildProcess = null;
        hurryUp = false;
    }

    public void ForcComplete() {
        switch (buildMethod) {
            case BuildMethod.typewriter: {
                    tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                    break;
                }
            case BuildMethod.fade: {
                    tmpro.ForceMeshUpdate();
                    break;
                }
        }
        Stop();
        OnComplete();
    }

    public IEnumerator Building() {
        Prepare();
        switch (buildMethod) {
            case BuildMethod.typewriterv2: {
                    yield return BuildTypewriterV2();
                    break;
                }
            case BuildMethod.typewriter: {
                    yield return BuildTypewriter();
                    break;
                }
            case BuildMethod.fade: {
                    yield return BuildFade();
                    break;
                }
            case BuildMethod.fadev2: {
                    yield return BuildFadeV2();
                    break;
                }
        }
        OnComplete();
    }

    private IEnumerator BuildTypewriter() {
        while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount) {
            tmpro.maxVisibleCharacters += hurryUp ? charactersPerCycle * 5 : charactersPerCycle;

            yield return new WaitForSeconds(0.015f / speed);
        }
        //time mesearument block START
        //unity action invocation for time mesearuments
        action.Invoke();
        //time mesearument block END
    }

    private IEnumerator BuildTypewriterV2() {
        float timer = 0;
        while (tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount) {
            timer -= Time.deltaTime;
            float timeAmountPerChar = hurryUp ? timePerCharacters / 5 : timePerCharacters;
            int characters = Mathf.RoundToInt(Mathf.Abs(timer / timeAmountPerChar));
            timer += characters * timeAmountPerChar;
            tmpro.maxVisibleCharacters = Mathf.Clamp(tmpro.maxVisibleCharacters + characters, 0, tmpro.textInfo.characterCount);
            yield return new WaitForEndOfFrame();
        }
        //time mesearument block START
        //unity action invocation for time mesearuments
        action.Invoke();
        //time mesearument block END
    }


    private IEnumerator BuildFadeV2() {
        //time mesearument block START
        //time mesearument auxiliary variable   
        bool notified = false;
        //time mesearument block END

        TMP_TextInfo textInfo = tmpro.textInfo;

        int minRange = preTextLength;
        int initMinRange = minRange;

        int amountShifted = 0;

        float timer = 0;

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];
        int MAX_ALPHA = 255;


        while (true) {
            timer -= Time.deltaTime;
            float timeAmountPerChar = hurryUp ? timePerCharacters / 5f : timePerCharacters;
            int characters = Mathf.RoundToInt(Mathf.Abs(timer / timeAmountPerChar));
            timer += characters * timeAmountPerChar;

            if (characters > 0) {
                amountShifted += characters;
                for (int i = minRange; i < textInfo.characterCount; i++) {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) {
                        //time mesearument block START
                        //needed to easier detection of a case when last char is invisible and it shoul start appearing
                        //generally not required
                        alphas[i] = 255f;
                        //time mesearument block END
                        continue;
                    }
                    int localIndex = i - initMinRange;
                    float amount = (-localIndex + amountShifted) / (float)gradientLineSize;

                    if (amount >= 1) {
                        minRange = i + 1;
                    } else if (amount <= 0) {
                        break;
                    }

                    alphas[i] = Mathf.Lerp(0, MAX_ALPHA, amount);


                    for (int v = 0; v < 4; v++) {
                        vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];
                    }
                }
                tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                //time mesearument block START
                if (!notified && alphas[textInfo.characterCount - 1] > 0) {
                    notified = true;
                    action.Invoke();
                }
                //time mesearument block END
            }

            if (minRange == textInfo.characterCount - 1 && (alphas[minRange - 1] >= 255 || !textInfo.characterInfo[minRange - 1].isVisible)) {
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator BuildFade() {
        bool notified = false;
        int minRange = preTextLength;
        int maxRange = minRange + 1;

        byte alphaThreshold = 15;

        TMP_TextInfo textInfo = tmpro.textInfo;
        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];
        int MAX_ALPHA = 255;

        while (true) {
            float fadeSpeed = ((hurryUp ? charactersPerCycle * 5 : charactersPerCycle) * speed) * 4f;

            for (int i = minRange; i < maxRange; i++) {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                if (!charInfo.isVisible) {
                    continue;
                }

                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                alphas[i] = Mathf.MoveTowards(alphas[i], MAX_ALPHA, fadeSpeed);

                for (int v = 0; v < 4; v++) {
                    vertexColors[charInfo.vertexIndex + v].a = (byte)alphas[i];
                }

                if (alphas[i] >= MAX_ALPHA) {
                    minRange++;
                }
            }
            if(!notified && maxRange == textInfo.characterCount) {
                notified = true;
                action.Invoke();
            }

            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

            bool lastCharacterIsINvisible = !textInfo.characterInfo[maxRange - 1].isVisible;
            if (alphas[maxRange - 1] > alphaThreshold || lastCharacterIsINvisible) {
                if (maxRange < textInfo.characterCount) {
                    maxRange++;
                } else if (alphas[maxRange - 1] >= 255 || lastCharacterIsINvisible) {
                    break;
                }
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void Prepare() {
        switch (buildMethod) {
            case BuildMethod.instant: {
                    PrepareInstant();
                    break;
                }
            case BuildMethod.typewriterv2:
            //falls through
            case BuildMethod.typewriter: {
                    PrepareTypewriter();
                    break;
                }
            case BuildMethod.fadev2:
            //falls through
            case BuildMethod.fade: {
                    PrepareFade();
                    break;
                }
        }
    }

    private void PrepareInstant() {
        tmpro.color = tmpro.color;
        tmpro.text = fullTargetText;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void PrepareTypewriter() {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = preText;
        if (preText != "") {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }

        tmpro.text += targetText;
        tmpro.ForceMeshUpdate();
    }

    private void PrepareFade() {
        tmpro.text = preText;
        if (preText != "") {
            tmpro.ForceMeshUpdate();
            preTextLength = tmpro.textInfo.characterCount;
        } else {
            preTextLength = 0;
        }
        tmpro.text += targetText;
        tmpro.maxVisibleCharacters = int.MaxValue;
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;
        Color colorVisible = new Color(textColor.r, textColor.g, textColor.b, 1f);
        Color colorHidden = new Color(textColor.r, textColor.g, textColor.b, 0f);

        Color32[] vertextColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        for (int i = 0; i < textInfo.characterCount; i++) {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible)
                continue;

            if (i < preTextLength) {
                for (int v = 0; v < 4; v++) {
                    vertextColors[charInfo.vertexIndex + v] = colorVisible;
                }
            } else {
                for (int v = 0; v < 4; v++) {
                    vertextColors[charInfo.vertexIndex + v] = colorHidden;
                }
            }
        }
        tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }
}
