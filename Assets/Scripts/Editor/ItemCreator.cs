using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ItemCreator : EditorWindow {

    [MenuItem("Window/Item Creator")]
    public static void ShowWindow()
    {
        GetWindow<ItemCreator>("Item Creator");
    }

    string itemID = "";
    string itemName = "";
    string itemDescription = "";

    void OnGUI()
    {
        GUILayout.Label("Fill the data and click create!");
        GUILayout.Label("");

        GUILayout.Label("Item ID:");
        itemID = GUILayout.TextArea(itemID);

        GUILayout.Label("Item Name:");
        itemName = GUILayout.TextArea(itemName);

        GUILayout.Label("Item Description:");
        itemDescription = GUILayout.TextArea(itemDescription);

        if (GUILayout.Button("Create Item"))
            CreateItem();
    }

    void CreateItem()
    {
        if (string.IsNullOrEmpty(itemID) || string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(itemDescription))
            return;

        string itemData = "INSERT INTO items(ID, Name, Description) VALUES('" + itemID + "', '" + itemName + "', '" + itemDescription + "')";
        itemID = itemName = itemDescription = "";

        if (File.Exists(itemID + ".sql"))
            File.Delete(itemID + ".sql");

        var f = File.Create(itemID + ".sql");
        f.Close();

        File.WriteAllText(itemID + ".sql", itemData);

    }
}
