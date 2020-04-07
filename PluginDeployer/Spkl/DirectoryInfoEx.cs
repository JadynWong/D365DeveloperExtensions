﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginDeployer.Spkl
{
    public static class DirectoryEx
    {
        public static string GetApplicationDirectory()
        {
            string path = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;

            return path;
        }

        public static string Search(string path, string search)
        {
            try
            {
                foreach (string f in Directory.GetFiles(path, search, SearchOption.AllDirectories))
                {
                    return f;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static List<string> Search(string path, string search,List<string> matches)
        {

            if (matches==null)
            {
                matches = new List<string>();
            }
            try
            {
                foreach (string f in search.Split('|').SelectMany(i=>Directory.GetFiles(path, i,SearchOption.AllDirectories)))
                {
                    matches.Add(f);
                }

                return matches;
            }
            catch
            {
                return null;
            }
        }
    }
}
