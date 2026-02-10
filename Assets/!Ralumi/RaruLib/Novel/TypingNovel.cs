using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RaruLib
{
    [Serializable]
    public class TalkDataSet
    {
        public Image textBoxImg;
        public TextMeshProUGUI textBox;
    }

    public class TypingNovel : MonoBehaviour
    {
        [SerializeField] TalkDataSet[] talkDataSet;
        [SerializeField] float textDuration = 1f;
        [Header("音いるなら")]
        [SerializeField] private SoundCommand soundCommand;
        [SerializeField] private string soundName = "popopo";

        private Subject<Unit> WaitNextSubject = new Subject<Unit>();
        public IObservable<Unit> OnWaitNext => WaitNextSubject;

        private bool isTyping;

        public void SetTalkData(TextBoxKind kind, string _text)
        {
            SetViewTalkData(kind, true);
            SetTypingText(kind, _text).Forget();
        }

        private void SetViewTalkData(TextBoxKind kind, bool isSet)
        {
            if (talkDataSet[(int)kind].textBoxImg != null) talkDataSet[(int)kind].textBoxImg.enabled = isSet;
            if (talkDataSet[(int)kind].textBox != null) talkDataSet[(int)kind].textBox.enabled = isSet;
        }

        private async UniTask SetTypingText(TextBoxKind kind, string _text)
        {
            var token = this.GetCancellationTokenOnDestroy();

            // タイプしてる時に新しいのが流れてきたら止めておく
            await UniTask.WaitUntil(() => !isTyping, cancellationToken: token);

            isTyping = true;
            talkDataSet[(int)kind].textBox.text = "";

            foreach (var text in _text)
            {
                await UniTask.WaitForSeconds(textDuration, cancellationToken: token);
                if (soundCommand != null) soundCommand.CallPlaySE(soundName);
                talkDataSet[(int)kind].textBox.text += text;
            }

            isTyping = false;
            WaitNextSubject.OnNext(Unit.Default);
        }

        public void HideTextBox(TextBoxKind kind)
        {
            talkDataSet[(int)kind].textBox.text = "";
            SetViewTalkData(kind, false);
        }
    }
}