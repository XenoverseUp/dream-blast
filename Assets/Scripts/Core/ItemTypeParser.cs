using System.Collections.Generic;
using UnityEngine;

public interface IItemTypeParser {
    bool CanParse(string typeString);
    CellItemType ParseType(string typeString);
}

public class CubeTypeParser : IItemTypeParser {
    public bool CanParse(string typeString) {
        return typeString == "r" || typeString == "g" || typeString == "b" || typeString == "y";
    }
    
    public CellItemType ParseType(string typeString) {
        return typeString switch {
            "r" => CellItemType.RedCube,
            "g" => CellItemType.GreenCube,
            "b" => CellItemType.BlueCube,
            "y" => CellItemType.YellowCube,
            _ => CellItemType.Empty,
        };
    }
}

public class RocketTypeParser : IItemTypeParser {
    public bool CanParse(string typeString) {
        return typeString == "vro" || typeString == "hro";
    }
    
    public CellItemType ParseType(string typeString) {
        return typeString switch {
            "vro" => CellItemType.VerticalRocket,
            "hro" => CellItemType.HorizontalRocket,
            _ => CellItemType.Empty,
        };
    }
}

public class ObstacleTypeParser : IItemTypeParser {
    public bool CanParse(string typeString) {
        return typeString == "bo" || typeString == "s" || typeString == "v";
    }
    
    public CellItemType ParseType(string typeString) {
        return typeString switch {
            "bo" => CellItemType.Box,
            "s" => CellItemType.Stone,
            "v" => CellItemType.Vase,
            _ => CellItemType.Empty,
        };
    }
}

public class RandomTypeParser : IItemTypeParser {
    public bool CanParse(string typeString) {
        return typeString == "rand";
    }
    
    public CellItemType ParseType(string typeString) {
        CellItemType[] cubeTypes = new CellItemType[] {
            CellItemType.RedCube,
            CellItemType.GreenCube,
            CellItemType.BlueCube,
            CellItemType.YellowCube
        };
        
        return cubeTypes[Random.Range(0, cubeTypes.Length)];
    }
}


public class ItemTypeParserManager {
    private static ItemTypeParserManager instance;
    private List<IItemTypeParser> parsers = new List<IItemTypeParser>();
    
    private static readonly object lockObject = new object();
    
    
    public static ItemTypeParserManager Instance {
        get {
            if (instance == null) {
                lock (lockObject) {
                    instance ??= new ItemTypeParserManager();
                }
            }
            return instance;
        }
    }
    
    
    private ItemTypeParserManager() {
        parsers.Add(new CubeTypeParser());
        parsers.Add(new RocketTypeParser());
        parsers.Add(new ObstacleTypeParser());
        parsers.Add(new RandomTypeParser());
    }
    
    
    public CellItemType ParseType(string typeString) {
        foreach (var parser in parsers) {
            if (parser.CanParse(typeString)) {
                return parser.ParseType(typeString);
            }
        }
        
        return CellItemType.Empty;
    }
    
    
    public void AddParser(IItemTypeParser parser) {
        if (parser != null && !parsers.Contains(parser)) {
            parsers.Add(parser);
        }
    }

    public bool IsCube(CellItemType type) {
        return type == CellItemType.RedCube || 
               type == CellItemType.GreenCube || 
               type == CellItemType.BlueCube || 
               type == CellItemType.YellowCube;
    }
    
    public bool IsRocket(CellItemType type) {
        return type == CellItemType.HorizontalRocket || 
               type == CellItemType.VerticalRocket;
    }
    
    public bool IsObstacle(CellItemType type) {
        return type == CellItemType.Box || 
               type == CellItemType.Stone || 
               type == CellItemType.Vase;
    }

    public bool IsCube(string type) {
        return type == "r" || 
               type == "g" || 
               type == "b" || 
               type == "y" ||
               type == "rand";
    }
    
    public bool IsRocket(string type) {
        return type == "hro" || type == "vro";
    }
    
    public bool IsObstacle(string type) {
        return type == "bo" || 
               type == "s" || 
               type == "v";
    }
}