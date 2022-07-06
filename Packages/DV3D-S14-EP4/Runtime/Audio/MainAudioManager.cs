#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;
// https://forum.unity.com/threads/solved-but-unhappy-scriptableobject-awake-never-execute.488468/#post-3188178

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewSoundEffect", menuName = "Audio/New Sound Effect")]
    public class MainAudioManager : ScriptableObject
    {
        #region config

        private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;
        public AudioClip[] clips;

        public Vector2 volume = new Vector2(0.5f, 0.5f);

        //Pitch / Semitones
        public bool useSemitones;

        public Vector2Int semitones = new Vector2Int(0, 0);

        public Vector2 pitch = new Vector2(1, 1);

        private SoundClipPlayOrder playOrder;

        private int playIndex = 0;

        #endregion

        #region PreviewCode
        private AudioSource previewer;


        void OnEnable()
        {
            MainManager.OnGameInitialized += init;
            InitInEditorMode();
        }
        private void init()
        {
            if (Application.isPlaying)
            {
                // I don't want to chek if it already exist but Unity gives me no other way to do it.
                var go = new GameObject("AudioPreview");
                previewer = go.AddComponent<AudioSource>();
                // play the audio
                PlayPreview();
                // after 3 seconsds, disable the gameobject
                Destroy(go, 3);
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
            // Debug.Log("isEditor");
#endif
        }

        void OnDisable()
        {
            MainManager.OnGameInitialized -= init;
            if (previewer)
            {
                DestroyImmediate(previewer.gameObject);
                return;
            }
            Debug.Log("source is null on Destroy");
        }
        public void PlayPreview()
        {
            if (!previewer)
            {
                init();
                Debug.Log("source was null on play, it's better to create a new one");
            }
            Play(previewer);
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
            if (useSemitones)
            {
                pitch.x = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.x);
                pitch.y = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.y);
            }
            else
            {
                semitones.x = Mathf.RoundToInt(Mathf.Log10(pitch.x) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
                semitones.y = Mathf.RoundToInt(Mathf.Log10(pitch.y) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
            }
        }

        private AudioClip GetAudioClip()
        {
            // get current clip
            var clip = clips[playIndex >= clips.Length ? 0 : playIndex];

            // find next clip
            switch (playOrder)
            {
                case SoundClipPlayOrder.in_order:
                    playIndex = (playIndex + 1) % clips.Length;
                    break;
                case SoundClipPlayOrder.random:
                    playIndex = Random.Range(0, clips.Length);
                    break;
                case SoundClipPlayOrder.reverse:
                    playIndex = (playIndex + clips.Length - 1) % clips.Length;
                    break;
            }

            // return clip
            return clip;
        }

        public AudioSource Play(AudioSource audioSourceParam = null)
        {
            if (clips.Length == 0)
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
            source.volume = Random.Range(volume.x, volume.y);
            source.pitch = useSemitones
                ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(semitones.x, semitones.y))
                : Random.Range(pitch.x, pitch.y);

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

        enum SoundClipPlayOrder
        {
            random,
            in_order,
            reverse
        }
    }
}
