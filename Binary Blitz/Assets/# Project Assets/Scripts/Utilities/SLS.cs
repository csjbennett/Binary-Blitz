using UnityEngine;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;

/// <summary>
/// The SLS (saving & loading system) is a quick and easy way to save bits of information in a file
/// </summary>
public static class SLS
{
    private static string directoryPath = Application.dataPath + "\\SaveData";
    private static string savePath = directoryPath + "\\Save.txt";

    public static List<SaveEntry> savedInts = new List<SaveEntry>();
    public static List<SaveEntry> savedFloats = new List<SaveEntry>();
    public static List<SaveEntry> savedStrings = new List<SaveEntry>();

    private static Dictionary<string, int> intDict = new Dictionary<string, int>();
    private static Dictionary<string, float> floatDict = new Dictionary<string, float>();
    private static Dictionary<string, string> stringDict = new Dictionary<string, string>();

    // Load data from save file
    public static void LoadSaveFile()
    {
        if (!Directory.Exists(directoryPath))
        { Directory.CreateDirectory(directoryPath); }
        if (!File.Exists(savePath))
        {
            File.Create(savePath).Close();
            string templateSave = Encrypt(JsonUtility.ToJson(new SaveData()));
            File.WriteAllText(savePath, templateSave);
        }

        string encryptedSave = File.ReadAllText(savePath);
        string savedFile = Decrypt(encryptedSave);
        SaveData save = JsonUtility.FromJson<SaveData>(savedFile);

        savedInts = save.savedInts;
        savedFloats = save.savedFloats;
        savedStrings = save.savedStrings;

        InitializeDictionaries();
    }

    // Write data to save file
    public static void WriteSaveFile()
    {
        SaveData save = new SaveData(savedInts, savedFloats, savedStrings);
        string saveJson = JsonUtility.ToJson(save);
        string encryptedSave = Encrypt(saveJson);

        File.WriteAllText(savePath, encryptedSave);
    }

    // Initializes dictionaries with saved data
    public static void InitializeDictionaries()
    {
        foreach (SaveEntry intEntry in savedInts)
        { intDict.Add(intEntry.key, int.Parse(intEntry.value)); }

        foreach (SaveEntry floatEntry in savedFloats)
        { floatDict.Add(floatEntry.key, float.Parse(floatEntry.value)); }

        foreach (SaveEntry stringEntry in savedStrings)
        { stringDict.Add(stringEntry.key, stringEntry.value); }
    }

    // Ints
    public static void SetInt(string key, int value)
    {
        if (!intDict.ContainsKey(key))
            intDict.Add(key, value);
        else
            intDict.Add(key, value);
    }
    public static int GetInt(string key, int defaultValue = 0)
    {
        if (!intDict.ContainsKey(key))
        {
            intDict.Add(key, defaultValue);
            return defaultValue;
        }
        else
            return intDict[key];
    }

    // Floats
    public static void SetFloat(string key, float value)
    {
        if (!floatDict.ContainsKey(key))
            floatDict.Add(key, value);
        else
            floatDict.Add(key, value);
    }
    public static float GetFloat(string key, float defaultValue = 0f)
    {
        if (!floatDict.ContainsKey(key))
        {
            floatDict.Add(key, defaultValue);
            return defaultValue;
        }
        else
            return floatDict[key];
    }

    // Strings
    public static void SetString(string key, string value)
    {
        if (!stringDict.ContainsKey(key))
            stringDict.Add(key, value);
        else
            stringDict.Add(key, value);
    }
    public static string GetString(string key, string defaultValue = "")
    {
        if (!stringDict.ContainsKey(key))
        {
            stringDict.Add(key, defaultValue);
            return defaultValue;
        }
        else
            return stringDict[key];
    }

    // Encryption
    private static byte[] key = new byte[8] { 5, 1, 8, 2, 3, 9, 2, 1 };
    private static byte[] iv = new byte[8] { 5, 1, 8, 2, 3, 9, 2, 1 };

    private static string Encrypt(this string text)
    {
        SymmetricAlgorithm algorithm = DES.Create();
        ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
        byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
        return Convert.ToBase64String(outputBuffer);
    }
    private static string Decrypt(this string text)
    {
        SymmetricAlgorithm algorithm = DES.Create();
        ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
        byte[] inputbuffer = Convert.FromBase64String(text);
        byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
        return Encoding.Unicode.GetString(outputBuffer);
    }
}

[Serializable]
public class SaveEntry
{
    public string key; public string value;
}

[Serializable]
public class SaveData
{
    public SaveData()
    {
        savedInts = new List<SaveEntry>();
        savedFloats = new List<SaveEntry>();
        savedStrings = new List<SaveEntry>();
    }

    public SaveData(List<SaveEntry> savedInts, List<SaveEntry> savedFloats, List<SaveEntry> savedStrings)
    {
        this.savedInts = savedInts;
        this.savedFloats = savedFloats;
        this.savedStrings = savedStrings;
    }

    public List<SaveEntry> savedInts = new List<SaveEntry>();
    public List<SaveEntry> savedFloats = new List<SaveEntry>();
    public List<SaveEntry> savedStrings = new List<SaveEntry>();
}