using NAudio.Wave;
using OpenLocoTool;
using OpenLocoTool.DatFileParsing;
using OpenLocoTool.Headers;
using OpenLocoTool.Objects;
using OpenLocoToolCommon;
using System.Drawing.Imaging;

namespace OpenLocoToolGui
{
	// how this program works
	//
	// 1. open UI, no loading
	// 2. user selects a directory
	// 3. if no open-loco-tool index file exists, open-loco-tool fully loads all dat files in directory, creates an index and writes it to `objectIndex.json` in that folder. this is SLOW (currently)
	// 4. next time that directory is opened, the index is read instead of loading all files. this is FAST

	public partial class MainForm : Form
	{
		readonly MainFormModel model;
		readonly ILogger logger;

		// could use pgObject.SelectedObjectsChanged event, but we'll just do this for now
		public ILocoObject? CurrentUIObject
		{
			get => currentUIObject;
			set
			{
				currentUIObject = value;
				RefreshObjectUI();
			}
		}
		ILocoObject? currentUIObject;

		IList<PictureBox> CurrentUIImages
		{
			get => currentUIImages;
			set
			{
				currentUIImages = value;
				CurrentUIImagePageNumber = 0;
			}
		}
		IList<PictureBox> currentUIImages = new List<PictureBox>();

		int CurrentUIImagePageNumber
		{
			get => currentUIImagePageNumber;
			set
			{
				currentUIImagePageNumber = value;
				var controls = GetPictureBoxesForPage(currentUIImagePageNumber);
				flpImageTable.SuspendLayout();
				flpImageTable.Controls.Clear();
				flpImageTable.Controls.AddRange(controls.ToArray());
				var pages = (CurrentUIImages.Count / imagesPerPage) + 1;
				tbCurrentPage.Text = $"Page ({currentUIImagePageNumber + 1} / {pages}) ";
				flpImageTable.ResumeLayout(true);
			}
		}
		int currentUIImagePageNumber;

		// DAT Dump viewer fields
		IList<Annotation> DATDumpAnnotations;
		readonly IDictionary<string, (int, int)> DATDumpAnnotationIdentifiers = new Dictionary<string, (int, int)>();
		readonly IDictionary<string, TreeNode> imageHeaderIndexToNode = new Dictionary<string, TreeNode>();
		readonly IDictionary<string, TreeNode> imageDataIndexToNode = new Dictionary<string, TreeNode>();
		const int bytesPerDumpLine = 32;
		const int addressStringSizeBytes = 8;
		const int addressStringSizePrependBytes = addressStringSizeBytes + 2;
		const int dumpWordSize = 4;
		readonly IDictionary<string, Action<string>> tvUniqueLoadValues = new Dictionary<string, Action<string>>();
		// End DAT Dump viewer fields

		const int imagesPerPage = 50;

		const string SettingsFile = "./settings.json";

		public MainForm()
		{
			InitializeComponent();

			logger = new Logger
			{
				Level = LogLevel.Debug2
			};

			model = new MainFormModel(logger, SettingsFile);
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			// can only do this after window handle has been created (so can't do in constructor)
			((Logger)logger).LogAdded += (s, e) => lbLogs.Invoke(() => lbLogs.Items.Insert(0, e.Log.ToString()));

			// setup dark mode???
			//DarkModify(this);

			InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
		}

		readonly Color DarkModeBackColor = Color.FromArgb(31, 31, 31);
		readonly Color DarkModeForeColor = Color.White;

		// poor-mans dark mode
		void DarkModify(Control control)
		{
			foreach (Control c in control.Controls)
			{
				c.BackColor = DarkModeBackColor;
				c.ForeColor = DarkModeForeColor;

				DarkModify(c);
			}
		}

		void InitUI(bool vanillaOnly, string filter)
		{
			InitFileTreeView(vanillaOnly, filter);
			InitCategoryTreeView(vanillaOnly, filter);
		}

		bool LoadObjDataDirectory(string directory, bool useExistingIndex)
		{
			if (string.IsNullOrEmpty(directory))
			{
				logger.Warning("Invalid directory");
				return false;
			}

			var allFiles = Directory.GetFiles(directory, "*.dat", SearchOption.AllDirectories);
			using (var progressForm = new ProgressBarForm())
			{
				progressForm.Text = $"Indexing {allFiles.Length} files";
				var progress = new Progress<float>(f => progressForm.SetProgress((int)(f * 100)));
				// can probably use a task instead of a thread, but its good enough
				var thread = new Thread(() =>
				{
					model.LoadDirectory(directory, progress, useExistingIndex);
					progressForm.CloseForm();
				});
				thread.Start();
				progressForm.ShowDialog();
			}

			return true;
		}

		static Bitmap MakeOriginalLocoIcon(bool isOriginal)
		{
			var bitmap = new Bitmap(16, 16);
			if (isOriginal)
			{
				var g = Graphics.FromImage(bitmap);
				g.FillEllipse(Brushes.MediumSpringGreen, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
				g.Dispose();
			}

			return bitmap;
		}

		static ImageList MakeImageList()
		{
			var imageList = new ImageList();
			var blankImage = MakeOriginalLocoIcon(false);
			var originalImage = MakeOriginalLocoIcon(true);
			imageList.Images.Add(blankImage);
			imageList.Images.Add(originalImage);
			return imageList;
		}

		static void AddObjectNode(string key, string text, string objName, TreeView tv)
		{
			var imageIndex = OriginalObjects.Names.Contains(objName.Trim()) ? 1 : 0;
			_ = tv.Nodes.Add(key, text, imageIndex, imageIndex);
		}
		static void AddObjectNode(string key, string text, string objName, TreeNode tn)
		{
			var imageIndex = OriginalObjects.Names.Contains(objName.Trim()) ? 1 : 0;
			_ = tn.Nodes.Add(key, text, imageIndex, imageIndex);
		}

		void InitFileTreeView(bool vanillaOnly, string fileFilter)
		{
			tvFileTree.SuspendLayout();
			tvFileTree.Nodes.Clear();

			var filteredFiles = string.IsNullOrEmpty(fileFilter)
				? model.HeaderIndex
				: model.HeaderIndex.Where(hdr => hdr.Key.Contains(fileFilter, StringComparison.InvariantCultureIgnoreCase));

			filteredFiles = filteredFiles.Where(f => !vanillaOnly || OriginalObjects.Names.Contains(f.Value.Name.Trim()));

			tvFileTree.ImageList = MakeImageList();

			foreach (var obj in filteredFiles)
			{
				var relative = Path.GetRelativePath(model.Settings.ObjDataDirectory, obj.Key);
				AddObjectNode(obj.Key, relative, obj.Value.Name, tvFileTree);
			}

			tvFileTree.Sort();
			tvFileTree.ResumeLayout(true);
		}

		void InitCategoryTreeView(bool vanillaOnly, string fileFilter)
		{
			tvObjType.SuspendLayout();
			tvObjType.Nodes.Clear();

			var filteredFiles = string.IsNullOrEmpty(fileFilter)
				? model.HeaderIndex
				: model.HeaderIndex.Where(hdr => hdr.Key.Contains(fileFilter, StringComparison.InvariantCultureIgnoreCase));

			filteredFiles = filteredFiles.Where(f => !vanillaOnly || OriginalObjects.Names.Contains(f.Value.Name.Trim()));

			tvObjType.ImageList = MakeImageList();

			var nodesToAdd = new List<TreeNode>();
			foreach (var group in filteredFiles.GroupBy(kvp => kvp.Value.ObjectType))
			{
				var typeNode = new TreeNode(group.Key.ToString());
				if (group.Key != ObjectType.Vehicle)
				{
					foreach (var obj in group)
					{
						AddObjectNode(obj.Key, obj.Value.Name, obj.Value.Name, typeNode);
					}
				}
				else
				{
					var vehicleGroup = group.GroupBy(o => o.Value.VehicleType);
					foreach (var vehicleType in vehicleGroup)
					{
						var vehicleTypeNode = new TreeNode(vehicleType.Key.ToString());
						foreach (var veh in vehicleType)
						{
							AddObjectNode(veh.Key, veh.Value.Name, veh.Value.Name, vehicleTypeNode);
						}

						typeNode.Nodes.Add(vehicleTypeNode);
					}
				}

				nodesToAdd.Add(typeNode);
			}

			var objDataNode = new TreeNode("ObjData");

			tvObjType.Sort();
			tvObjType.Nodes.Add(objDataNode);

			if (File.Exists(model.Settings.G1Path))
			{
				var dataNode = new TreeNode("Data");

				objDataNode.Nodes.AddRange(nodesToAdd.ToArray());

				AddObjectNode(model.Settings.G1Path, "g1", "g1", dataNode);
				tvObjType.Nodes.Add(dataNode);

				model.LoadDataDirectory(model.Settings.DataDirectory);

				tvUniqueLoadValues["g1"] = LoadG1;
			}

			tvObjType.ResumeLayout(true);
		}

		// note: doesn't work atm
		private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (pgObject.SelectedObject is not ILocoObject obj)
			{
				return;
			}

			saveFileDialog1.InitialDirectory = model.Settings.ObjDataDirectory;
			saveFileDialog1.DefaultExt = "dat";
			saveFileDialog1.Filter = "Locomotion DAT files (.dat)|*.dat";
			if (saveFileDialog1.ShowDialog() == DialogResult.OK)
			{
				var filename = saveFileDialog1.FileName;

				try
				{
					var exists = File.Exists(filename);
					model.SaveFile(filename, obj);

					if (!exists)
					{
						// we made a new file (as opposed to overwriting an existing one) so lets update the UI to show it
						InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
					}

					MessageBox.Show($"File \"{filename}\" saved successfully");
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error saving \"{filename}\": " + ex.Message);
				}
			}
		}

		private void setObjectDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (objectDirBrowser.ShowDialog(this) == DialogResult.OK)
			{
				if (LoadObjDataDirectory(objectDirBrowser.SelectedPath, true))
				{
					InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
				}
			}
		}

		private void setDataDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (objectDirBrowser.ShowDialog(this) == DialogResult.OK)
			{
				if (model.LoadDataDirectory(objectDirBrowser.SelectedPath))
				{
					pgObject.SelectedObject = model.G1;
					var images = CreateImages(model.G1.G1Elements, model.Palette);
					CurrentUIImages = CreateImageControls(images).ToList();
				}
			}
		}

		IEnumerable<PictureBox> GetPictureBoxesForPage(int page) => CurrentUIImages.Skip(page * imagesPerPage).Take(imagesPerPage);

		private void recreateIndexToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (LoadObjDataDirectory(model.Settings.ObjDataDirectory, false))
			{
				InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
			}
		}

		void tbFileFilter_TextChanged(object sender, EventArgs e)
		{
			InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
		}

		void LoadDataDump(string path, bool isG1 = false)
		{
			if (File.Exists(path))
			{
				var byteList = File.ReadAllBytes(path);
				var resultingByteList = byteList;
				DATDumpAnnotations = isG1
					? ObjectAnnotator.AnnotateG1Data(byteList)
					: ObjectAnnotator.Annotate(byteList, out resultingByteList);

				var extraLine = resultingByteList.Length % bytesPerDumpLine;
				if (extraLine > 0)
				{
					extraLine = 1;
				}

				var dumpLines = resultingByteList
						.Select(b => string.Format("{0,2:X2}", b))
						.Chunk(dumpWordSize)
						.Select(c => string.Format("{0} ", string.Concat(c)))
						.Chunk(bytesPerDumpLine / dumpWordSize)
						.Zip(Enumerable.Range(0, (resultingByteList.Length / bytesPerDumpLine) + extraLine))
						.Select(l => string.Format("{0:X" + addressStringSizeBytes + "}: {1}", l.Second * bytesPerDumpLine, string.Concat(l.First))).ToArray();

				tvDATDumpAnnotations.SuspendLayout();
				tvDATDumpAnnotations.Nodes.Clear();
				var currentParent = new TreeNode();
				IDictionary<string, TreeNode> parents = new Dictionary<string, TreeNode>();

				foreach (var annotation in DATDumpAnnotations)
				{
					var constructAnnotationText = (Annotation annotation) => string.Format("{0} (0x{1:X}-0x{2:X})", annotation.Name, annotation.Start, annotation.End);
					var annotationText = constructAnnotationText(annotation);
					parents[annotationText] = new TreeNode(annotationText);
					DATDumpAnnotationIdentifiers[annotationText] = (annotation.Start, annotation.End);
					if (annotation.Parent == null)
					{
						tvDATDumpAnnotations.Nodes.Add(parents[constructAnnotationText(annotation)]);
					}
					else if (parents.ContainsKey(constructAnnotationText(annotation.Parent)))
					{
						var parentText = constructAnnotationText(annotation.Parent);
						parents[parentText].Nodes.Add(parents[annotationText]);

						if (annotation.Parent.Name == "Headers")
						{
							imageHeaderIndexToNode[annotation.Name] = parents[annotationText];
						}

						if (annotation.Parent.Name == "Images")
						{
							imageDataIndexToNode[annotation.Name] = parents[annotationText];
						}
					}
				}

				tvDATDumpAnnotations.ResumeLayout();
				rtbDATDumpView.Text = string.Join("\n", dumpLines);
			}
		}

		void LoadG1(string filename)
		{
			pgObject.SelectedObject = model.G1;
			var images = CreateImages(model.G1.G1Elements, model.Palette);
			CurrentUIImages = CreateImageControls(images).ToList();
			LoadDataDump(filename, true);
		}

		void tv_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e != null && e.Node != null && tvUniqueLoadValues.ContainsKey(e.Node.Text))
			{
				tvUniqueLoadValues[e.Node.Text].Invoke(e.Node.Name);
			}
			else
			{
				CurrentUIObject = model.LoadAndCacheObject(e.Node.Name);
				LoadDataDump(e.Node.Name);
			}
		}

		void CreateSounds(SoundObject soundObject)
		{
			flpImageTable.SuspendLayout();
			flpImageTable.Controls.Clear();

			var pcmHeader = soundObject.SoundObjectData.PcmHeader;

			var soundButton = new Button
			{
				Size = new Size(100, 100),
				Text = "Play sound",
			};

			soundButton.Click += (args, sender) =>
			{
				// do it asyncly to a) give user ui control and b) allow multiple sounds to play at once
				Task.Run(() =>
				{
					using (var ms = new MemoryStream(soundObject.RawPcmData))
					using (var rs = new RawSourceWaveStream(ms, new WaveFormat(pcmHeader.SamplesPerSecond, 16, pcmHeader.NumberChannels)))
					using (var wo = new WaveOutEvent())
					{
						wo.Init(rs);
						wo.Play();
						while (wo.PlaybackState == PlaybackState.Playing)
						{
							Thread.Sleep(50);
						}
					}
				});
			};

			flpImageTable.Controls.Add(soundButton);

			flpImageTable.ResumeLayout(true);
		}

		IEnumerable<PictureBox> CreateImageControls(IEnumerable<Bitmap> images)
		{
			// on these controls we could add a right_click handler to replace image with user-created one
			foreach (var img in images)
			{
				var pb = new PictureBox
				{
					Image = img,
					BorderStyle = BorderStyle.FixedSingle,
					SizeMode = PictureBoxSizeMode.AutoSize,
					ContextMenuStrip = imgContextMenu
				};

				yield return pb;
			}
		}

		IEnumerable<Bitmap> CreateImages(List<G1Element32> G1Elements, Color[] palette)
		{
			if (palette is null)
			{
				logger.Error("Palette was empty; please load a valid palette file");
				yield break;
			}

			for (var i = 0; i < G1Elements.Count; ++i)
			{
				var currElement = G1Elements[i];
				var imageData = currElement.ImageData;

				if (currElement.ImageData.Length == 0 || currElement.Flags.HasFlag(G1ElementFlags.IsR8G8B8Palette))
				{
					logger.Info($"skipped loading g1 element {i} with flags {currElement.Flags}");
					continue;
				}

				var dstImg = new Bitmap(currElement.Width, currElement.Height);
				var rect = new Rectangle(0, 0, currElement.Width, currElement.Height);
				var dstImgData = dstImg.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
				for (var y = 0; y < currElement.Height; ++y)
				{
					for (var x = 0; x < currElement.Width; ++x)
					{
						var paletteIndex = imageData[(y * currElement.Width) + x];

						// the issue with greyscale here is it isn't normalised so all heightmaps are really dark and hard to see
						//var colour = obj.Object is HillShapesObject
						//	? Color.FromArgb(paletteIndex, paletteIndex, paletteIndex) // for hillshapes, its just a heightmap so lets put it in greyscale
						//	: palette[paletteIndex];

						var colour = palette[paletteIndex];
						ImageHelpers.SetPixel(dstImgData, x, y, colour);
					}
				}

				dstImg.UnlockBits(dstImgData);
				yield return dstImg;
			}
		}

		void SelectNewPalette()
		{
			using (var openFileDialog = new OpenFileDialog())
			{
				openFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
				openFileDialog.Filter = "Palette Image Files(*.png)|*.png|All files (*.*)|*.*";
				openFileDialog.FilterIndex = 1;
				openFileDialog.RestoreDirectory = true;

				if (openFileDialog.ShowDialog() == DialogResult.OK)
				{
					model.PaletteFile = openFileDialog.FileName;
					RefreshObjectUI();
				}
			}
		}

		private void setPaletteToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SelectNewPalette();
		}

		private void RefreshObjectUI()
		{
			flpImageTable.SuspendLayout();
			flpImageTable.Controls.Clear();

			if (CurrentUIObject?.G1Elements != null && CurrentUIObject.G1Header != null && CurrentUIObject.G1Header.TotalSize != 0 && CurrentUIObject.G1Elements.Count != 0)
			{
				if (model.Palette is null)
				{
					MessageBox.Show("No palette file loaded - please load one from File -> Load Palette. You can use palette.png in the top level folder of this repo.");
					return;
					//SelectNewPalette();
				}

				var images = CreateImages(CurrentUIObject.G1Elements, model.Palette);
				CurrentUIImages = CreateImageControls(images).ToArray();
			}

			if (CurrentUIObject?.Object is SoundObject soundObject)
			{
				CreateSounds(soundObject);
			}

			flpImageTable.ResumeLayout(true);

			pgObject.SelectedObject = CurrentUIObject;
		}

		private void imgContextMenuSave_Click(object sender, EventArgs e)
		{
			if (imgContextMenu.SourceControl is PictureBox pb)
			{
				using (var saveFileDialog = new SaveFileDialog())
				{
					saveFileDialog.InitialDirectory = Directory.GetCurrentDirectory();
					saveFileDialog.Filter = "PNG Files(*.png)|*.png|All files (*.*)|*.*";
					saveFileDialog.FilterIndex = 1;
					saveFileDialog.RestoreDirectory = true;

					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						pb.Image.Save(saveFileDialog.FileName);
						logger.Info($"Saved image to {saveFileDialog.FileName}");
					}
				}
			}
		}

		private void btnPagePrevious_Click(object sender, EventArgs e)
		{
			CurrentUIImagePageNumber = Math.Max(CurrentUIImagePageNumber - 1, 0);
		}

		private void btnPageNext_Click(object sender, EventArgs e)
		{
			if (currentUIImages?.Count > 0)
			{
				CurrentUIImagePageNumber = Math.Min(CurrentUIImagePageNumber + 1, CurrentUIImages.Count / imagesPerPage);
			}
		}

		private void dataDumpAnnotations_AfterSelect(object sender, TreeViewEventArgs e)
		{
			var dumpPositionToRTBPosition = (int position) => rtbDATDumpView.GetFirstCharIndexFromLine(
				position / bytesPerDumpLine)
				+ (position % bytesPerDumpLine * 2)            // Bytes are displayed 2 characters wide
				+ (position % bytesPerDumpLine / dumpWordSize) // Every word is separated by an extra space
				+ addressStringSizePrependBytes;               // Each line starts with 10 characters indicating address

			if (DATDumpAnnotationIdentifiers.TryGetValue(e.Node!.Text, out var positionValues))
			{
				//var linePosStart = rtbDATDumpView.GetFirstCharIndexFromLine(positionValues.Item1 / bytesPerDumpLine);
				//var linePosEnd = rtbDATDumpView.GetFirstCharIndexFromLine(positionValues.Item2 / bytesPerDumpLine);

				var selectPositionStart = dumpPositionToRTBPosition(positionValues.Item1);
				var selectPositionEnd = Math.Min(dumpPositionToRTBPosition(positionValues.Item2), rtbDATDumpView.TextLength - 1);
				rtbDATDumpView.Select(selectPositionStart, selectPositionEnd - selectPositionStart);
			}
		}

		private void headerToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (imgContextMenu.SourceControl is PictureBox pb)
			{
				var index = currentUIImages.IndexOf(pb);
				var keys = "Header " + (index + 1);
				if (index >= 0 && imageHeaderIndexToNode.ContainsKey(keys))
				{
					ObjectTabViewControl.SelectedIndex = 1;
					tvDATDumpAnnotations.SelectedNode = imageHeaderIndexToNode[keys];
					dataDumpAnnotations_AfterSelect(sender, new TreeViewEventArgs(imageHeaderIndexToNode[keys]));
					tvDATDumpAnnotations.Focus();
				}
			}
		}

		private void pictureDataToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (imgContextMenu.SourceControl is PictureBox pb)
			{
				var index = currentUIImages.IndexOf(pb);
				var keys = "Image " + (index + 1);
				if (index >= 0 && imageDataIndexToNode.ContainsKey(keys))
				{
					ObjectTabViewControl.SelectedIndex = 1;
					tvDATDumpAnnotations.SelectedNode = imageDataIndexToNode[keys];
					dataDumpAnnotations_AfterSelect(sender, new TreeViewEventArgs(imageDataIndexToNode[keys]));
					tvDATDumpAnnotations.Focus();
				}
			}
		}

		private void cbVanillaObjects_CheckedChanged(object sender, EventArgs e)
		{
			InitUI(cbVanillaObjects.Checked, tbFileFilter.Text);
		}
	}
}
