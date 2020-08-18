﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Frosty.Controls;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Attributes;
using FrostySdk.Ebx;
using FrostySdk.IO;
using FrostySdk.Managers;
using FrostySdk.Resources;
using Microsoft.Win32;
using System.IO;
using System.Collections.ObjectModel;
using System.CodeDom;

// <summary>
// Author: Kyle Won
// Facilitates res/chunk file imports within the Frosty res/chunk explorer window.
// </summary>

namespace FrostyResChunkImporter
{
    class ChunkResImporter
    {
        private FrostyChunkResExplorer _chunkResExplorer;
        private static List<ChunkAssetEntry> _allChunks;
        private static List<object> _allResFiles;
        private List<ChunkResFile> _chunkFiles;
        private List<ChunkResFile> _resFiles;
        private string _searchName;

        public ChunkResImporter(FrostyChunkResExplorer chunkResExplorer, List<ChunkResFile> chunkFiles, List<ChunkResFile> resFiles)
        {
            _chunkResExplorer = chunkResExplorer;
            _chunkFiles = chunkFiles;
            _resFiles = resFiles;
            
        }

        // Facilitates chunk imports from chunk files list. Takes boolean which denotes whether to import or revert files
        public void Import(bool revert)
        {
            // Checks if chunks have been indexed
            if (_allChunks == null || _allResFiles == null)
            {
                InitResChunkLists(_chunkResExplorer);
            }
            Predicate<ChunkAssetEntry> predicate = compareChunks;

            // Find and import chunks in Frosty chunk explorer
            foreach (ChunkResFile newChunk in _chunkFiles)
            {
                _searchName = newChunk.fileName;
                ChunkAssetEntry oldChunk = _allChunks.Find(predicate);
                if (revert) 
                { 
                    RevertAsset(oldChunk); 
                }
                else 
                { 
                    ImportChunk(oldChunk, newChunk);
                }
            }

            // Find and import res files in Frosty res explorer


        }

        // Imports a single chunk. Takes chunk to replace and path to new chunk
        private int ImportChunk(ChunkAssetEntry oldChunk, ChunkResFile newChunk)
        {
            using (NativeReader nativeReader = new NativeReader(new FileStream(newChunk.absolutePath, FileMode.Open, FileAccess.Read)))
            {
                byte[] end = nativeReader.ReadToEnd();
                App.AssetManager.ModifyChunk(oldChunk.Id, end, (Texture)null);
            }
            return 0;
        }

        // Import res file from res files list
        public int ImportResFiles()
        {
            return 0;
        }

        private static void InitResChunkLists(FrostyChunkResExplorer chunkResExplorer)
        {
            ListBox chunksListBox = ReflectionHelper.GetFieldValue<ListBox>(chunkResExplorer, "chunksListBox");
            FrostyDataExplorer resExplorer = ReflectionHelper.GetFieldValue<FrostyDataExplorer>(chunkResExplorer, "resExplorer");
            TreeView resTreeList = ReflectionHelper.GetFieldValue<TreeView>(resExplorer, "assetTreeView");
            IEnumerable<object> curResExplorerItems = ReflectionHelper.GetPropertyValue<IEnumerable<object>>(resTreeList, "ItemsSource");
           

            // Add all chunks to a list 
            _allChunks = new List<ChunkAssetEntry>();
            foreach (ChunkAssetEntry chunk in chunksListBox.Items)
            {
                _allChunks.Add(chunk);
            }


            // Recursively add all res files to a list
            _allResFiles = new List<object>();
            foreach (object res in curResExplorerItems)
            {
                _allResFiles.Add(res);
                IEnumerable<object> nextResExplorerItems = ReflectionHelper.GetPropertyValue<IEnumerable<object>>(res, "Children");
                if (nextResExplorerItems.Count() > 0)
                {
                    traverseRecursive(nextResExplorerItems);
                }
            }
            return;
        }

        private static void traverseRecursive(IEnumerable<object> children)
        {
            foreach(object res in children)
            {
                _allResFiles.Add(res);
                IEnumerable<object> nextResExplorerItems = ReflectionHelper.GetPropertyValue<IEnumerable<object>>(res, "Children");
                if (nextResExplorerItems.Count() > 0)
                {
                    traverseRecursive(nextResExplorerItems);
                }
            }
        }

        private bool compareChunks(ChunkAssetEntry oldChunk)
        {
            return oldChunk.Name == _searchName;
        }

        // Revert given chunk
        public void RevertAsset(AssetEntry chunk)
        {
            if (chunk == null || !chunk.IsModified)
                return;
            App.AssetManager.RevertAsset(chunk, false, false);
        }
    }
}
