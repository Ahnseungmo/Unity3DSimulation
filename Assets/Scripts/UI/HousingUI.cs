using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HousingUI : MonoBehaviour
{
    public static HousingUI Instance;

    public GameObject Panel;
    public Transform ContentParent;
    public GameObject ButtonPrefab;

    private void Awake()
    {
        Instance = this;
        Panel.SetActive(false);
    }

    private void Start()
    {
        // Populate UI with available furniture
        // Assuming WorldObjectManager has the list
        if (WorldObjectManager.Instance != null)
        {
            foreach (var prefab in WorldObjectManager.Instance.WorldObjectPrefabs)
            {
                var btnObj = Instantiate(ButtonPrefab, ContentParent);
                var btn = btnObj.GetComponent<Button>();
                var txt = btnObj.GetComponentInChildren<Text>(); // Or TMP_Text

                if (txt != null) txt.text = prefab.name;

                string prefabName = prefab.name;
                btn.onClick.AddListener(() => OnFurnitureSelected(prefabName));
            }
        }
    }

    public void Toggle()
    {
        Panel.SetActive(!Panel.activeSelf);
    }

    private void OnFurnitureSelected(string prefabName)
    {
        HousingManager.Instance.StartPlacement(prefabName);
        Toggle(); // Close UI after selection
    }
}
