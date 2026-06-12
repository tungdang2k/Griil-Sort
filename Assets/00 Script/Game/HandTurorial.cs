using System.Collections;
using DG.Tweening;
using UnityEngine;

public class HandTurorial : MonoBehaviour
{
    [SerializeField] private RectTransform m_hand;
    [SerializeField] private RectTransform m_startPos;
    [SerializeField] private RectTransform m_endPos;
    private Sequence m_seq;

    public void ShowHandGuide()
    {
        gameObject.SetActive(true);
        StartCoroutine(HandGuide());
    }

    private IEnumerator HandGuide()
    {
        yield return null;
        
        m_seq?.Kill();

        m_seq = DOTween.Sequence()
            .SetLink(gameObject)
            .SetLoops(-1, LoopType.Restart);

        m_seq.AppendCallback(() =>
        {
            m_hand.position = m_startPos.position;
        });
         
        m_seq.Append(m_hand.DOMove(
            m_endPos.position,
            3f));
    }

    public void HideHandeGuide()
    {
        m_seq?.Kill();
        gameObject.SetActive(false);
    }

}
