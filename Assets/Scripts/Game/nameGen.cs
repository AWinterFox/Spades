using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System;

public static class NameGen
{
    public static string GetAiName()
    {
        string[] names = new string[] { "Aaren", "Aarika", "Abagael", "Abagail", "Abbe", "Abbey", "Abbi", "Abbie", "Abby", "Abbye", "Abigael", "Abigail", "Abigale", "Abra", "Ada", "Adah", "Adaline", "Adan", "Adara", "Adda", "Addi", "Addia", "Addie", "Addy", "Adel", "Adela", "Adelaida" };
        var name = names[UnityEngine.Random.Range(0, names.Length)];
        return name;
    }

    public static string GetSimulatedName()
    {
        string[] names = new string[] { "obiwanrigaud", "factsoutline", "sandalsopinion", "Lazyamphora", "mainmastgrouch", "raddl3dl!lypad", "Laddiedeer", "3mulsionlocate", "GUMMYswarpet", "bobolynerappel", "randomCHAMPion", "placatefalse", "lumpishfraunhofer", "niagaradisease", "sufficemidnite", "spokeerstwhile", "hinnyprimary", "shoalstemson", "paintingjuniper", "blacksmonkeys", "cutidealistic", "raisinnewton", "Cosmicsoupy", "equinoxon", "Stageflinch", "dupl3xe" };
        var name = names[UnityEngine.Random.Range(0, names.Length)];
        return name;
    }
}

