using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Ionic.Zip;
using Ionic.Zlib;

using CSVFiles;

namespace IWDPacker
{
        // GAMEDIR - game directory path
        // FFNAME - name of the FF file(without extension)

        // CSVs..
        // GAMEDIR\zone_source\english\assetlist\FFNAME.csv
        // GAMEDIR\zone_source\english\assetlist\FFNAME_load.csv
        // -includeCod....
            // GAMEDIR\zone_source\english\assetlist\code_post_gfx_mp.csv
            // GAMEDIR\zone_source\english\assetlist\common_mp.csv
            // GAMEDIR\zone_source\english\assetlist\localized_common_mp.csv
            // GAMEDIR\zone_source\english\assetlist\localized_code_post_gfx_mp.csv

        // ARGS
            // gameDir - important are 
            // zone_source
            // raw\soundaliases
            // -gameDir=""
            // -ffName="" // name of compiled FF

            // -outputFile="" // file dir + name + extension
            // -compression="" // between 0 - 9; 9 = the highest
            // -compareDate // TODO!!!
   
            // where looking for ...., may be defined more times = more paths
                // -imagesDir=""
                // -soundsDir=""
                // -weaponsDir=""

            // CSV file for include files, that are not in zone files...
            // may be defined more times = more files
                // -imagesInclude="" // image,imageFileName
                // -soundsInclude="" // loaded_sound,soundFilePath + ext
                // -weaponsInclude="" // weapon,weaponFilePath

                // -imagesIncludeRegex="" // compares list of images from imagesDir
                // -soundsIncludeRegex="" // compares list of sounds from soundsDir
                // -weaponsIncludeRegex="" // compares list of weapons from weaponsDir

            // CSV file for exclude files, that are not in zone files...
            // may be defined more times = more files
                // -imagesExclude="" // image,imageFileName
                // -soundsExclude="" // loaded_sound,soundFilePath + ext
                // -weaponsExclude="" // weapon,weaponFilePath

                // -imagesExcludeRegex="" // compares list of images from FF/IWD/includes
                // -soundsExcludeRegex="" // compares list of sounds from FF/IWD/includes
                // -weaponsExcludeRegex="" // compares list of weapons from FF/IWD/includes

            // -verbose

            // adds to IWD replaced assets, which mod/map use
            // usable for compiling mod...
                // -includeCodImages
                // -includeCodSounds
                // -includeCodWeapons

            // -debugUnusedInDirs // prints a list of all unused images, sounds and weapons, that are in dirs

    class Packer
    {
        public TraceSource Trace { get; private set; }

        bool _verbose;

        string _gameDir;

        string _ffName;

        string _outputFile;
        CompressionLevel _compression;
        bool _compareDate;

        List<string> _imagesDir = new List<string>();
        List<string> _soundsDir = new List<string>();
        List<string> _weaponsDir = new List<string>();

        List<string> _imagesInclude = new List<string>();
        List<string> _soundsInclude = new List<string>();
        List<string> _weaponsInclude = new List<string>();

        string _imagesIncludeRegex = "";
        string _soundsIncludeRegex = "";
        string _weaponsIncludeRegex = "";

        List<string> _imagesExclude = new List<string>();
        List<string> _soundsExclude = new List<string>();
        List<string> _weaponsExclude = new List<string>();

        string _imagesExcludeRegex = "";
        string _soundsExcludeRegex = "";
        string _weaponsExcludeRegex = "";

        bool _includeCodImages;
        bool _includeCodSounds;
        bool _includeCodWeapons;

        bool _debugUnusedInDirs;

        void ParseArgs(string[] args)
        {
            foreach (string arg in args)
            {
                string[] argToks = arg.Split('=');
                string name = argToks[0].TrimStart('-');
                string value = argToks.Length > 1 ? argToks[1] : null;

                switch (name)
                {
                    case "verbose":
                        _verbose = true;
                        break;
                    case "gameDir":
                        _gameDir = value;
                        break;
                    case "ffName":
                        _ffName = value;
                        break;
                    case "outputFile":
                        _outputFile = value;
                        break;
                    case "compression":
                        try
                        {
                            _compression = (CompressionLevel)Enum.Parse(typeof(CompressionLevel), value);
                        }
                        catch (Exception e)
                        { 
                            string compression = "";
                            foreach (string c in Enum.GetNames(typeof(CompressionLevel)))
                                compression += c + " ";

                            throw new ApplicationException("Unknown compression, allowed values are " + compression);
                        }
                        break;
                    case "compareDate":
                        _compareDate = true;
                        break;
                    case "imagesDir":
                        _imagesDir.Add(value);
                        break;
                    case "soundsDir":
                        _soundsDir.Add(value);
                        break;
                    case "weaponsDir":
                        _weaponsDir.Add(value);
                        break;
                    case "imagesInclude":
                        _imagesInclude.Add(value);
                        break;
                    case "soundsInclude":
                        _soundsInclude.Add(value);
                        break;
                    case "weaponsInclude":
                        _weaponsInclude.Add(value);
                        break;
                    case "imagesIncludeRegex":
                        _imagesIncludeRegex = value;
                        break;
                    case "soundsIncludeRegex":
                        _soundsIncludeRegex = value;
                        break;
                    case "weaponsIncludeRegex":
                        _weaponsIncludeRegex = value;
                        break;
                    case "imagesExclude":
                        _imagesExclude.Add(value);
                        break;
                    case "soundsExclude":
                        _soundsExclude.Add(value);
                        break;
                    case "weaponsExclude":
                        _weaponsExclude.Add(value);
                        break;
                    case "imagesExcludeRegex":
                        _imagesExcludeRegex = value;
                        break;
                    case "soundsExcludeRegex":
                        _soundsExcludeRegex = value;
                        break;
                    case "weaponsExcludeRegex":
                        _weaponsExcludeRegex = value;
                        break;
                    case "includeCodImages":
                        _includeCodImages = true;
                        break;
                    case "includeCodSounds":
                        _includeCodSounds = true;
                        break;
                    case "includeCodWeapons":
                        _includeCodWeapons = true;
                        break;
                    case "debugUnusedInDirs":
                        _debugUnusedInDirs = true;
                        break;
                    default:
                        throw new ApplicationException("Unknown arg '" + arg + "'");
                }
            }
        }

        #region ZIP
        int AddFilesToZIP(ZipFile zip, string folderInArchive, List<string> searchDirs, List<string> fileFullPaths)
        {
            int count = 0;
            // delete old unused files..
            Trace.TraceEvent(TraceEventType.Verbose, 0, "Removing old files from IWD...");
            List<string> entriesToRemove = new List<string>();
            foreach (ZipEntry entry in zip.Entries)
            {
                if (!CODTraps.ReplacePathSeps(entry.FileName).StartsWith(folderInArchive))
                    continue;

                string shortPath = CODTraps.ReplacePathSeps(entry.FileName).Substring(folderInArchive.Length + 1);
                if (!AssetListContainsShortPath(shortPath, searchDirs, fileFullPaths))
                {
                    entriesToRemove.Add(entry.FileName);
                    Trace.TraceEvent(TraceEventType.Verbose, 0, CODTraps.ReplacePathSeps(entry.FileName));
                    count++;
                }
            }
            zip.RemoveEntries(entriesToRemove);

            // add & modify files...
            Trace.TraceEvent(TraceEventType.Verbose, 0, "Updating files in IWD...");
            foreach (string fileFullPath in fileFullPaths)
            {
                string shortPath = GetShortPath(fileFullPath, searchDirs);
                string pathInArchive = Path.Combine(folderInArchive, shortPath);

                ZipEntry entry = zip.Entries.FirstOrDefault(a => CODTraps.ReplacePathSeps(a.FileName) == pathInArchive);
                if (entry == null)
                {
                    entry = zip.AddFile(fileFullPath, Path.Combine(folderInArchive, Path.GetDirectoryName(shortPath)));
                    entry.LastModified = DateTime.Now;
                    entry.CompressionLevel = _compression;
                    Trace.TraceEvent(TraceEventType.Verbose, 0, CODTraps.ReplacePathSeps(entry.FileName));
                    count++;
                }
                else if (!_compareDate || File.GetLastWriteTime(fileFullPath) > entry.LastModified)
                {
                    entry = zip.UpdateFile(fileFullPath, Path.Combine(folderInArchive, Path.GetDirectoryName(shortPath)));
                    entry.LastModified = DateTime.Now;
                    entry.CompressionLevel = _compression;
                    Trace.TraceEvent(TraceEventType.Verbose, 0, CODTraps.ReplacePathSeps(entry.FileName));
                    count++;
                }
            }

            return count;
        }

        bool AssetListContainsShortPath(string shortPath, List<string> searchDirs, List<string> fileFullPaths)
        {
            foreach (string fileFullPath in fileFullPaths)
            {
                if (GetShortPath(fileFullPath, searchDirs) == shortPath)
                    return true;
            }
            return false;
        }

        string GetShortPath(string fullPath, List<string> searchDirs)
        {
            foreach (string searchDir in searchDirs)
            {
                if (fullPath.Length > searchDir.Length && fullPath.StartsWith(searchDir))
                    return fullPath.Substring(searchDir.Length + 1);
            }
            return null;
        }

        void zip_SaveProgress(object sender, SaveProgressEventArgs args)
        {
            if (args.EventType != ZipProgressEventType.Saving_AfterWriteEntry)
                return;

            if (args.EntriesTotal > 0)
            {
                float percentage = ((float)args.EntriesSaved / (float)args.EntriesTotal) * 100;
                UpdateProgressCounter(percentage);
            }
        }

        void UpdateProgressCounter(float percentage)
        {
            Console.Write("\rSaving..." + Convert.ToInt32(percentage) + "%".PadRight(67));
        }
        #endregion

        public Packer(string[] args)
        {
            ParseArgs(args);

            Trace = new TraceSource("IWDPacker");
            Trace.Listeners.Add(new MyConsoleListener(_verbose));
            Trace.Switch.Level = SourceLevels.Verbose;

            List<string> imageFiles = GetImageFiles();
            List<string> soundFiles = GetSoundFiles();
            List<string> weaponFiles = GetWeaponFiles();

            using (ZipFile zip = new ZipFile(_outputFile, ASCIIEncoding.ASCII))
            {
                int count;
                Trace.TraceEvent(TraceEventType.Information, 0, "================");
                Trace.TraceEvent(TraceEventType.Information, 0, "Adding images...");
                count = AddFilesToZIP(zip, "images", _imagesDir, imageFiles);
                Trace.TraceEvent(TraceEventType.Information, 0, "Count: " + count);
                Trace.TraceEvent(TraceEventType.Information, 0, "================");

                Trace.TraceEvent(TraceEventType.Information, 0, "================");
                Trace.TraceEvent(TraceEventType.Information, 0, "Adding sounds...");
                count = AddFilesToZIP(zip, "sound", _soundsDir, soundFiles);
                Trace.TraceEvent(TraceEventType.Information, 0, "Count: " + count);
                Trace.TraceEvent(TraceEventType.Information, 0, "================");

                Trace.TraceEvent(TraceEventType.Information, 0, "================");
                Trace.TraceEvent(TraceEventType.Information, 0, "Adding weapons...");
                count = AddFilesToZIP(zip, "weapons\\mp", _weaponsDir, weaponFiles);
                Trace.TraceEvent(TraceEventType.Information, 0, "Count: " + count);
                Trace.TraceEvent(TraceEventType.Information, 0, "================");

                zip.SaveProgress += new EventHandler<SaveProgressEventArgs>(zip_SaveProgress);
                zip.Save(_outputFile);
                UpdateProgressCounter(100);
                Console.WriteLine();

                zip.Dispose();
            }

            Trace.TraceEvent(TraceEventType.Information, 0, "IWD packing finished successfully");
        }

        #region Images
        List<string> GetImageFiles()
        {
            // file names...
            List<string> filesInFF = GetImageFileNames();
            List<string> includeFiles = GetIncludeExcludeFiles(_imagesInclude, ZoneCSVFileType.Image);
            List<string> excludeFiles = GetIncludeExcludeFiles(_imagesExclude, ZoneCSVFileType.Image);

            if (!String.IsNullOrEmpty(_imagesIncludeRegex))
            {
                List<string> includeRegexFiles = GetIncludeRegexFiles(_imagesDir, _imagesIncludeRegex, new string[] { ".iwd" });
                filesInFF.AddRange(includeRegexFiles);
            }

            filesInFF.AddRange(includeFiles);

            List<string> finalFileNames = new List<string>(filesInFF.Except(excludeFiles).Distinct(StringComparer.OrdinalIgnoreCase));

            List<string> finalExcludedFiles = new List<string>();
            if (!String.IsNullOrEmpty(_imagesExcludeRegex))
            {
                finalExcludedFiles = finalFileNames.ToList();
                finalFileNames = ExcludeRegexFiles(finalFileNames, _imagesExcludeRegex);
                finalExcludedFiles = new List<string>(finalExcludedFiles.Except(finalFileNames));
            }

            // file paths...
            List<string> imageFiles = GetAvailableFiles(finalFileNames, _imagesDir, ".iwi");
            if (_debugUnusedInDirs)
                DebugUnused(imageFiles, _imagesDir, new string[] { "*.iwi" }, finalExcludedFiles, false);

            return imageFiles;
        }

        List<string> GetImageFileNames()
        {
            List<string> fileNames = new List<string>();
            List<ZoneCSVFile> csvFiles = GetZoneCSVFiles(@"zone_source\english\assetlist", _includeCodImages);

            foreach (ZoneCSVFile csv in csvFiles)
            {
                csv.Read();
                fileNames.AddRange(csv.GetFileList(ZoneCSVFileType.Image));
            }

            //fileNames.AddRange(GetImageFileNamesFromMaterials());

            return fileNames;
        }
        /*
        List<string> GetImageFileNamesFromMaterials()
        {
            List<string> imageNames = new List<string>();
            List<string> materialNames = new List<string>();
            List<ZoneCSVFile> csvFiles = GetZoneCSVFiles(@"zone_source");

            foreach (ZoneCSVFile csv in csvFiles)
            {
                csv.Read();
                materialNames.AddRange(csv.GetFileList(ZoneCSVFileType.Material));
            }

            string materialSearchDir = Path.Combine(_gameDir, @"raw\materials");
            string materialPath;
            foreach (string str in materialNames)
            {
                materialPath = Path.Combine(materialSearchDir, str);
                if (File.Exists(materialPath))
                {
                    MaterialAsset m = new MaterialAsset(materialPath);
                    if (!String.IsNullOrEmpty(m.ColorMap))
                        imageNames.Add(m.ColorMap);

                    if (!String.IsNullOrEmpty(m.NormalMap))
                        imageNames.Add(m.NormalMap);

                    if (!String.IsNullOrEmpty(m.SpecularMap))
                        imageNames.Add(m.SpecularMap);
                }
            }

            return imageNames;            
        }
        */
        #endregion

        #region Sounds
        List<string> GetSoundFiles()
        {
            // file names...
            List<string> filesInFF = GetSoundFileNames();
            List<string> includeFiles = GetIncludeExcludeFiles(_soundsInclude, ZoneCSVFileType.LoadedSound);
            List<string> excludeFiles = GetIncludeExcludeFiles(_soundsExclude, ZoneCSVFileType.LoadedSound);

            if (!String.IsNullOrEmpty(_soundsIncludeRegex))
            {
                List<string> includeRegexFiles = GetIncludeRegexFiles(_soundsDir, _soundsIncludeRegex, new string[] { ".wav", ".mp3" });
                filesInFF.AddRange(includeRegexFiles);
            }

            filesInFF.AddRange(includeFiles);

            List<string> finalFileNames = new List<string>(filesInFF.Except(excludeFiles, StringComparer.OrdinalIgnoreCase).Distinct(StringComparer.OrdinalIgnoreCase));

            List<string> finalExcludedFiles = new List<string>();
            if (!String.IsNullOrEmpty(_soundsExcludeRegex))
            {
                finalExcludedFiles = finalFileNames.ToList();
                finalFileNames = ExcludeRegexFiles(finalFileNames, _soundsExcludeRegex);
                finalExcludedFiles = new List<string>(finalExcludedFiles.Except(finalFileNames, StringComparer.OrdinalIgnoreCase));
            }

            // file paths...
            List<string> soundFiles = GetAvailableFiles(finalFileNames, _soundsDir);
            if (_debugUnusedInDirs)
                DebugUnused(soundFiles, _soundsDir, new string[] { "*.wav", "*.mp3" }, finalExcludedFiles, true);

            return soundFiles;
        }

        List<string> GetSoundFileNames()
        {
            List<string> fileNames = new List<string>();
            List<ZoneCSVFile> csvFiles = GetZoneCSVFiles(@"zone_source", _includeCodSounds);

            foreach (ZoneCSVFile csv in csvFiles)
            {
                csv.Read();
                fileNames.AddRange(csv.GetFileList(ZoneCSVFileType.Sound));
            }

            return GetSoundFileNamesFromSoundAliases(fileNames);
        }

        List<string> GetSoundFileNamesFromSoundAliases(List<string> soundAliaseCSVs)
        {
            List<string> files = new List<string>();

            string searchDir = Path.Combine(_gameDir, "raw", "soundaliases");
            foreach (string soundAliasCSV in soundAliaseCSVs)
            {
                SoundAliasCSVFile csv = new SoundAliasCSVFile(searchDir, soundAliasCSV);
                csv.Read();
                files.AddRange(csv.GetSoundFileList());
            }

            return files;
        }
        #endregion

        #region Weapons
        List<string> GetWeaponFiles()
        {
            // file names...
            List<string> filesInFF = GetWeaponFileNames();
            List<string> includeFiles = GetIncludeExcludeFiles(_weaponsInclude, ZoneCSVFileType.Weapon);
            List<string> excludeFiles = GetIncludeExcludeFiles(_weaponsExclude, ZoneCSVFileType.Weapon);

            if (!String.IsNullOrEmpty(_weaponsIncludeRegex))
            {
                List<string> includeRegexFiles = GetIncludeRegexFiles(_weaponsDir, _weaponsIncludeRegex, new string[] { "" });
                filesInFF.AddRange(includeRegexFiles);
            }

            filesInFF.AddRange(includeFiles);

            List<string> finalFileNames = new List<string>(filesInFF.Except(excludeFiles).Distinct(StringComparer.OrdinalIgnoreCase));

            List<string> finalExcludedFiles = new List<string>();
            if (!String.IsNullOrEmpty(_weaponsExcludeRegex))
            {
                finalExcludedFiles = finalFileNames.ToList();
                finalFileNames = ExcludeRegexFiles(finalFileNames, _weaponsExcludeRegex);
                finalExcludedFiles = new List<string>(finalExcludedFiles.Except(finalFileNames));
            }

            // file paths...
            List<string> weaponFiles = GetAvailableFiles(finalFileNames, _weaponsDir);
            if (_debugUnusedInDirs)
                DebugUnused(weaponFiles, _weaponsDir, new string[] { "*_mp" }, finalExcludedFiles, false);

            return weaponFiles;
        }

        List<string> GetWeaponFileNames()
        {
            List<string> fileNames = new List<string>();
            List<ZoneCSVFile> csvFiles = GetZoneCSVFiles(@"zone_source\english\assetlist", _includeCodWeapons);

            foreach (ZoneCSVFile csv in csvFiles)
            {
                csv.Read();
                fileNames.AddRange(csv.GetFileList(ZoneCSVFileType.Weapon));
            }

            //fileNames.AddRange(GetImageFileNamesFromMaterials());

            return fileNames;
        }
        #endregion

        List<string> GetIncludeExcludeFiles(List<string> filePaths, ZoneCSVFileType type)
        {
            List<string> files = new List<string>();
            foreach (string filePath in filePaths)
            {
                if (!File.Exists(filePath))
                    throw new ApplicationException("Could not find '" + filePath + "', check args.");

                string searchDir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                ZoneCSVFile csvFile = new ZoneCSVFile(searchDir, fileName);
                csvFile.Read();
                files.AddRange(csvFile.GetFileList(type));
            }
            return files;
        }

        List<string> GetIncludeRegexFiles(List<string> dirList, string regexPattern, string[] extensions)
        {
            List<string> finalFiles = new List<string>();
            foreach (string searchDir in dirList)
            {
                string[] files = Directory.GetFiles(searchDir, "*", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    if (!extensions.Contains(Path.GetExtension(file)))
                        continue;

                    string shortFileName = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)).Substring(searchDir.Length + 1);
                    if (Regex.IsMatch(shortFileName, regexPattern, RegexOptions.IgnoreCase))
                        finalFiles.Add(shortFileName);
                }
            }
            return finalFiles;
        }

        List<string> ExcludeRegexFiles(List<string> filesInFF, string regexPattern)
        {
            List<string> finalFiles = new List<string>(filesInFF.Count);
            foreach (string file in filesInFF)
            {
                if (!Regex.IsMatch(file, regexPattern, RegexOptions.IgnoreCase))
                    finalFiles.Add(file);
            }
            return finalFiles;
        }

        List<string> GetAvailableFiles(List<string> fileNames, List<string> searchDirs)
        {
            return GetAvailableFiles(fileNames, searchDirs, "");
        }

        List<string> GetAvailableFiles(List<string> fileNames, List<string> searchDirs, string ext)
        {
            List<string> filePaths = new List<string>();

            string fullPath;
            foreach (string fileName in fileNames)
            {
                foreach (string searchDir in searchDirs)
                {
                    fullPath = Path.Combine(searchDir, fileName + ext);
                    if (File.Exists(fullPath))
                        filePaths.Add(fullPath);
                }
            }

            return filePaths;
        }

        List<ZoneCSVFile> GetZoneCSVFiles(string localSearchDir, bool includeCodFFs)
        {
            List<ZoneCSVFile> files = new List<ZoneCSVFile>();
            string searchDir = Path.Combine(_gameDir, localSearchDir);

            string ffName = _ffName;
            if (!String.IsNullOrEmpty(ffName))
            {
                string ffPath = Path.Combine(searchDir, ffName + ".csv");
                if (!File.Exists(ffPath))
                    throw new ApplicationException("Could not find '" + ffPath + "', try to recompile FF.");

                files.Add(new ZoneCSVFile(searchDir, ffName));
            

                string ffLoadName = _ffName + "_load";
                string ffLoadPath = Path.Combine(searchDir, ffLoadName + ".csv");
                if (File.Exists(ffLoadPath))
                    files.Add(new ZoneCSVFile(searchDir, ffLoadName));
            }

            if (includeCodFFs)
            {
                string commonMpName = "common_mp";
                string commonMpPath = Path.Combine(searchDir, commonMpName + ".csv");
                if (!File.Exists(commonMpPath))
                    throw new ApplicationException("Could not find '" + commonMpPath + "', try to reinstall ModTools");

                files.Add(new ZoneCSVFile(searchDir, commonMpName));

                string codeMpName = "code_post_gfx_mp";
                string codeMpPath = Path.Combine(searchDir, codeMpName + ".csv");
                if (!File.Exists(codeMpPath))
                    throw new ApplicationException("Could not find '" + codeMpPath + "', try to reinstall ModTools");

                files.Add(new ZoneCSVFile(searchDir, codeMpName));

                string localizedCommonMpName = "localized_common_mp";
                string localizedCommonMpPath = Path.Combine(searchDir, localizedCommonMpName + ".csv");
                if (!File.Exists(localizedCommonMpPath))
                    throw new ApplicationException("Could not find '" + localizedCommonMpPath + "', try to reinstall ModTools");

                files.Add(new ZoneCSVFile(searchDir, localizedCommonMpName));

                string localizedCodeMpName = "localized_code_post_gfx_mp";
                string localizedCodeMpPath = Path.Combine(searchDir, localizedCodeMpName + ".csv");
                if (!File.Exists(localizedCodeMpPath))
                    throw new ApplicationException("Could not find '" + localizedCodeMpPath + "', try to reinstall ModTools");

                files.Add(new ZoneCSVFile(searchDir, localizedCodeMpName));
            }
            return files;
        }

        void DebugUnused(List<string> usedFiles, List<string> searchDirs, string[] searchPatterns, List<string> excludedFiles, bool useExtension)
        {
            Trace.TraceEvent(TraceEventType.Verbose, 0, "================");
            Trace.TraceEvent(TraceEventType.Verbose, 0, "List of unused assets...");
            int count = 0;
            foreach (string searchDir in searchDirs)
            {
                foreach (string searchPattern in searchPatterns)
                {
                    string[] files = Directory.GetFiles(searchDir, searchPattern, SearchOption.AllDirectories);
                    foreach (string file in files)
                    {
                        string shortFileName = file.Substring(searchDir.Length + 1);
                        if (!useExtension)
                            shortFileName = Path.Combine(Path.GetDirectoryName(shortFileName), Path.GetFileNameWithoutExtension(shortFileName));

                        if (!usedFiles.Contains(file, StringComparer.OrdinalIgnoreCase) && !excludedFiles.Contains(shortFileName, StringComparer.OrdinalIgnoreCase))
                        {
                            Trace.TraceEvent(TraceEventType.Verbose, 0, file);
                            count++;
                        }
                    }
                }
            }
            Trace.TraceEvent(TraceEventType.Verbose, 0, "Total count: " + count);
            Trace.TraceEvent(TraceEventType.Verbose, 0, "================");
            Trace.Flush();
        }
    }
}
