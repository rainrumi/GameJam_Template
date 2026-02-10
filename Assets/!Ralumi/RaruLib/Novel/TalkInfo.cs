using System;
using System.Collections.Generic;
using UnityEngine;

namespace RaruLib
{
    public enum TalkKind
    {
        TalkData1
    }
    public enum TextBoxKind
    {
        Normal1
    }

    [Serializable]
    public class DialogueEntry
    {
        [TextArea(2, 4)] public string text;
        public TextBoxKind textBoxKind;
        public Sprite sprite;
    }

    [Serializable]
    public class DialogueData
    {
        public List<DialogueEntry> dialogues;
    }

    [CreateAssetMenu(fileName = nameof(TalkInfo), menuName = "Settings/" + nameof(TalkInfo))]
    public class TalkInfo : ScriptableObject
    {
        [SerializeField] private List<DialogueData> entries;
        public List<DialogueEntry> GetDialogues(TalkKind Index) => entries[(int)Index].dialogues;

    }
}