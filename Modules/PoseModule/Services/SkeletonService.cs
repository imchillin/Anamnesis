// Concept Matrix 3.
// Licensed under the MIT license.

namespace ConceptMatrix.PoseModule
{
	using System;
	using System.Collections.Generic;
	using System.Threading.Tasks;
	using ConceptMatrix;
	using ConceptMatrix.Memory;
	using ConceptMatrix.Memory.Offsets;

	public class SkeletonService : IService
	{
		public const string SkeletonsDirectory = "Modules/Pose/Skeletons/";
		private Dictionary<Appearance.Races, Dictionary<string, Bone>> precachedBones;

		public async Task Initialize()
		{
			this.precachedBones = new Dictionary<Appearance.Races, Dictionary<string, Bone>>();
			foreach (Appearance.Races race in Enum.GetValues(typeof(Appearance.Races)))
			{
				this.precachedBones[race] = await this.Load(race);
			}
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
			if (this.precachedBones.ContainsKey(race))
			{
				return this.precachedBones[race];
			}

			try
			{
				IFileService fileService = Services.Get<IFileService>();
				Dictionary<string, Bone> bones = new Dictionary<string, Bone>();

				string fileName = race.ToString();

				while (!string.IsNullOrEmpty(fileName))
				{
					SkeletonFile file = await fileService.Open<SkeletonFile>(SkeletonFile.File, SkeletonsDirectory + fileName);

					if (file == null)
						throw new Exception("Failed to load skeleton file: " + fileName);

					foreach ((string name, Bone bone) in file.Bones)
					{
						// If we already have this bone, then its being overwritten by a higher file.
						if (bones.ContainsKey(name))
							continue;

						bone.Offsets.Name = name;

						bones[name] = bone;
					}

					// Get the based on value to load bones from.
					fileName = file.BasedOn;
				}

				return bones;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to load skeleton for race: " + race, ex);
			}
		}

		[Serializable]
		public class Bone
		{
			public Offset<Transform> Offsets { get; set; }
			public string Parent { get; set; }
			public string Group { get; set; }
		}

		[Serializable]
		private class SkeletonFile : FileBase
		{
			public static readonly FileType File = new FileType(".json", "Skeleton file", typeof(SkeletonFile));

			public override FileType Type => File;

			public string BasedOn { get; set; }
			public Dictionary<string, Bone> Bones { get; set; }
		}
	}
}
