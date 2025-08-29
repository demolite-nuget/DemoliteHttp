using System.IO;
using Demolite.Http.Config;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;

class Build : NukeBuild
{
	/// Support plugins are available for:
	///   - JetBrains ReSharper        https://nuke.build/resharper
	///   - JetBrains Rider            https://nuke.build/rider
	///   - Microsoft VisualStudio     https://nuke.build/visualstudio
	///   - Microsoft VSCode           https://nuke.build/vscode
	public static int Main() => Execute<Build>(x => x.Publish);

	[Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
	readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

	[Solution(GenerateProjects = true)] 
	readonly Solution Solution;
	
	static AbsolutePath ReleaseDirectory => RootDirectory / .. / .. / "release";
	
	static AbsolutePath PushDirectory => RootDirectory / .. / .. / "push";

	Target Clean => t => t
		.Before(Restore)
		.Executes(() =>
			{
				Directory.Delete(PushDirectory, true);
				Directory.CreateDirectory(PushDirectory);
				return DotNetTasks.DotNetClean(s => s.SetProject(Solution));
			}
		);

	Target Restore => t => t
		.DependsOn(Clean)
		.Executes(() => DotNetTasks.DotNetRestore(s => s.SetProjectFile(Solution)));

	Target Compile => t => t
		.DependsOn(Restore)
		.Executes(() =>
			DotNetTasks.DotNetBuild(s => s.SetProjectFile(Solution).SetConfiguration(Configuration))
		);

	Target Test => t => t
		.DependsOn(Compile)
		.Executes(() => DotNetTasks.DotNetTest(s => s.SetProjectFile(Solution).SetConfiguration(Configuration)));

	Target Pack => t => t
		.DependsOn(Test)
		.Executes(() =>
			{
				DotNetTasks.DotNetPack(s
					=> s
						.SetProject(Solution.Demolite_Http)
						.SetConfiguration(Configuration)
						.SetVersion(AppVersion.Version)
						.SetOutputDirectory(PushDirectory)
				);
			}
		);

	Target Publish => t => t
		.DependsOn(Pack)
		.Executes(() =>
			{
				foreach (var package in Directory.GetFiles(PushDirectory, "*.nupkg"))
				{
					if (IsLocalBuild)
					{
						DotNetTasks.DotNetNuGetPush(s => s.SetTargetPath(package).SetSource("C:/NuGet"));
						File.Copy(package, Path.Combine(ReleaseDirectory, Path.GetFileName(package)));
					}
					
				}
			}
		);
}