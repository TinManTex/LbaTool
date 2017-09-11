using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace LbaTool
{
    class Program
    {
        private const string DefaultDictionaryFileName = "lba_dictionary.txt";
        private const string DefaultHashMatchOutputFileName = "lba hash matches.txt";

        static void Main(string[] args)
        {
            var hashManager = new HashManager();

            // Read hash dictionary
            if (File.Exists(DefaultDictionaryFileName))
            {
                hashManager.HashLookupTable = MakeHashLookupTableFromFile(DefaultDictionaryFileName);
            }

            List<string> files = new List<string>();
            bool outputHashes = false;
            foreach (var lbaPath in args)
            {
                if (lbaPath.ToLower() == "-outputhashes" || lbaPath.ToLower() == "-o")
                {
                    outputHashes = true;
                } else
                {
                if (File.Exists(lbaPath))
                {
                        files.Add(lbaPath);
                    } else
                    {
                        if (Directory.Exists(lbaPath)) {
                            var dirFiles = Directory.GetFiles(lbaPath, "*.*", SearchOption.AllDirectories);
                            foreach (var file in dirFiles)
                            {
                                files.Add(file);
                            }
                        }
                    }
                }
            }
            foreach (var lbaPath in files)
            {
                    // Read input file
                    string fileExtension = Path.GetExtension(lbaPath);
                    if (fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                    {
                        LbaFile lba = ReadFromXml(lbaPath);
                        WriteToBinary(lba, Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(lbaPath)) + ".lba");
                } else if (fileExtension.Equals(".lba", StringComparison.OrdinalIgnoreCase))
                    {
                    Console.WriteLine(lbaPath);
                        LbaFile lba = ReadFromBinary(lbaPath, hashManager);
                    if (outputHashes)
                    {
                        var locatorNamesUnique = new HashSet<string>();
                        var dataSetsUnique = new HashSet<string>();
                        var unknownUnique = new HashSet<string>();
                        foreach (var iLocator in lba.Locators) {
                            switch (lba.Type)
                            {
                                case LocatorType.Type0:
                                    var locatorT0 = iLocator as LocatorType0;

                                    break;
                                case LocatorType.Type2:
                                    var locatorT2 = iLocator as LocatorType2;
                                    
                                    locatorNamesUnique.Add(locatorT2.LocatorName.HashValue.ToString());
                                    dataSetsUnique.Add(locatorT2.DataSet.HashValue.ToString());
                                    break;
                                case LocatorType.Type3:
                                    var locatorT3 = iLocator as LocatorType3;
                                    locatorNamesUnique.Add(locatorT3.LocatorName.HashValue.ToString());
                                    dataSetsUnique.Add(locatorT3.DataSet.HashValue.ToString());
                                    //TODO: seperate?
                                    unknownUnique.Add(locatorT3.Unknown30.HashValue.ToString());
                                    unknownUnique.Add(locatorT3.Unknown31.HashValue.ToString());
                                    unknownUnique.Add(locatorT3.Unknown32.HashValue.ToString());
                                    unknownUnique.Add(locatorT3.Unknown33.HashValue.ToString());
                                    break;
                                default:
                                    break;
                            }
                        }

                        string fileDirectory = Path.GetDirectoryName(lbaPath);
                        if (locatorNamesUnique.Count > 0)
                        {
                            List<string> hashes = locatorNamesUnique.ToList<string>();
                            hashes.Sort();
                            string outputPath = Path.Combine(fileDirectory, string.Format("{0}_locatorNameHashes.txt", Path.GetFileName(lbaPath)));
                            File.WriteAllLines(outputPath, hashes.ToArray());
                        }
                        if (dataSetsUnique.Count > 0)
                        {
                            List<string> hashes = dataSetsUnique.ToList<string>();
                            hashes.Sort();
                            string outputPath = Path.Combine(fileDirectory, string.Format("{0}_dataSetHashes.txt", Path.GetFileName(lbaPath)));
                            File.WriteAllLines(outputPath, hashes.ToArray());
                        }
                        if (unknownUnique.Count > 0)
                        {
                            List<string> hashes = unknownUnique.ToList<string>();
                            hashes.Sort();
                            string outputPath = Path.Combine(fileDirectory, string.Format("{0}_unknownNameHashes.txt", Path.GetFileName(lbaPath)));
                            File.WriteAllLines(outputPath, hashes.ToArray());
                        }
                    } else
                    {
                        WriteToXml(lba, Path.GetFileNameWithoutExtension(lbaPath) + ".lba.xml");
                    }
                } else
                    {
                        throw new IOException("Unrecognized input type.");
                    }
                }

            // Write hash matches output
            //WriteHashMatchesToFile(DefaultHashMatchOutputFileName, hashManager);
        }

        public static void WriteToBinary(LbaFile lba, string path)
        {
            using (BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate)))
            {
                lba.Write(writer);
            }
        }

        public static LbaFile ReadFromBinary(string path, HashManager hashManager)
        {
            LbaFile lba = new LbaFile();
            using (BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open)))
            {
                lba.Read(reader, hashManager);
            }
            return lba;
        }

        public static void WriteToXml(LbaFile lba, string path)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() {
                Encoding = Encoding.UTF8,
                Indent = true
            };
            using (var writer = XmlWriter.Create(path, xmlWriterSettings))
            {
                lba.WriteXml(writer);
            }
        }

        public static LbaFile ReadFromXml(string path)
        {
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings {
                IgnoreWhitespace = true
            };

            LbaFile lba = new LbaFile();
            using (var reader = XmlReader.Create(path, xmlReaderSettings))
            {
                lba.ReadXml(reader);
            }
            return lba;
        }

        /// <summary>
        /// Opens a file containing one string per line, hashes each string, and adds each pair to a lookup table.
        /// </summary>
        static Dictionary<uint, string> MakeHashLookupTableFromFile(string path)
        {
            ConcurrentDictionary<uint, string> table = new ConcurrentDictionary<uint, string>();

            // Read file
            List<string> stringLiterals = new List<string>();
            using (StreamReader file = new StreamReader(path))
            {
                // TODO multi-thread
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    stringLiterals.Add(line);
                }
            }

            // Hash entries
            Parallel.ForEach(stringLiterals, delegate (string entry) {
                uint hash = HashManager.HashString(entry);
                table.TryAdd(hash, entry);
            });

            // Return lookup table
            return new Dictionary<uint, string>(table);
        }

        /// <summary>
        /// Outputs all hash matched strings to a file.
        /// </summary>
        static void WriteHashMatchesToFile(string path, HashManager hashManager)
        {
            using (StreamWriter file = new StreamWriter(path))
            {
                foreach (var entry in hashManager.UsedHashes)
                {
                    file.WriteLine(entry.Value);
                }
            }
        }
    }
}
