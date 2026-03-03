// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.Converters;

using Anamnesis.Actor.Refresh;
using System;
using System.Globalization;
using System.Windows.Data;

/// <summary>
/// Converts a <see cref="RefreshBlockedReason"/> to a localization key.
/// </summary>
[ValueConversion(typeof(RefreshBlockedReason), typeof(string))]
public class RefreshBlockedReasonToKeyConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is not RefreshBlockedReason reason)
			return "Character_WarningNoRefresher";

		return value switch
		{
			RefreshBlockedReason.WorldFrozen => "Character_WarningGposeWorldPosFrozen",
			RefreshBlockedReason.PoseEnabled => "Character_WarningPoseEnabled",
			RefreshBlockedReason.OverworldInGpose => "Character_WarningOverworldInGpose",
			RefreshBlockedReason.IntegrationDisabled => "Character_WarningNoRefresher",
			_ => "Character_WarningGenericUnavailable", // Always show a message to avoid empty warning boxes.
		};
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		=> throw new NotSupportedException();
}
