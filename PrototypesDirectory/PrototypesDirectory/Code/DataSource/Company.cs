using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Matematici.DataSource
{
    public class Company
    {
        public Company(string Name, string Folder)
        {
            this.Name = Name;
            this.Folder = Folder;
            FileSystemWatcher = new FileSystemWatcher(Folder, "*.pdf")
            {
                NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite
            };
            FileSystemWatcher.Error += FileSystemWatcher_Error;
            FileSystemWatcher.Changed += FileSystemWatcher_Changed;
            FileSystemWatcher.Created += FileSystemWatcher_Changed;
            FileSystemWatcher.Deleted += FileSystemWatcher_Changed;
            FileSystemWatcher.Renamed += FileSystemWatcher_Changed;
            FileSystemWatcher.EnableRaisingEvents = true;
            ReadFolder();
        }

        public string Name { get; }
        public string Folder { get; }

        private FileSystemWatcher FileSystemWatcher { get; }
        private bool Invalid { get; set; }
        private Thread Thread { get; set; }

        public Exception Exception { get; private set; }

        private List<PrototypeFile> Prototypes { get; set; } = new List<PrototypeFile>();
        public List<string> Departments { get; private set; } = new List<string>();
        public Dictionary<string, List<string>> PrototypeTypes { get; private set; } = new Dictionary<string, List<string>>();

        public List<PrototypeFile> Search(string Department, string PrototypeType, string Prototype)
        {
            return Prototypes.Where(x =>
                (string.IsNullOrEmpty(Department) || x.Department == Department)
                && (string.IsNullOrEmpty(PrototypeType) || x.PrototypeType == PrototypeType)
                && (string.IsNullOrEmpty(Prototype) || x.Code.Contains(Prototype) || x.Description.Contains(Prototype))
            ).ToList() ?? new List<PrototypeFile>();
        }

        private void FileSystemWatcher_Error(object sender, ErrorEventArgs e)
        {
            FileSystemWatcher.EnableRaisingEvents = false;
            Exception = e.GetException();
        }

        private void FileSystemWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Refresh();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void Refresh()
        {
            if (Thread == null)
            {
                Thread = new Thread(new ThreadStart(ReadFolder));
                Thread.Start();
            }
            else
                Invalid = true;
        }

        private void ReadFolder()
        {
            try
            {
                int PathOffset = Folder.Length;
                var Prototypes = 
                    Directory.GetFiles(Folder, "*.pdf")
                    .Select(f => new PrototypeFile(f.Substring(PathOffset)))
                    .OrderBy(x => x.Company).ThenBy(x => x.Department).ThenBy(x => x.PrototypeType).ThenBy(x => x.Code).ThenBy(x => x.Description)
                    .ToList();
                var PrototypeTypes =
                    Prototypes
                        .GroupBy(d => d.Department).ToDictionary(d => d.Key, d => d
                            .Select(t => t.PrototypeType).Distinct().ToList()
                        );
                var Departments = PrototypeTypes.Keys.ToList();

                SwapSources(Departments, PrototypeTypes, Prototypes);

                Thread = null;
                if (Invalid)
                {
                    Invalid = false;
                    Refresh();
                }
            }
            catch (Exception Exception)
            {
                this.Exception = Exception;
                FileSystemWatcher.EnableRaisingEvents = false;
                Thread = null;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SwapSources(List<string> Departments, Dictionary<string, List<string>> PrototypeTypes, List<PrototypeFile> Prototypes)
        {
            this.Prototypes = Prototypes;
            this.Departments = Departments;
            this.PrototypeTypes = PrototypeTypes;
        }
    }
}