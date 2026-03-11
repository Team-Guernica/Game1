using UnityEngine;

public class TradeNPC : MonoBehaviour
{
    [Header("NPC Recipe")]
    [SerializeField] private TradeRecipe npcRecipe;

    private void OnMouseDown()
    {
        if (TradeUIManager.Instance == null)
        {
            Debug.LogWarning("[TradeNPC] TradeUIManager.Instance 없음");
            return;
        }

        if (npcRecipe == null)
        {
            Debug.LogWarning("[TradeNPC] npcRecipe 없음");
            return;
        }

        TradeUIManager.Instance.SetNpcRecipe(npcRecipe);
        TradeUIManager.Instance.OpenTradeUI();
    }
}