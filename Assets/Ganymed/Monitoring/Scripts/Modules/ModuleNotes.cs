using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ganymed.Monitoring.Core;
using Ganymed.Utils.ExtensionMethods;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ganymed.Monitoring.Modules
{
    [CreateAssetMenu(fileName = "Module_Notes", menuName = "Monitoring/Modules/Notes")]
    public class ModuleNotes : Module<string>
    {
        #region --- [FIELDS] ---
        
        [Header("Module TODO Settings")]
        [SerializeField] private TextAsset TextAsset = null;
        [SerializeField] private Color pendingColor = new Color(1f, 0.23f, 0f);
        [SerializeField] private Color checkedColor = new Color(0.6f, 1f, 1f);
        
        public static ModuleNotes Instance = null;
        public static event ModuleUpdateDelegate OnValueChanged;

        private List<Note> Notes = new List<Note>();
        
        private string EditorContentCache = string.Empty;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [STRUCT] ---

        private readonly struct Note
        {
            public readonly string todo;
            public readonly bool done;

            public Note(string todo, bool done)
            {
                this.todo = todo;
                this.done = done;
            }
        }

        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [MODULE INITIALIZATION] ---

        private void OnEnable()
        {
            Notes = ReadDataFromFile(TextAsset).ToList();
            UpdateDisplay();
        }
        
        protected override void OnInitialize()
        {
            Instance = this;
            
            InitializeUpdateEvent(ref OnValueChanged);

            if(Application.isPlaying)
                Notes = ReadDataFromFile(TextAsset).ToList();
            UpdateDisplay();
            InitializeValue(EditorContentCache);
        }

        protected override void OnQuit()
        {
            if(!IsEnabled) return;
            SafeAndUpdate();
        }

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region --- [SAFE] ---

        /// <summary>
        /// Safe Data and update canvas.
        /// </summary>
        private void SafeAndUpdate()
        {
            UpdateDisplay();
            WriteDataToFile(TextAsset, Notes);
        }
    
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [READ AND WRITE] ---

        private static IEnumerable<Note> ReadDataFromFile(TextAsset file)
        {
            var returnValue = new List<Note>();
            
            var text = file.text;
            text = text.Replace("//TODO:", string.Empty);
            var split = text.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var todo in split)
            {
                returnValue.Add(new Note(todo.Replace("[X]", "").Cut().TryRemoveFromEnd('.'), todo.EndsWith("[X]")));
            }
            
            return returnValue;
        }

        private static void WriteDataToFile(Object file, IEnumerable<Note> todos)
        {
#if UNITY_EDITOR
            var text = todos.Aggregate(string.Empty, (current, entry) 
                    => $"{current}//TODO: {entry.todo.Cut()}{(entry.done? " [X]" : "")}{(entry.todo.Cut().EndsWith(".")? "": ".")}\n");

            
            File.WriteAllText(UnityEditor.AssetDatabase.GetAssetPath(file), text);
            UnityEditor.EditorUtility.SetDirty(file);
#endif
        }
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [UPDATE] ---

        private void UpdateDisplay()
        {
            EditorContentCache = string.Empty;
            for (var i = 0; i < Notes.Count; i++)
            {
                EditorContentCache = 
                    $"{EditorContentCache}\n" +
                    $"{(Notes[i].done? checkedColor.AsRichText() : pendingColor.AsRichText())}" +
                    $"{Notes[i].todo} [{i:00}]" +
                    $"";
            }
            OnValueChanged?.Invoke(EditorContentCache);
        }        

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [COMMAND PROCESSING] ---

        public void RemoveAllDataAndCompile()
        {
            Notes.Clear();
            SafeAndUpdate();
        }

        public void RemoveCompletedDataAndCompile()
        {
            for (var i = 0; i < Notes.Count; i++)
            {
                if(Notes[i].done)
                    Notes.Remove(Notes[i]);
            }
            
            SafeAndUpdate();
        }

        public void SetNote(int index, bool value)
        {
            if (Notes.Indices() < index) return;
            
            Notes[index] = new Note(Notes[index].todo, value);
            
            SafeAndUpdate();
        }

        public void RemoveNoteAndCompile(int index)
        {
            if (Notes.Indices() < index) return;
            
            Notes.RemoveAt(index);
            
            SafeAndUpdate();
        }

        public void AddNoteAndCompile(string add)
        {
            if(add.IsNullOrWhiteSpace()) return;
            
            Notes.Add(new Note(add, false));
            
            SafeAndUpdate();
        }

        #endregion
    }
}