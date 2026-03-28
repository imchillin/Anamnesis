// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Utilities;

using Anamnesis.Actor.Pages;
using Anamnesis.Actor.Posing;
using Anamnesis.Files;
using Anamnesis.GameData;
using Anamnesis.Memory;
using Anamnesis.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

public static class ExpressionPoseLibraryGenerator
{
	private static volatile bool s_isGenerating = false;
	private static volatile bool s_cancelRequested = false;

	public static async Task GenerateLibraryAsync(ObjectHandle<ActorMemory> actorHandle)
	{
		if (s_isGenerating)
		{
			s_cancelRequested = true;
			Log.Information("Cancellation of expression pose library generation requested...");
			return;
		}

		if (actorHandle == null || !actorHandle.IsValid)
		{
			Log.Warning("Invalid actor handle provided to ExpressionPoseLibraryGenerator.");
			return;
		}

		s_isGenerating = true;
		s_cancelRequested = false;

		try
		{
			if (Application.Current != null)
			{
				await Application.Current.Dispatcher.InvokeAsync(() =>
				{
					_ = new PosePage();
					PosePage.WorkQueue.Value.Enabled = true;
				});
			}

			await actorHandle.DoAsync(async actor =>
			{
				if (actor.DrawData?.Customize == null || actor.Animation == null || actor.Animation.AnimationIds == null || actor.Animation.Speeds == null)
				{
					Log.Warning("Invalid actor state for generating expression pose library.");
					return;
				}

				string baseDir = Path.Combine(FileService.ParseToFilePath(FileService.DefaultPoseDirectory.Path), "Standard Facial Expressions");
				Directory.CreateDirectory(baseDir);

				Log.Information($"Starting expression pose library generation at {baseDir}");

				// Gather facial animations. Found on the facial slot (2) and typically prefixed with "face"
				var expressions = GameDataService.Emotes
					.Where(e => e.RowId != 0 && e.EmoteCategoryRowId == 3)
					.ToList();

				if (expressions.Count == 0)
				{
					Log.Warning("No facial expressions found in ActionTimelines.");
					return;
				}

				bool prevSpeedControl = AnimationService.Instance.SpeedControlEnabled;
				bool prevAutoRefresh = actor.AutomaticRefreshEnabled;
				bool prevLinkSpeeds = actor.Animation.LinkSpeeds;

				actor.AutomaticRefreshEnabled = false;
				actor.Animation.LinkSpeeds = false;
				AnimationService.Instance.SpeedControlEnabled = true;

				try
				{
					foreach (DataPaths dataPath in Enum.GetValues<DataPaths>())
					{
						if (dataPath.ToString().EndsWith("NPC"))
							continue;

						if (!TryGetRaceGenderTribe(dataPath, out ActorCustomizeMemory.Races race, out ActorCustomizeMemory.Genders gender, out ActorCustomizeMemory.Tribes[] tribes))
							continue;

						if (!CustomizeOptionsCache.ValidFaceIds.TryGetValue(dataPath, out List<byte>? headOptions) || headOptions == null)
							continue;

						foreach (ActorCustomizeMemory.Tribes tribe in tribes)
						{
							foreach (byte head in headOptions)
							{
								Log.Information($"Generating for {race} {gender} {tribe} Face {head}...");

								actor.DrawData.Customize.Race = race;
								actor.DrawData.Customize.Gender = gender;
								actor.DrawData.Customize.Tribe = tribe;
								actor.DrawData.Customize.Head = head;

								// Force refresh and wait for draw
								await actor.Refresh(forceReload: true);
								SkeletonEntity? skeleton = await TryGetSkeletonAsync(actorHandle);
								if (skeleton == null || skeleton.GetBone("j_kao") == null)
								{
									Log.Warning($"Skeleton creation failed or no face bones found after retries, skipping Face {head}.");
									continue;
								}

								AnimationService.Instance.ApplyIdle(actor);

								foreach (var expression in expressions)
								{
									if (s_cancelRequested)
										return;

									string dirPath = Path.Combine(baseDir, $"{race}", $"{gender}", $"{tribe}", $"Face {head}");
									Directory.CreateDirectory(dirPath);

									var savePath = new FileInfo(Path.Combine(dirPath, $"{expression.Name}.pose"));

									// Unfreeze the facial slot before blending
									actor.Animation.LinkSpeeds = false;
									if (actor.Animation.Speeds != null)
									{
										for (int i = 0; i < actor.Animation.Speeds.Length; i++)
											actor.Animation.Speeds[i].Value = 1.0f;
									}

									// Blend in the facial expression
									actor.Animation.BaseOverride = expression.LoopTimeline!.Value.AnimationId;
									await Task.Delay(1000);

									// Freeze the animation
									if (actor.Animation.Speeds != null)
									{
										for (int i = 0; i < actor.Animation.Speeds.Length; i++)
											actor.Animation.Speeds[i].Value = 0.0f;
									}

									await PosePage.WorkQueue.Value.Enqueue(() =>
									{
										var skeletonMemory = actor.ModelObject?.Skeleton;
										if (skeletonMemory != null && skeletonMemory.Address != IntPtr.Zero)
										{
											try
											{
												skeletonMemory.Synchronize();
											}
											catch (Exception ex)
											{
												Log.Verbose(ex, "Failed to sync skeleton during expression generation.");
											}
										}

										skeleton.ReadTransforms();
									});

									await PosePage.WorkQueue.Value.Enqueue(() =>
									{
										skeleton.SelectHead();

										// Only save face bones back to create a pure facial bone/expression pose mask file
										HashSet<string> faceBones = skeleton.SelectedBones.Select(b => b.Name).ToHashSet();

										// Clear the selection so we don't accidentally leak state
										skeleton.ClearSelection();

										if (faceBones.Count > 0)
										{
											var pose = new PoseFile();
											pose.WriteToFile(actorHandle, skeleton, faceBones);

											using var stream = new FileStream(savePath.FullName, FileMode.Create);
											pose.Serialize(stream);
										}
										else
										{
											Log.Warning($"No face bones found for {expression.Name} ({race} {gender} {tribe} Face {head})");
										}
									});

									actor.Animation.BaseOverride = 0;
								}
							}
						}
					}

					Log.Information("Facial expression pose library generation complete.");
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed while generating expression pose library.");
				}
				finally
				{
					actor.Animation.BaseOverride = 0;
					AnimationService.Instance.ResetAnimationOverride(actor);

					AnimationService.Instance.SpeedControlEnabled = prevSpeedControl;
					if (actor.Animation != null)
						actor.Animation.LinkSpeeds = prevLinkSpeeds;

					actor.AutomaticRefreshEnabled = prevAutoRefresh;
					await actor.Refresh();
				}
			});
		}
		finally
		{
			s_isGenerating = false;
			s_cancelRequested = false;
		}
	}

	private static async Task<SkeletonEntity?> TryGetSkeletonAsync(ObjectHandle<ActorMemory> actorHandle, int maxRetries = 50)
	{
		SkeletonEntity? skeleton = null;
		int retries = maxRetries;

		while (retries > 0)
		{
			await Task.Delay(100);

			await PosePage.WorkQueue.Value.Enqueue(() =>
			{
				try
				{
					skeleton = new SkeletonEntity(actorHandle);
				}
				catch (ArgumentOutOfRangeException)
				{
					skeleton = null;
				}
				catch (Exception ex)
				{
					Log.Error(ex, "Failed to create skeleton during expression library generation.");
					skeleton = null;
				}
			});

			if (skeleton != null && skeleton.GetBone("j_kao") != null)
				return skeleton;

			retries--;
		}

		// Retries exhausted
		return null;
	}

	private static bool TryGetRaceGenderTribe(DataPaths path, out ActorCustomizeMemory.Races race, out ActorCustomizeMemory.Genders gender, out ActorCustomizeMemory.Tribes[] tribes)
	{
		(ActorCustomizeMemory.Races Race, ActorCustomizeMemory.Genders Gender, ActorCustomizeMemory.Tribes[] Tribes) result = path switch
		{
			DataPaths.MidlanderMasculine => (ActorCustomizeMemory.Races.Hyur, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Midlander]),
			DataPaths.MidlanderFeminine => (ActorCustomizeMemory.Races.Hyur, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Midlander]),
			DataPaths.HighlanderMasculine => (ActorCustomizeMemory.Races.Hyur, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Highlander]),
			DataPaths.HighlanderFeminine => (ActorCustomizeMemory.Races.Hyur, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Highlander]),
			DataPaths.ElezenMasculine => (ActorCustomizeMemory.Races.Elezen, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Tribes.Duskwight]),
			DataPaths.ElezenFeminine => (ActorCustomizeMemory.Races.Elezen, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Wildwood, ActorCustomizeMemory.Tribes.Duskwight]),
			DataPaths.MiqoteMasculine => (ActorCustomizeMemory.Races.Miqote, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Tribes.KeeperOfTheMoon]),
			DataPaths.MiqoteFeminine => (ActorCustomizeMemory.Races.Miqote, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.SeekerOfTheSun, ActorCustomizeMemory.Tribes.KeeperOfTheMoon]),
			DataPaths.RoegadynMasculine => (ActorCustomizeMemory.Races.Roegadyn, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Tribes.Hellsguard]),
			DataPaths.RoegadynFeminine => (ActorCustomizeMemory.Races.Roegadyn, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.SeaWolf, ActorCustomizeMemory.Tribes.Hellsguard]),
			DataPaths.LalafellMasculine => (ActorCustomizeMemory.Races.Lalafel, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Tribes.Dunesfolk]),
			DataPaths.LalafellFeminine => (ActorCustomizeMemory.Races.Lalafel, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Plainsfolk, ActorCustomizeMemory.Tribes.Dunesfolk]),
			DataPaths.AuRaMasculine => (ActorCustomizeMemory.Races.AuRa, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Tribes.Xaela]),
			DataPaths.AuRaFeminine => (ActorCustomizeMemory.Races.AuRa, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Raen, ActorCustomizeMemory.Tribes.Xaela]),
			DataPaths.HrothgarMasculine => (ActorCustomizeMemory.Races.Hrothgar, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Tribes.TheLost]),
			DataPaths.HrothgarFeminine => (ActorCustomizeMemory.Races.Hrothgar, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Helions, ActorCustomizeMemory.Tribes.TheLost]),
			DataPaths.VieraMasculine => (ActorCustomizeMemory.Races.Viera, ActorCustomizeMemory.Genders.Masculine, [ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Tribes.Veena]),
			DataPaths.VieraFeminine => (ActorCustomizeMemory.Races.Viera, ActorCustomizeMemory.Genders.Feminine, [ActorCustomizeMemory.Tribes.Rava, ActorCustomizeMemory.Tribes.Veena]),
			_ => (default, default, []),
		};

		(race, gender, tribes) = result;
		return tribes.Length > 0;
	}
}
