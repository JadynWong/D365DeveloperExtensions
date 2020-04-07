﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Collections.Generic;

namespace CrmDeveloperExtensions2.Core.Vs
{
    public static class SolutionWorker
    {
        public static void SetBuildConfigurationOff(SolutionConfigurations buildConfigurations, string projectName)
        {
            foreach (SolutionConfiguration buildConfiguration in buildConfigurations)
            {
                //Localize these?
                if (buildConfiguration.Name != "Debug" && buildConfiguration.Name != "Release")
                    continue;

                SolutionContexts contexts = buildConfiguration.SolutionContexts;
                foreach (SolutionContext solutionContext in contexts)
                {
                    if (solutionContext.ProjectName == projectName)
                        solutionContext.ShouldBuild = false;
                }
            }
        }

        public static IList<Project> GetProjects()
        {
            if (!(Package.GetGlobalService(typeof(DTE)) is DTE dte))
                return null;

            Projects projects = dte.Solution.Projects;
            List<Project> list = new List<Project>();
            var item = projects.GetEnumerator();
            while (item.MoveNext())
            {
                Project project = (Project)item.Current;
                if (project == null)
                    continue;

                if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    list.AddRange(GetSolutionFolderProjects(project));
                else
                    list.Add(project);
            }

            return list;
        }

        private static IEnumerable<Project> GetSolutionFolderProjects(Project solutionFolder)
        {
            List<Project> list = new List<Project>();
            for (var i = 1; i <= solutionFolder.ProjectItems.Count; i++)
            {
                var subProject = solutionFolder.ProjectItems.Item(i).SubProject;
                if (subProject == null)
                    continue;

                // If this is another solution folder, do a recursive call, otherwise add
                if (subProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
                    list.AddRange(GetSolutionFolderProjects(subProject));
                else
                    list.Add(subProject);
            }
            return list;
        }
    }
}