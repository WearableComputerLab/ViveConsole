using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceAnnotation : MonoBehaviour
{
    //[Header("UI Elements")]
    //public TMPro.TextMeshProUGUI statusText;
    //public TMPro.TextMeshProUGUI hypothesisText;
    //public TMPro.TextMeshProUGUI resultText;

    [Header("Configuration Options")]
    public ConfidenceLevel confidenceThreshold;

    DictationRecognizer dictationRecognizer;
    VoiceRecorder recorder;
    bool dictationStarted;
    bool waitingForRecognizer;
    List<DictationResult> transcription;

    KeywordListener keywordListener;

    struct DictationResult
    {
        public string text;
        public ConfidenceLevel confidence;

        public DictationResult(string t, ConfidenceLevel c)
        {
            text = t;
            confidence = c;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        InitialiseVoiceRecognition();
        recorder = GetComponent<VoiceRecorder>() ?? gameObject.AddComponent<VoiceRecorder>();
        keywordListener = FindObjectOfType<KeywordListener>();
        KeywordListener.keywordHeard.AddListener(HandleKeywordActions);
    }

    private void HandleKeywordActions(string keyword)
    {
        switch (keyword)
        {
            case "annotate":
                ToggleDictationPressed(true);
                break;
            default:
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dictationStarted)
        {
            //statusText.SetText(dictationRecognizer.Status.ToString());
        }
    }

    private void OnGUI()
    {
        var spinnerCharacters = @"\|/-";
        var spinnerFrame = spinnerCharacters[Time.frameCount % spinnerCharacters.Length];
        GUILayout.Label(waitingForRecognizer ? $"Processing {spinnerFrame}" : dictationStarted ? "Listening ..." : "Dictation disabled");
    }

    public async void ToggleDictationPressed(bool dictationEnabled)
    {
        await ToggleDictationPressedAsync(dictationEnabled);
    }

    async Task ToggleDictationPressedAsync(bool enableDictation)
    {
        if (dictationStarted == enableDictation)
            return;

        waitingForRecognizer = true;

        if (enableDictation)
        {
            keywordListener?.StopListening();
            await Task.Delay(100); // wait for phraserecognitionsystem to turn off
            dictationRecognizer.Start();
            recorder.StartRecording();
            dictationStarted = true;
        }
        else
        {
            dictationRecognizer.Stop();
            recorder.StopRecording();
            await Task.Delay(300);
            // Save recorded audio to disk
            recorder.SaveRecording();

            keywordListener?.StartListening();
            dictationStarted = false;
        }

        waitingForRecognizer = false;
    }

    void InitialiseVoiceRecognition()
    {
        dictationRecognizer = new DictationRecognizer(confidenceThreshold, DictationTopicConstraint.Dictation);

        dictationRecognizer.DictationComplete += DictationRecognizer_DictationComplete;
        dictationRecognizer.DictationError += DictationRecognizer_DictationError;
        dictationRecognizer.DictationHypothesis += DictationRecognizer_DictationHypothesis;
        dictationRecognizer.DictationResult += DictationRecognizer_DictationResult;

        transcription = new List<DictationResult>();
    }

    private void DictationRecognizer_DictationResult(string text, ConfidenceLevel confidence)
    {
        transcription.Add(new DictationResult(text, confidence));
        Debug.Log($"{text} [confidence={confidence}]");
        //resultText.SetText($"{text} [confidence={confidence}]");
        //statusText.SetText(dictationRecognizer.Status.ToString());
        dictationStarted = dictationRecognizer.Status == SpeechSystemStatus.Running;

        // TODO: Search for command to end dictation in text
        // TODO: Change this to OrdinalIgnoreCase string comparison?
        if (text.Contains("stop dictation"))
        {
            ToggleDictationPressed(false);
        }
    }

    private void DictationRecognizer_DictationHypothesis(string text)
    {
        //hypothesisText.SetText(text);
    }

    private void DictationRecognizer_DictationError(string error, int hresult)
    {
        dictationStarted = false;
        Debug.Log($"DictationRecognizer Error (0x{hresult:x}): {error}");
        //statusText.SetText($"Error ({hresult}): {error}");
    }

    private void DictationRecognizer_DictationComplete(DictationCompletionCause cause)
    {
        Debug.Log($"Dictation complete: {cause.ToString()}");
        string transcriptionResult = string.Empty;
        foreach (var tr in transcription)
        {
            transcriptionResult += tr.text + " ";
        }
        Debug.Log($"<color=red>Transcription result:</color>\n{transcriptionResult}");
        dictationStarted = false;
    }
}