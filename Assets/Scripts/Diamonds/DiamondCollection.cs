using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondCollection : MonoBehaviour
{

    public static DiamondCollection Instance { get; private set; }

    public DiamondUI diamondUI;

    private bool hasLapiz = false;
    private bool hasRuby = false;
    private bool hasEmerald = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CollectDiamond(Diamond.DiamondType type)
    {
        switch (type)
        {
            case Diamond.DiamondType.Lapiz:
                hasLapiz = true;
                diamondUI.UpdateDiamondUI(Diamond.DiamondType.Lapiz);
                break;
            case Diamond.DiamondType.Ruby:
                hasRuby = true;
                diamondUI.UpdateDiamondUI(Diamond.DiamondType.Ruby);
                break;
            case Diamond.DiamondType.Emerald:
                hasEmerald = true;
                diamondUI.UpdateDiamondUI(Diamond.DiamondType.Emerald);
                break;
        }
    }

    public bool AllDiamondsCollected()
    {
        return hasLapiz && hasRuby && hasEmerald;
    }
}
