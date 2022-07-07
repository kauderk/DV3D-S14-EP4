#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;
using System;
using System.Linq;

// https://forum.unity.com/threads/solved-but-unhappy-scriptableobject-awake-never-execute.488468/#post-3188178

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "MainAudioManager", menuName = "Audio/MainAudioManager")]
    public class MainAudioManager : ScriptableObject
    {
        #region config
        private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;

        public AudioMixer MainAudioMixer;
        public Vector2 volume = new Vector2(0.5f, 0.5f);

        public List<InputOutputData> Background = new List<InputOutputData>();
        public List<InputOutputData> Pickups = new List<InputOutputData>();
        public List<InputOutputData> Fails = new List<InputOutputData>();
        public List<InputOutputData> EpicFails = new List<InputOutputData>();
        private Dictionary<AudioMood, List<InputOutputData>> audioMoods = new Dictionary<AudioMood, List<InputOutputData>>();
        #endregion

        #region PreviewCode

        public static Action<int, AudioMood> OnScoreChanged;

        void OnEnable()
        {
            MainManager.OnGameInitialized += OnLoad;
        }

        private void OnLoad()
        {
            string[] array = Enum.GetNames(typeof(AudioMood));
            for (int i = 0; i < array.Length; i++)
            {
                var name = array[i];
                var value = (AudioMood)i;

                // use reflection to get the List<InputOutputData> field value that matches the name
                var field = GetType().GetField(name);
                var list = (List<InputOutputData>)field.GetValue(this);
                audioMoods[value] = list;

                GameObject go = null;
                AudioSource sourcePreviewer = null;

#if UNITY_EDITOR
                go = EditorUtility
                    .CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave,
                        typeof(AudioSource));
                sourcePreviewer = go.GetComponent<AudioSource>();
#else
                // get the audio mixer group that matches the name
                go = new GameObject($"AudioPreview_{name}");
                sourcePreviewer = go.AddComponent<AudioSource>();
#endif
                // those under Master
                sourcePreviewer.outputAudioMixerGroup = MainAudioMixer.FindMatchingGroups(name)[0];

                audioMoods[value].ForEach(IO =>
                {
                    IO.Source = sourcePreviewer;
                    IO.GameObj = go;
                });
            }


            OnScoreChanged += ScoreChanged;
        }

        void OnDisable()
        {
            OnScoreChanged -= ScoreChanged;
            MainManager.OnGameInitialized -= OnLoad;
            foreach (var list in audioMoods.Values)
                foreach (var IO in list)
                {
                    if (IO.GameObj)
                        DestroyImmediate(IO.GameObj);
                    else
                        Debug.Log("Audio Source is null on Destroy");
                }
        }

        void ScoreChanged(int score, AudioMood mood)
        {
            // get the value of the mood form the dictionary
            var list = GetIO_AudioList(mood);
            if (list.Count == 0)
                return;
            // reverse the list
            // list.Reverse();
            // find the first one that is greater than the score
            var IO_Audio = list.Aggregate(list[0],
                 (acc, crr) =>
                 {
                     if (crr.Other.Threshold >= score)
                         return crr; // we have a bigger one, return it
                     return acc; // bad luck, keep going
                 });
            if (IO_Audio == null)
            {
                Debug.Log("No Audio found for score " + score);
                return;
            }
            // BE Carefull, give it a key, else it will span the hell out of it
            Play(IO_Audio);
        }

        private List<InputOutputData> GetIO_AudioList(AudioMood mood)
        {
            var list = audioMoods.TryGetValue(mood, out List<InputOutputData> value) ? value : null;
            if (list == null)
            {
                Debug.Log("No Audio list found for mood " + mood);
                return null;
            }
            return list;
        }

        private InputOutputData GetIO_Audio(AudioMood mood, string id)
        {
            var list = GetIO_AudioList(mood);
            if (list.Count == 0)
                return null;
            return list.Find(IO => IO.Id == id);
        }

        public void PlayPreview()
        {
            var IO = GetIO_Audio(AudioMood.Background, "base");
            if (!IO.Clip)
            {
                OnLoad();
                Debug.Log("source was null on play, it's better to create a new one");
            }
            IO = GetIO_Audio(AudioMood.Background, "base");
            Play(IO);
        }

        public void StopPreview()
        {
            var IO = GetIO_Audio(AudioMood.Background, "base");
            if (IO.Source)
            {
                IO.Source.Stop();
            }
        }

        #endregion

        public void SyncPitchAndSemitones(InputOutputData IO_Audio)
        {
            var other = IO_Audio.Other;
            if (other.UseSemitones)
            {
                other.Pitch.x = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, other.Semitones.x);
                other.Pitch.y = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, other.Semitones.y);
            }
            else
            {
                other.Semitones.x = Mathf.RoundToInt(Mathf.Log10(other.Pitch.x) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
                other.Semitones.y = Mathf.RoundToInt(Mathf.Log10(other.Pitch.y) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
            }
        }

        private AudioClip GetAudioClip(InputOutputData IO_Audio)
        {
            var playIndex = IO_Audio.Other.PlayIndex;
            var length = Background.Count;
            // get current clip
            var clip = Background[playIndex >= length ? 0 : playIndex];

            // find next clip
            switch (IO_Audio.Other.PlayOrder)
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
            IO_Audio.Other.PlayIndex = playIndex;

            // return clip
            return IO_Audio.Clip;
        }

        public AudioSource Play(InputOutputData IO_Audio)
        {
            if (IO_Audio.Clip.length == 0)
            {
                Debug.Log($"Missing sound clips for {name}");
                return null;
            }

            var source = IO_Audio.Source;
            if (source == null)
            {
                var _obj = new GameObject("Sound", typeof(AudioSource));
                source = _obj.GetComponent<AudioSource>();
            }

            // set source config:
            var vol = IO_Audio.Other.Volume;
            source.clip = GetAudioClip(IO_Audio);
            source.time = IO_Audio.Other.Persist ? IO_Audio.Other.Timestamp : 0;
            source.volume = Random.Range(vol.x, vol.y);
            source.pitch = IO_Audio.Other.UseSemitones
                ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(IO_Audio.Other.Semitones.x, IO_Audio.Other.Semitones.y))
                : Random.Range(IO_Audio.Other.Pitch.x, IO_Audio.Other.Pitch.y);
            source.loop = IO_Audio.Other.Loop;

            source.Play();

            if (!source || !IO_Audio.Source)
            {
                Debug.Log("source is null");
            }
#if UNITY_EDITOR
            if (source != IO_Audio.Source)
            {
                Destroy(source.gameObject);
            }
#else
                DestroyImmediate(source.gameObject);
#endif
            return IO_Audio.Source = source;
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
    EpicFails
}

[System.Serializable]
public class InputOutputData
{
    public string Id;
    public AudioClip Clip;
    public AudioSource Source;
    public GameObject GameObj;
    public Other Other = new Other();
}
[System.Serializable]
public class Other
{
    public bool Persist = false;
    public float Timestamp = 0.0f;
    public bool Loop = true;
    //add inspector label
    public int Threshold = 1;
    public bool UseSemitones;

    public Vector2Int Semitones = new Vector2Int(0, 0);

    public Vector2 Pitch = new Vector2(1, 1);

    public SoundClipPlayOrder PlayOrder;

    public int PlayIndex = 0;

    public Vector2 Volume = new Vector2(0.5f, 0.5f);
}