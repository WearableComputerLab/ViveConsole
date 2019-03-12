/**
* Spatial AR Unity Framework ~ The Crazy Bruce Lab
*
* Copyright (c) 2017 Daniel Jackson
*
* This code can be used by members of the Wearable Computer Lab, University
* of South Australia for research purposes. Commercial use is not allowed
* without written permission. This copyright notice is subject to change.
*/

using Application = UnityEngine.Application;
using Debug = UnityEngine.Debug;
using JsonUtility = UnityEngine.JsonUtility;
using Path = System.IO.Path;
using File = System.IO.File;
using Directory = System.IO.Directory;
using Exception = System.Exception;

namespace UnitySARCommon.Settings
{
    // Base class for Settings objects to be persisted using the SettingsManager.
    public abstract class Settings
    { }

    /**
     * Class to handle reading and writing of Settings from a file.
     * Initialize using a sub-class of the abstract Settings class to read and write instances of that class.
     * This allows you to have application specific fields in settings objects.
     */
    public class SettingsManager<T>
        where T : Settings
    {
        /**
         * Read the contents of a file and parse it as a JSON object of type T.
         * If filename is not an absolute path, it will be relative to the 
         * Application's dataPath.
         */
        public T Load(string filename)
        {
            string srcFilePath = filename;
            if (!Path.IsPathRooted(srcFilePath)) // If relative path, base at dataPath.
            {
                srcFilePath = Path.Combine(Application.dataPath, srcFilePath);
            }
            if (!File.Exists(srcFilePath)) // If file doesn't exist throw an error.
            {
                Debug.Log("Error loading setting file. File " + srcFilePath + " does not exist.");
                throw new Exception("No file " + srcFilePath);
            }
            T settings = JsonUtility.FromJson<T>(File.ReadAllText(srcFilePath));
            return settings;
        }

        /**
         * Convert an object of type T to JSON representation, and output to a file.
         * If filename is not an absolute path, it will be relative to the 
         * Application's dataPath.
         */
        public void Persist(string filename, T settings)
        {
            string dstFilePath = filename;
            if (!Path.IsPathRooted(dstFilePath)) // If relative path, base at dataPath.
            {
                dstFilePath = Path.Combine(Application.dataPath, dstFilePath);
            }
            if (!Directory.Exists(Path.GetDirectoryName(dstFilePath))) // If directory doesnt exist create it.
            {
                Directory.CreateDirectory(Path.GetDirectoryName(dstFilePath));
            }
            File.WriteAllText(dstFilePath, JsonUtility.ToJson(settings));
        }
    }
}