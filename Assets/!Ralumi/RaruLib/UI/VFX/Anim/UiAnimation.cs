using UnityEngine;
using UnityEngine.EventSystems;

/***************************************************************
* 
* 使い方
* 前提,このクラスはUIのAnimator変数の制御を行います
* 
* １,クラスを任意UI(Button,Img,Panel等)にアタッチ
* ２,UIのRayCastをオンにする
* ３,クラスの変数トリガー設定をカスタマイズする
* 
* 追記,制御をカスタムする場合は本クラスを継承してください
* 
* ************************************************************/

namespace RaruLib
{
    public class UiAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField] protected Animator animator;

        [SerializeField, Tooltip("マウスが触れるとAnimatorのIsSelectがtrueになります")]
        protected bool isSelect = false;

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (isSelect) animator.SetBool("IsSelect", true);
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (isSelect) animator.SetBool("IsSelect", false);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (isSelect) animator.SetBool("IsSelect", false);
        }
    }
}