using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using System.Text.RegularExpressions;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        public bool printLanguage = true;

        [Header("UI")] 
        public Button button;
        public Text buttonText;
        public Text outputText;
        public Text timeText;
        public Dropdown languageDropdown;
        public Toggle translateToggle;
        public Toggle vadToggle;
        public ScrollRect scroll;
        
        public GameObject cube;
        private Renderer rend;

        public Transform warrior;
        public float speed = 5f;

        public delegate void Mensaje(string command);
        public event Mensaje OnRegularExpression;

        private string _buffer;

        private void Awake()
        {
            whisper.OnNewSegment += OnNewSegment;
            whisper.OnProgress += OnProgressHandler;
            
            microphoneRecord.OnRecordStop += OnRecordStop;
            
            button.onClick.AddListener(OnButtonPressed);
            languageDropdown.value = languageDropdown.options
                .FindIndex(op => op.text == whisper.language);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            translateToggle.isOn = whisper.translateToEnglish;
            translateToggle.onValueChanged.AddListener(OnTranslateChanged);

            vadToggle.isOn = microphoneRecord.vadStop;
            vadToggle.onValueChanged.AddListener(OnVadChanged);
            if (cube != null) {
                rend = cube.GetComponent<Renderer>();
            }
        }

        private void OnVadChanged(bool vadStop)
        {
            microphoneRecord.vadStop = vadStop;
        }

        private void OnButtonPressed()
        {
            if (!microphoneRecord.IsRecording)
            {
                microphoneRecord.StartRecord();
                buttonText.text = "Stop";
            }
            else
            {
                microphoneRecord.StopRecord();
                buttonText.text = "Record";
            }
        }
        
        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            buttonText.text = "Record";
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();
            
            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText) 
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            var text = res.Result;
            if (IsTextMatchingRegex(text)) {
                rend.material.color = new Color(Random.value, Random.value, Random.value);
            } else if (IsTextMatchingWarriorRegex(text)) {
                OnRegularExpression("Avanza");
            }

            if (printLanguage)
                text += $"\n\nLanguage: {res.Language}";
            
            outputText.text = text;
            UiUtils.ScrollDown(scroll);
        }
        
        private bool IsTextMatchingRegex(string text)
        {
            string pattern = @"\b(cambia|Cambia|change|Change)\b";

            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private bool IsTextMatchingWarriorRegex(string text)
        {
            string pattern = @"\b(Adelante|Avanza|Muévete|Move|Forward|Ahead)\b";

            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private void OnLanguageChanged(int ind)
        {
            var opt = languageDropdown.options[ind];
            whisper.language = opt.text;
        }
        
        private void OnTranslateChanged(bool translate)
        {
            whisper.translateToEnglish = translate;
        }

        private void OnProgressHandler(int progress)
        {
            if (!timeText)
                return;
            timeText.text = $"Progress: {progress}%";
        }
        
        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text;
            outputText.text = _buffer + "...";
            UiUtils.ScrollDown(scroll);
        }
    }
}