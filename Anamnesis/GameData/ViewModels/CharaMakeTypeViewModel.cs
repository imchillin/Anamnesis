// © Anamnesis.
// Licensed under the MIT license.

namespace Anamnesis.GameData.ViewModels
{
	using System.Collections.Generic;
	using System.Windows.Media;
	using Anamnesis.GameData.Sheets;
	using Anamnesis.Memory;
	using Lumina;
	using Lumina.Excel;

	public class CharaMakeTypeViewModel : ExcelRowViewModel<CharaMakeType>, ICharaMakeType
	{
		private List<ImageSource>? facialFeatureList;

		public CharaMakeTypeViewModel(uint key, ExcelSheet<CharaMakeType> sheet, GameData lumina)
			: base(key, sheet, lumina)
		{
		}

		public override string Name => this.Key.ToString();
		public Customize.Genders Gender => (Customize.Genders)this.Value.Gender;
		public Customize.Races Race => (Customize.Races)this.Value.Race!.Row;
		public Customize.Tribes Tribe => (Customize.Tribes)this.Value.Tribe!.Row;

		public IEnumerable<ImageSource> FacialFeatures
		{
			get
			{
				if (this.facialFeatureList == null)
				{
					this.facialFeatureList = new List<ImageSource>();

					foreach (uint iconId in this.Value.FacialFeatureOptions!)
					{
						ImageSource? img = this.lumina.GetImage(iconId);

						if (img == null)
							continue;

						this.facialFeatureList.Add(img);
					}
				}

				return this.facialFeatureList;
			}
		}
	}
}
