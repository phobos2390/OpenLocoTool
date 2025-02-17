﻿
using System.Text.Json.Serialization;

namespace OpenLocoToolGui
{
	public class GuiSettings
	{
		public string ObjDataDirectory { get; set; }

		public string DataDirectory { get; set; }

		public string PaletteFile { get; set; } = "palette.png";

		public string IndexFileName { get; set; } = "objectIndex.json";

		public string G1DatFileName { get; set; } = "g1.DAT";

		[JsonIgnore]
		public string IndexFilePath => Path.Combine(ObjDataDirectory, IndexFileName);

		[JsonIgnore]
		public string G1Path => Path.Combine(DataDirectory, G1DatFileName);
	}
}
