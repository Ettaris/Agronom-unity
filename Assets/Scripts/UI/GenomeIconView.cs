using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Data;

public class GenomeIconView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [SerializeField] private GameObject PermanentBadge;
    private Image _iconImage;
    private GenomePropertyData _data;

    public void Setup(GenomePropertyData data)
    {
        _data = data;
        if (_iconImage != null && data != null)
        {
            _iconImage.sprite = data.icon;
            GetComponent<Image>().sprite = data.icon;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_data != null)
        {
            TooltipManager.Show(_data, eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Hide();
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (_data != null)
        {
            TooltipManager.UpdatePosition(eventData.position);
        }
    }
}