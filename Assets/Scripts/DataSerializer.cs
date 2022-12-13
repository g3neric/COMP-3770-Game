using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class DataSerializer : MonoBehaviour {
    public Dictionary<string, int> CreateData() {
        Dictionary<string, int> data = new Dictionary<string, int>();
        data.Add("TotalDeaths", 0); //
        data.Add("TotalKills", 0); //
        data.Add("TotalExtractions", 0); //
        data.Add("TotalSessions", 0); //
        data.Add("FastestRunInSeconds", 0); //
        data.Add("TotalSecondsPlayed", 0); //
        data.Add("TotalGoldAcquired", 0); //
        data.Add("EzDifficultyCount", 0);
        data.Add("MidDifficultyCount", 0);
        data.Add("ImpossibleDifficultyCount", 0);
        return data;
    }

    public void ResetData() {
        string destination = Application.persistentDataPath + "/save.dat";
        File.Delete(destination);
        CheckIfFileCreated();
    }

    public void CheckIfFileCreated() {
        string destination = Application.persistentDataPath + "/save.dat";
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (!File.Exists(destination)) {
            file = File.Create(destination);
            bf.Serialize(file, CreateData());
            file.Close();
        }
    }

    public void ModifySavedDataValue(string varName, int value, bool increment) {
        FileStream file;
        BinaryFormatter bf = new BinaryFormatter();
        string destination = Application.persistentDataPath + "/save.dat";

        if (File.Exists(destination)) {
            // read all data
            Dictionary<string, int> data = LoadFile();
            if (increment) {
                data[varName] += value;
            } else {
                data[varName] = value;
            }

            // open file for writing
            file = File.OpenWrite(destination);

            // serialize data and write to file
            bf.Serialize(file, data);

            // close file
            file.Close();
        } 
    }

    // returns a dictionary with all stats 
    public Dictionary<string, int> LoadFile() {
        string destination = Application.persistentDataPath + "/save.dat";
        FileStream file;

        if (File.Exists(destination)) {
            // open file
            file = File.OpenRead(destination);
            BinaryFormatter bf = new BinaryFormatter();
            Dictionary<string, int> data = (Dictionary<string, int>)bf.Deserialize(file);

            // close file
            file.Close();
            // return the dictionary
            return data;
        } else {
            Debug.LogError("File not found");
            return null;
        }
    }
}

