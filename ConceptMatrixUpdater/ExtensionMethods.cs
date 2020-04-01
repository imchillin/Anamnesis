using System;

namespace ConceptMatrixUpdater
{
	public static class ExtensionMethods
	{
		public static string VersionString(this Version v) => string.Format("v{0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);
	}
}
