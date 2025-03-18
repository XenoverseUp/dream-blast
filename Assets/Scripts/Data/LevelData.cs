using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class LevelData {
    [SerializeField] private int level_number;
    [SerializeField] private int grid_width;
    [SerializeField] private int grid_height;
    [SerializeField] private int move_count;
    [SerializeField] private List<string> grid;
    
    public int LevelNumber => level_number;
    public int GridWidth => grid_width;
    public int GridHeight => grid_height;
    public int MoveCount => move_count;
    public List<string> Grid => grid;

    private static readonly List<string> obstacles = new List<string> {"bo", "v", "s"};
    
    public string GetItemAt(int x, int y) {
        int index = y * grid_width + x;
        
        if (index >= 0 && index < grid.Count) {
            return grid[index];
        }
        
        Debug.LogWarning($"Trying to access grid position outside bounds: ({x}, {y})");
        return "rand";
    }

    public Dictionary<string, int> GetObstacleCountMap() {
        Dictionary<string, int> obstacleCountMap = new Dictionary<string, int>();

        foreach (string item in grid) {
            if (!obstacles.Contains(item)) continue;
            obstacleCountMap[item] = obstacleCountMap.GetValueOrDefault(item, 0) + 1;
        }

        return obstacleCountMap;
    }
    
    public bool IsValidPosition(int x, int y) {
        return x >= 0 && x < grid_width && y >= 0 && y < grid_height;
    }
    
    public bool Validate() {
        if (grid_width <= 0 || grid_height <= 0 || move_count <= 0) {
            Debug.LogError($"Level {level_number}: Invalid grid dimensions or move count");
            return false;
        }
        
        if (grid == null || grid.Count == 0) {
            Debug.LogError($"Level {level_number}: Grid data is missing");
            return false;
        }
        
        if (grid.Count != grid_width * grid_height) {
            Debug.LogError($"Level {level_number}: Grid item count ({grid.Count}) doesn't match dimensions ({grid_width}x{grid_height})");
            return false;
        }
        
        foreach (string item in grid) {
            if (!IsValidGridItem(item)) {
                Debug.LogError($"Level {level_number}: Invalid grid item: {item}");
                return false;
            }
        }
        
        return true;
    }
    
    private bool IsValidGridItem(string itemType) {
        return itemType == "r" || itemType == "g" || itemType == "b" || itemType == "y" || 
               itemType == "rand" || itemType == "vro" || itemType == "hro" || 
               itemType == "bo" || itemType == "s" || itemType == "v";
    }


    // Factory

    public static LevelData LoadLevel(int levelNumber) {
        string resourcePath = $"Levels/level_{levelNumber.ToString("00")}";
        TextAsset levelAsset = Resources.Load<TextAsset>(resourcePath);
        
        if (levelAsset == null) {
            Debug.LogError($"Failed to load level {levelNumber}. File not found at {resourcePath}");
            return null;
        }
        
        try {
            LevelData levelData = JsonUtility.FromJson<LevelData>(levelAsset.text);
            

            if (!levelData.Validate()) {
                Debug.LogError($"Level {levelNumber} validation failed. Check the JSON format.");
                return null;
            }

            
            return levelData;
        } catch (Exception e) {
            Debug.LogError($"Failed to parse level {levelNumber} data: {e.Message}");
            return null;
        }
    }
}