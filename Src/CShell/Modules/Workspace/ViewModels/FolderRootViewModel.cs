﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using CShell.Framework.Results;
using Caliburn.Micro;

namespace CShell.Modules.Workspace.ViewModels
{
    public class FolderRootViewModel : FolderViewModel
    {
        private FileSystemWatcher fileSystemWatcher;

        public FolderRootViewModel(string path, CShell.Workspace workspace)
            :base(path, workspace)
        {
            DisplayName = directoryInfo.FullName;
            IsEditable = false;
            IsExpanded = true;


            Initialize();
        }

        private void Initialize()
        {
            //Workspace.PropertyChanged += WorkspaceOnPropertyChanged;
            fileSystemWatcher = new FileSystemWatcher();

            fileSystemWatcher.Path = directoryInfo.FullName;
            fileSystemWatcher.IncludeSubdirectories = true;

            // Add event handlers.
            fileSystemWatcher.Changed += OnChanged;
            fileSystemWatcher.Created += OnChanged;
            fileSystemWatcher.Deleted += OnChanged;
            fileSystemWatcher.Renamed += OnRenamed;

            // Begin watching.
            fileSystemWatcher.EnableRaisingEvents = true;
        }

        private void WorkspaceOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "RootFolder")
            {
                fileSystemWatcher.EnableRaisingEvents = false;
                directoryInfo = new DirectoryInfo(Workspace.WorkspaceDirectory);
                DisplayName = directoryInfo.FullName;

                Children.Clear();
                Children.AddRange(LoadChildren());
                NotifyOfPropertyChange(() => DisplayName);
                NotifyOfPropertyChange(() => ToolTip);
                NotifyOfPropertyChange(() => RelativePath);

                fileSystemWatcher.Path = directoryInfo.FullName;
                fileSystemWatcher.EnableRaisingEvents = true;
            }
            if(args.PropertyName == "Filter")
            {
                Children.Clear();
                Children.AddRange(LoadChildren());
            }
        }

        // Define the event handlers. 
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            //Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            if(e.ChangeType == WatcherChangeTypes.Deleted || e.ChangeType == WatcherChangeTypes.Created || e.ChangeType == WatcherChangeTypes.Renamed)
            {
                //Console.WriteLine("Reloading children");
                Children.Clear();
                Children.AddRange(LoadChildren());
            }
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            //Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
            if(e.ChangeType == WatcherChangeTypes.Renamed)
            {
                //Console.WriteLine("Reloading children");
                Children.Clear();
                Children.AddRange(LoadChildren());
            }
        }

        protected override BindableCollection<TreeViewModel> LoadChildren()
        {
            var children = base.LoadChildren();
            //see if there's a bin folder
            var binVm = children.FirstOrDefault(vm => (vm is FolderViewModel) && vm.DisplayName.Equals(Constants.BinFolder, StringComparison.OrdinalIgnoreCase)) as FolderViewModel;
            if (binVm != null)
            {
                children.Insert(children.IndexOf(binVm), new FolderBinViewModel(binVm.DirectoryInfo, binVm.Workspace));
                children.Remove(binVm);
            }
            var packagesVm = children.FirstOrDefault(vm => (vm is FolderViewModel) && vm.DisplayName.Equals(Constants.PackagesFolder, StringComparison.OrdinalIgnoreCase)) as FolderViewModel;
            if (packagesVm != null)
            {
                children.Insert(children.IndexOf(packagesVm), new FolderPackagesViewModel(packagesVm.DirectoryInfo, packagesVm.Workspace));
                children.Remove(packagesVm);
            }
            var packagesFileVm = children.FirstOrDefault(vm => (vm is FileViewModel) && vm.DisplayName.Equals(Constants.PackagesFile, StringComparison.OrdinalIgnoreCase)) as FileViewModel;
            if (packagesFileVm != null)
            {
                children.Insert(children.IndexOf(packagesFileVm), new FilePackagesViewModel(packagesFileVm.FileInfo));
                children.Remove(packagesFileVm);
            }
            return children;
        }

        #region Edit Root Folder
        public IEnumerable<IResult> ChangeRootFolder()
        {
            //yield return Show.Dialog<RootFolderSettingsViewModel>();
            yield break;
        }

        #endregion

    }//end class
}
