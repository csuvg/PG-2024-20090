using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Level
{
    public string LevelIndex; // Num del nivel
    public List<Target> Targets; // Lista de objetivos en el nivel
}