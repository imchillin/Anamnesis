// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Injection.Offsets;

	public class SkeletonService : IService
	{
		public const string SkeletonsDirectory = "Modules/Skeletons/";

		public async Task Initialize()
		{
			Dictionary<string, Bone> testSkel = await this.Load(Appearance.Races.Hyur);
			Log.Write("got " + testSkel.Count + " bones");
		}

		public Task Shutdown()
		{
			return Task.CompletedTask;
		}

		public Task Start()
		{
			return Task.CompletedTask;
		}

		// Load the skeleton file and its BasedOn values
		public async Task<Dictionary<string, Bone>> Load(Appearance.Races race)
		{
			IFileService fileService = Services.Get<IFileService>();
			Dictionary<string, Bone> bones = new Dictionary<string, Bone>();

			string fileName = race.ToString();

			while (!string.IsNullOrEmpty(fileName))
			{
				SkeletonFile file = await fileService.Open<SkeletonFile>(SkeletonFile.File, SkeletonsDirectory + fileName);
				foreach ((string name, Bone bone) in file.Bones)
				{
					// If we already have this bone, then its being overwritten by a higher file.
					if (bones.ContainsKey(name))
						continue;

					bones[name] = bone;
				}

				// Get the based on value to load bones from.
				fileName = file.BasedOn;
			}

			return bones;
		}

		[Serializable]
		public class Bone
		{
			public Offset<Transform> Offsets { get; set; }
			public string Parent { get; set; }
		}

		[Serializable]
		private class SkeletonFile : FileBase
		{
			public static readonly FileType File = new FileType(".json", "Skeleton file", typeof(SkeletonFile));

			public string BasedOn { get; set; }
			public Dictionary<string, Bone> Bones { get; set; }

			public override FileType GetFileType()
			{
				return File;
			}
		}
	}
}
