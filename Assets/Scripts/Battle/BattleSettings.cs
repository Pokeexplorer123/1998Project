using UnityEngine;

public class BattleSettings : MonoBehaviour
{
    public static BattleSettings Instance { get; private set; }
    public Texture2D SendOutPalette;

    private void Awake()
    {
        Instance = this;
    }
}
