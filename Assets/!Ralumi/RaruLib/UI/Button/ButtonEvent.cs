using System;
using UniRx;
using UnityEngine;

namespace RaruLib
{
    public class ButtonEvent : MonoBehaviour
    {
        private Subject<Unit> clickSubject = new Subject<Unit>();
        public IObservable<Unit> OnClickEvent => clickSubject;

        public void OnClick() { clickSubject.OnNext(Unit.Default); }
    }
}