using UnityEngine;
using Zenject;

public class HUD : MonoBehaviour
{

    /*      these variables need to be set in the inspector       */
    public DialoguePopUp npcDialoguePopUp;
    public DamagePopUp damagePopUp;
    public ItemChangePopUp itemChangePopUp;

    [Inject]
    UIManager _UIManager;

    void Start()
    {
        // show NPC dialogue screen when UIManager sends NpcSpeakScreenOpened message
        _UIManager.hub.Connect<NpcDialogueScreen>(UIManager.Message.NpcDialogueScreenOpened,
            dialogueScreen => npcDialoguePopUp.Show(dialogueScreen));

        // hide NPC Dialogue PopUp when UIManager sends NpcDialogueScreenCompleted message
        _UIManager.hub.Connect(UIManager.Message.NpcDialogueScreenCompleted,
            () => npcDialoguePopUp.Hide());
            
        /*
        // show damage popup when UIManager sends PlayerDamaged message
        _UIManager.hub.Connect<int>(UIManager.Message.PlayerDamaged,
            damage => damagePopUp.Show(damage));
            
        // show item change popup when UIManager sends ItemChanged message
        _UIManager.hub.Connect<Item>(UIManager.Message.ItemChanged,
            item => damagePopUp.Show(item)); */
    }
}