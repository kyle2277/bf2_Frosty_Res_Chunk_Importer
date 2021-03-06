﻿// CustomAssetContextMenu.cs - FrostyMeshImporter
// Contributors:
//      Copyright (C) 2021  Kyle Won
// This file is subject to the terms and conditions defined in the 'LICENSE' file.

using System;
using FrostyEditor;
using FrostyEditor.Controls;
using FrostySdk.Managers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FrostyMeshImporter.Program;

namespace FrostyMeshImporter.Controls
{
    // Injects new functions into the context menu for AssetEntry items in the given FrostyDataExplorer.
    class CustomAssetContextMenu
    {
        // Number of functions added to the context menu.
        public int numAddedCommands;
        // The data explorer to inject functions into.
        private FrostyDataExplorer dataExplorer;
        // Context menu icons.
        private static Image _editLabelIcon = new Image
        {
            Source = new BitmapImage(new Uri("/FrostyEditor;Component/Images/EditLabel.png", UriKind.Relative)),
            Opacity = 0.5
        };
        private static Image _exportIcon = new Image
        {
            Source = new BitmapImage(new Uri("/FrostyEditor;Component/Images/Export.png", UriKind.Relative)),
            Opacity = 0.5
        };
        private static Image _revertIcon = new Image
        {
            Source = new BitmapImage(new Uri("/FrostyEditor;Component/Images/Revert.png", UriKind.Relative)),
            Opacity = 0.5
        };

        public CustomAssetContextMenu(FrostyDataExplorer dataExplorer)
        {
            this.dataExplorer = dataExplorer;
            numAddedCommands = 0;
        }

        // Remove all added functons from context menu.
        public void ResetContextMenu()
        {
            var items = dataExplorer.AssetContextMenu.Items;
            while(numAddedCommands > 0)
            {
                items.RemoveAt(items.Count - 1);
                numAddedCommands -= 1;
            }
        }

        // Add functions to selected asset context menu depending on the type of asset.
        public void UpdateContextMenu(object sender, RoutedEventArgs e)
        {
            if(dataExplorer?.SelectedAsset is AssetEntry asset)
            {
                ResetContextMenu();
                if(asset.Type.Contains("MeshAsset"))
                {
                    // Add res file export to context menu
                    MenuItem meshExport = new MenuItem();
                    meshExport.Click += OnExportResourceFilesCommand;
                    meshExport.Header = "Export Mesh Files";
                    meshExport.Icon = _exportIcon;
                    // Todo icon
                    dataExplorer.AssetContextMenu.Items.Add(meshExport);
                    numAddedCommands += 1;
                } else if(asset.Type.Equals("FsUITextDatabase"))
                {
                    // Add FrosTxt to context menu
                    MenuItem openFrosTxt = new MenuItem();
                    openFrosTxt.Click += OnFrosTxtCommand;
                    openFrosTxt.Header = "Open FrosTxt";
                    openFrosTxt.Icon = _editLabelIcon;
                    MenuItem revertLocalization = new MenuItem();
                    revertLocalization.Click += ContextRevertProfile;
                    revertLocalization.Header = "Revert FrosTxt";
                    revertLocalization.Icon = _revertIcon;
                    dataExplorer.AssetContextMenu.Items.Add(openFrosTxt);
                    dataExplorer.AssetContextMenu.Items.Add(revertLocalization);
                    numAddedCommands += 2;
                }
            }
        }
    }
}
