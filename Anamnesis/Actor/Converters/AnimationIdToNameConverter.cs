﻿// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Actor.Converters;

using Anamnesis.GameData.Excel;
using Anamnesis.Services;
using System;
using System.Globalization;
using System.Windows.Data;

[ValueConversion(typeof(uint), typeof(string))]
public class AnimationIdToNameConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		string animationName;

		try
		{
			ushort animId = (ushort)value;
			ActionTimeline timeline = GameDataService.ActionTimelines.GetRow(animId);
			animationName = timeline.Key ?? LocalizationService.GetString("Character_Action_NoAnimation");
		}
		catch
		{
			animationName = LocalizationService.GetString("Character_Action_UnknownAnimation");
		}

		return animationName;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
