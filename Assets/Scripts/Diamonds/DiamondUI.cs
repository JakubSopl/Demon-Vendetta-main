using UnityEngine;
using UnityEngine.UI;

public class DiamondUI : MonoBehaviour
{
    public Image lapizImage;
    public Image rubyImage;
    public Image emeraldImage;
    public Sprite lapizColoredSprite;
    public Sprite rubyColoredSprite;
    public Sprite emeraldColoredSprite;

    public void UpdateDiamondUI(Diamond.DiamondType type)
    {
        switch (type)
        {
            case Diamond.DiamondType.Lapiz:
                lapizImage.sprite = lapizColoredSprite;
                break;
            case Diamond.DiamondType.Ruby:
                rubyImage.sprite = rubyColoredSprite;
                break;
            case Diamond.DiamondType.Emerald:
                emeraldImage.sprite = emeraldColoredSprite;
                break;
        }
    }
}
