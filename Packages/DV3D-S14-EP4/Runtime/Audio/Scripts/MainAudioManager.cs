#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using System;
using UnityEngine.Serialization;
// https://forum.unity.com/threads/solved-but-unhappy-scriptableobject-awake-never-execute.488468/#post-3188178

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MainAudioManager", menuName = "Audio/MainAudioManager")]
    public class MainAudioManager : ScriptableObject
    {
        #region config

        private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;

        public Vector2 volume = new Vector2(0.5f, 0.5f);

        public List<InputOutputData> Background = new List<InputOutputData>();
        public List<InputOutputData> Pickups = new List<InputOutputData>();
        public List<InputOutputData> Fails = new List<InputOutputData>();
        public List<InputOutputData> EpicFails = new List<InputOutputData>();
        private Dictionary<AudioMood, List<InputOutputData>> audioMoods = new Dictionary<AudioMood, List<InputOutputData>>();
        #endregion

        #region PreviewCode
        private AudioSource previewer;


        public static Action<int, AudioMood> OnScoreChanged;

        void OnEnable()
        {
            OnScoreChanged += ScoreChanged;
            MainManager.OnGameInitialized += init;
            InitInEditorMode();
        }
        void OnDisable()
        {
            OnScoreChanged += ScoreChanged;
            MainManager.OnGameInitialized -= init;
            if (previewer)
            {
                DestroyImmediate(previewer.gameObject);
                return;
            }
            Debug.Log("source is null on Destroy");
        }

        void ScoreChanged(int score, AudioMood mood)
        {
            var x = audioMoods[mood].Find(x => x.Other.Threshold > score);

        }

        private void init()
        {
            if (Application.isPlaying)
            {
                // I don't want to chek if it already exist but Unity gives me no other way to do it.
                var go = new GameObject("AudioPreview");
                previewer = go.AddComponent<AudioSource>();
                PlayPreview();
                return;
            }
            InitInEditorMode();
        }

        private void InitInEditorMode()
        {
#if UNITY_EDITOR
            previewer = EditorUtility
                .CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave,
                    typeof(AudioSource))
                .GetComponent<AudioSource>();
#endif
        }

        public void PlayPreview()
        {
            if (!previewer)
            {
                init();
                Debug.Log("source was null on play, it's better to create a new one");
            }
            Play(previewer, Background[0].Other.Loop);
        }

        public void StopPreview()
        {
            if (previewer)
            {
                previewer.Stop();
            }
        }

        #endregion

        public void SyncPitchAndSemitones()
        {
            if (Background[0].Other.UseSemitones)
            {
                Background[0].Other.Pitch.x = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Background[0].Other.Semitones.x);
                Background[0].Other.Pitch.y = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Background[0].Other.Semitones.y);
            }
            else
            {
                Background[0].Other.Semitones.x = Mathf.RoundToInt(Mathf.Log10(Background[0].Other.Pitch.x) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
                Background[0].Other.Semitones.y = Mathf.RoundToInt(Mathf.Log10(Background[0].Other.Pitch.y) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
            }
        }

        private AudioClip GetAudioClip()
        {
            var playIndex = Background[0].Other.PlayIndex;
            var length = Background.Count;
            // get current clip
            var clip = Background[playIndex >= length ? 0 : playIndex];

            // find next clip
            switch (Background[0].Other.PlayOrder)
            {
                case SoundClipPlayOrder.in_order:
                    playIndex = (playIndex + 1) % length;
                    break;
                case SoundClipPlayOrder.random:
                    playIndex = Random.Range(0, length);
                    break;
                case SoundClipPlayOrder.reverse:
                    playIndex = (playIndex + length - 1) % length;
                    break;
            }
            Background[0].Other.PlayIndex = playIndex;

            // return clip
            return Background[0].Clips;
        }

        public AudioSource Play(AudioSource audioSourceParam = null, bool loop = true)
        {
            if (Background[0].Clips.length == 0)
            {
                Debug.Log($"Missing sound clips for {name}");
                return null;
            }

            var source = audioSourceParam;
            if (source == null)
            {
                var _obj = new GameObject("Sound", typeof(AudioSource));
                source = _obj.GetComponent<AudioSource>();
            }

            // set source config:
            source.clip = GetAudioClip();
            source.time = Background[0].Other.Persist ? Background[0].Other.Timestamp : 0;
            source.volume = Random.Range(volume.x, volume.y);
            source.pitch = Background[0].Other.UseSemitones
                ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(Background[0].Other.Semitones.x, Background[0].Other.Semitones.y))
                : Random.Range(Background[0].Other.Pitch.x, Background[0].Other.Pitch.y);
            source.loop = loop;

            source.Play();

            if (!source || !previewer)
            {
                Debug.Log("source is null");
            }
#if UNITY_EDITOR
            if (source != previewer)
            {
                DestroyImmediate(source.gameObject);
            }
#else
                Destroy(source.gameObject, source.clip.length / source.pitch);
#endif
            return source;
        }

    }
}

public enum SoundClipPlayOrder
{
    in_order,
    reverse,
    random
}
public enum AudioMood
{
    Background,
    Pickups,
    Fails,
    EpicFails,
    Automatic
}

[System.Serializable]
public class InputOutputData
{
    public string Id;
    public AudioClip Clips;
    public Other Other = new Other();
}
[System.Serializable]
public class Other
{
    public bool Persist = false;
    public float Timestamp = 0.0f;
    public bool Loop = true;
    //add inspector label
    [FormerlySerializedAs("Clips")]
    public int Threshold = 1;
    public bool UseSemitones;

    public Vector2Int Semitones = new Vector2Int(0, 0);

    public Vector2 Pitch = new Vector2(1, 1);

    public SoundClipPlayOrder PlayOrder;

    public int PlayIndex = 0;
}