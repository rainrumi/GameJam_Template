using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

/***************************************************************
* 
* 使い方
* 前提１,このクラスはサウンド全般の管理を行います。
* 
* １,クラスをオブジェにアタッチ
* ２,soundGroupsの要素を音のカテゴリの数だけ作成（例：カテゴリをBGMとSEに分ける場合は２）
* ３,soundDatasの要素をカテゴリに設定する音の数だけ作成
* ４,音量変更、再生、停止したい音に設定したstringの名前をメソッドで呼び出す
* 
* 追記.何故これを書くのかは書きませんが、同じコンポーネントはひとつのオブジェに複数個アタッチできます。
* 
* ************************************************************/

public enum SoundKind
{
    BGM,
    SE
}

namespace RaruLib
{
    public class Sound : MonoBehaviour
    {
        public static Sound instance;
        public bool dontDestroy;    // 本スクリプトの非破壊化

        private bool _finishedInit = false; // 初期化が終了した
        public bool finishedInit => _finishedInit;

        [SerializeField] SoundGroup[] soundGroups;          // 音のカテゴリ
        private Dictionary<string, SoundGroup> groupDict;

        [Serializable]
        public class SoundGroup
        {
            public string groupName;        // カテゴリの名称
            [Range(0, 1)] public float volume = 0.5f;     // カテゴリの音量
            public SoundData[] soundDatas;  // カテゴリに入れる音
            public Dictionary<string, AudioSource> soundDict;

            [Serializable]
            public class SoundData
            {
                public string soundName;        // 音の名称
                public AudioSource audioSource; // 音のソース
            }
        }

        private void Awake()
        {
            //Debug.Log(instance);
            if (instance == null)
            {
                instance = this;
                if (dontDestroy)
                {
                    DontDestroyOnLoad(this);
                }
            }
            else
            {
                Debug.Log($"重複した{instance}を削除します");
                Destroy(instance.gameObject);
            }

            InitSoundGroup();

            foreach (var data in soundGroups)   // 音量の初期化
            { ChangeVolume(data.groupName, data.volume); }

            _finishedInit = true;   // 
        }

        // 全ての辞書の初期化と挿入
        private void InitSoundGroup()
        {
            groupDict = new Dictionary<string, SoundGroup>();

            foreach (var data in soundGroups)
            {
                groupDict.Add(data.groupName, data);

                data.soundDict = new Dictionary<string, AudioSource>();
                foreach (var dataSet in data.soundDatas)
                {
                    data.soundDict.Add(dataSet.soundName, dataSet.audioSource);
                }
            }
        }

        /***************************************************************
        * 音量取得
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        public float GetVolume(string groupName)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return 0; }

            return groupDict[groupName].volume;
        }

        /***************************************************************
        * 音量変更
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        public void ChangeVolume(string groupName, float newVolume)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return; }

            groupDict[groupName].volume = newVolume;
            foreach (var dataSet in groupDict[groupName].soundDict)
            { dataSet.Value.volume = groupDict[groupName].volume; }
        }

        /***************************************************************
        * 再生
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        /// <param name="soundName">音の名称</param>
        public void Play(string groupName, string soundName)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return; }
            if (!groupDict[groupName].soundDict.ContainsKey(soundName))
            { Debug.Log($"{soundName}は存在しない音源名です"); return; }
            groupDict[groupName].soundDict[soundName].Play();
        }

        /***************************************************************
        * フェード再生
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        /// <param name="soundName">音の名称</param>
        public async UniTaskVoid Play(string groupName, string soundName, float duration)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return; }
            if (!groupDict[groupName].soundDict.ContainsKey(soundName))
            { Debug.Log($"{soundName}は存在しない音源名です"); return; }
            if (groupDict[groupName].soundDict[soundName].isPlaying)
            { Debug.Log($"既に再生中です"); return; }

            float cacheVolume = GetVolume(groupName);

            groupDict[groupName].soundDict[soundName].volume = 0;

            await UniTask.Yield();

            groupDict[groupName].soundDict[soundName].Play();
            groupDict[groupName].soundDict[soundName].DOFade(cacheVolume, duration);
        }

        /***************************************************************
        * 停止
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        /// <param name="soundName">音の名称</param>
        public void Stop(string groupName, string soundName)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return; }
            if (!groupDict[groupName].soundDict.ContainsKey(soundName))
            { Debug.Log($"{soundName}は存在しない音源名です"); return; }

            groupDict[groupName].soundDict[soundName].Stop();
        }

        /***************************************************************
        * フェード停止
        ***************************************************************/
        /// <param name="groupName">カテゴリ</param>
        /// <param name="soundName">音の名称</param>
        public async UniTaskVoid Stop(string groupName, string soundName, float duration)
        {
            if (!groupDict.ContainsKey(groupName))
            { Debug.Log($"{groupName}は存在しないカテゴリです"); return; }
            if (!groupDict[groupName].soundDict.ContainsKey(soundName))
            { Debug.Log($"{soundName}は存在しない音源名です"); return; }

            float cacheVolume = GetVolume(groupName);

            groupDict[groupName].soundDict[soundName].DOFade(0, duration);

            await UniTask.WaitForSeconds(duration);

            groupDict[groupName].soundDict[soundName].Stop();
            groupDict[groupName].soundDict[soundName].volume = cacheVolume;
        }

    }
}