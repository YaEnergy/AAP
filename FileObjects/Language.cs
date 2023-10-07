﻿using AAP.Files;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AAP.FileObjects
{

    public class Language : INotifyPropertyChanged
    {
        private readonly Dictionary<string, string> translationDictionary;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// a language that can not translate anything
        /// </summary>
        public Language()
        {
            translationDictionary = new();
        }

        [JsonConstructor]
        public Language(Dictionary<string, string> translationDictionary) 
        {
            this.translationDictionary = translationDictionary;   
        }

        public string GetString(string key)
        {
            if (translationDictionary.ContainsKey(key))
            {
                return translationDictionary[key];
            }

            ConsoleLogger.Warn("No string value for language key " + key + " found!");

            return "No string value for language key " + key + " found!";
        }

        public static Language Decode(Stream stream)
        {
            JsonSerializer js = JsonSerializer.CreateDefault();
            StreamReader sr = new(stream);
            JsonTextReader jr = new(sr);

            Language imported = js.Deserialize<Language>(jr) ?? throw new Exception("No language could be imported!");
            jr.CloseInput = true;
            jr.Close();

            return imported;
        }
    }
}
