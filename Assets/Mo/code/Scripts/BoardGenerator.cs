using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public enum TileType
    {
        Monster,
        Chest,
        Shop,
        Boss
    }

    [Header("Board Settings")]
    [Tooltip("Total number of tiles on the board.")]
    public int totalTiles = 20;
    
    [Tooltip("The size of each section before hitting a boss.")]
    public int sectionSize = 5;
    
    [Tooltip("Minimum number of monster tiles required in every section.")]
    public int minMonstersPerSection = 2;

    [Header("Current Board State")]
    public List<TileType> boardLayout = new List<TileType>();

    private void Start()
    {
        GenerateBoard();
        PrintBoard(); // For debugging purposes
    }

    public void GenerateBoard()
    {
        boardLayout.Clear();

        int numSections = totalTiles / sectionSize;

        for (int i = 0; i < numSections; i++)
        {
            List<TileType> section = GenerateSection();
            boardLayout.AddRange(section);
        }
    }

    private List<TileType> GenerateSection()
    {
        List<TileType> section = new List<TileType>();
        
        // We need (sectionSize - 1) normal tiles, and 1 Boss at the end
        int normalTilesCount = sectionSize - 1;
        
        // 1. Initialize empty spots for the normal tiles
        for (int i = 0; i < normalTilesCount; i++)
        {
            section.Add(TileType.Shop); // Placeholder, will be overwritten
        }

        // 2. Fulfill the minimum monster requirement
        for (int i = 0; i < minMonstersPerSection; i++)
        {
            section[i] = TileType.Monster;
        }

        // 3. Fill the remaining spots with random distribution (Monster, Chest, Shop)
        for (int i = minMonstersPerSection; i < normalTilesCount; i++)
        {
            int randomVal = Random.Range(0, 3);
            switch (randomVal)
            {
                case 0: section[i] = TileType.Monster; break;
                case 1: section[i] = TileType.Chest; break;
                case 2: section[i] = TileType.Shop; break;
            }
        }

        // 4. Shuffle the normal tiles to randomize their order within the section
        ShuffleList(section);

        // 5. Always add the Boss at the very end of the section (the 5th tile)
        section.Add(TileType.Boss);

        return section;
    }

    // Helper method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // --- Debugging ---
    private void PrintBoard()
    {
        string boardString = "Generated Board:\n";
        for (int i = 0; i < boardLayout.Count; i++)
        {
            boardString += $"[{i + 1}] {boardLayout[i]}\n";
        }
        Debug.Log(boardString);
    }

    // --- Event Handling ---
    // Call this method when the player lands on a tile
    public void OnTileEntered(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= boardLayout.Count) return;

        TileType currentTile = boardLayout[tileIndex];
        Debug.Log($"Entered Tile {tileIndex + 1}: {currentTile}");

        switch (currentTile)
        {
            case TileType.Monster:
                TriggerCombatLogic();
                break;
            case TileType.Chest:
                FreeGachaShop();
                break;
            case TileType.Shop:
                OpenShopUI();
                break;
            case TileType.Boss:
                TriggerBossEncounter();
                break;
        }
    }

    private void TriggerCombatLogic()
    {
        Debug.Log("Event: Combat started!");
        // TODO: Implement your combat logic here
    }

    private void FreeGachaShop()
    {
        Debug.Log("Event: Opening Free Gacha Shop!");
        // TODO: Implement chest/gacha logic here
    }

    private void OpenShopUI()
    {
        Debug.Log("Event: Opening Shop UI!");
        // TODO: Implement shop UI opening here
    }

    private void TriggerBossEncounter()
    {
        Debug.Log("Event: Boss Encounter started!");
        // TODO: Implement boss logic here
    }
}
