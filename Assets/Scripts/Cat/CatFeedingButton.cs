using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class FeedCatButton : MonoBehaviour
{
    public CatController catController;
    public Bowl bowl;

    public void OnClick()
    {
        Debug.Log("[FeedCatButton] OnClick() → нажата кнопка");

        if (catController == null || bowl == null)
        {
            Debug.LogError("[FeedCatButton] OnClick() → нет CatController или Bowl!");
            return;
        }

        if (!catController.CanBeFed())
        {
            Debug.Log("[FeedCatButton] OnClick() → кот не голоден");
            catController.ResumeFromMenu();
            return;
        }

        catController.ResumeFromMenu();

        Debug.Log("[FeedCatButton] OnClick() → кот идёт к миске");
        catController.GoEatFromBowl(bowl);
    }
}