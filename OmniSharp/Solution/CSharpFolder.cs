using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO;
using System.Linq;
using DesignTimeHostDemo;
using System.Text.RegularExpressions;

namespace OmniSharp.Solution
{
    public class CSharpFolder : ISolution
    {
        Logger _logger;
        CSharpProject _project;
        IFileSystem _fileSystem;

        public CSharpFolder(string folder, Logger logger, IFileSystem fileSystem)
        {
            _logger = logger;
            FileName = folder;
            _fileSystem = fileSystem;
        }

        public string FileName { get; private set; }
        public bool Terminated { get; private set; }
        public bool Loaded { get; private set; }

        public void LoadSolution()
        {
            Loaded = false;
            _project = new CSharpProject(this, _logger, FileName, _fileSystem);
            Loaded = true;

            var dth = new DesignTimeHostDemo.Program();
            var kreHome = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".kre");

            _logger.Debug("KRE Home = " + kreHome);

            var defaultAlias = Path.Combine(kreHome, "alias", "default.alias");

            _logger.Debug("Using default alias = " + defaultAlias);

            var version = File.ReadAllText(defaultAlias).Trim();

            _logger.Debug("Using KRE version = " + version);

            // TODO: Make this work on windows

            var krePath = Path.Combine(kreHome, "packages", version);

            _logger.Debug("Using KRE at = " + krePath);

            dth.Go(krePath, FileName);
            dth.OnUpdateFileReference += OnUpdateFileReference;
        }

        void OnUpdateFileReference(object sender, FileReferenceEventArgs e)
        {
            _project.AddReference(e.Reference);
        }

        public CSharpFile GetFile(string filename)
        {
            return (from file in _project.Files
                     where file.FileName.Equals(filename, StringComparison.InvariantCultureIgnoreCase)
                     select file).FirstOrDefault();
        }

        public IProject ProjectContainingFile(string filename)
        {
            return _project;
        }

        public void Reload()
        {
            LoadSolution();
        }

        public void Terminate()
        {
            Terminated = true;
        }

        public List<IProject> Projects
        {
            get
            {
                return new List<IProject> { _project };
            }
        }

    }

}
