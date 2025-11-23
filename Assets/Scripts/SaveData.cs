using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SaveData
{
    public string sceneName;

    // Time
    public float elapsedTime;

    // Wave
    public int currentWave;
    public bool isWaveActive;
    public float waveTimer;

    // Core + Player
    public CoreSaveData core;
    public PlayerSaveData player;

    // Buildings
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}

[Serializable]
public class CoreSaveData
{
    public float posX, posY, posZ;
    public int currentHealth;
    public int maxHealth;
    public int oreAmount;
}

[Serializable]
public class PlayerSaveData
{
    public float posX, posY, posZ;
}

[Serializable]
public class BuildingSaveData
{
    public string prefabName;   // tên prefab (VD: "Wall", "Turret", "Conveyor")
    public int currentHealth;
    public int anchorX, anchorY;

    // tuỳ loại building
    public int upgradeLevel;
    public int storedOre;
    public int conveyorDir;     // cast từ ConveyorDirection
}
