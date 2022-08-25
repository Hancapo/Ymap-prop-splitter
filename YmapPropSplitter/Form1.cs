using CodeWalker.GameFiles;
using CodeWalker.World;
using System.Globalization;

namespace YmapPropSplitter
{
    public partial class Form1 : Form
    {

        public List<ArchetypeElement> YtypArchetypes = new();
        public string[] SelectedYmaps;
        public string[] SelectedYtyps;
        public string[] SelectedYmapsToMerge;
        public string[] SelectedTrainTracks;
        public Form1()
        {
            InitializeComponent();
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }

        private void btnBrowseYTYP_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog fbw = new();

            DialogResult dialog = fbw.ShowDialog();

            if (dialog == DialogResult.OK)
            {
                tbYTYP.Text = fbw.SelectedPath;
                int ytypCount = Directory.GetFiles(fbw.SelectedPath, "*.ytyp").Length;

                if (ytypCount > 0)
                {
                    SelectedYtyps = Directory.GetFiles(fbw.SelectedPath, "*.ytyp");

                    lbYTYPstatus.Text = ($"{ytypCount} YTYP(s) found!");


                }
                else
                {
                    MessageBox.Show($"No YTYP(s) found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }



        }

        private void btnBrowseYMAP_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbw = new();
            fbw.ShowDialog();

            tbYMAP.Text = fbw.SelectedPath;

            int ymapCount = Directory.GetFiles(fbw.SelectedPath, "*.ymap").Length;

            if (ymapCount > 0)
            {
                SelectedYmaps = Directory.GetFiles(fbw.SelectedPath, "*.ymap");

                lbYmap.Text = $"{ymapCount} YMAP(s) found!";


            }
            else
            {
                MessageBox.Show($"No YMAP(s) found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBrowseOutput_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog fbw = new();
            fbw.ShowDialog();

            tbOutput.Text = fbw.SelectedPath;
        }

        private void tbYTYP_TextChanged(object sender, EventArgs e)
        {


        }

        private void btnSplit_Click(object sender, EventArgs e)
        {
            if (SelectedYmaps != null && SelectedYtyps.Length != 0 && tbOutput.Text != String.Empty)
            {
                //YTYP Processing
                foreach (var ytyp in SelectedYtyps)
                {
                    ArchetypeElement archetypeElement = new();

                    YtypFile ytypFile = new();
                    ytypFile.Load(File.ReadAllBytes(ytyp));

                    archetypeElement.YtypName = Path.GetFileNameWithoutExtension(ytyp);

                    List<MetaHash> metaHashes = new();

                    foreach (var archs in ytypFile.AllArchetypes)
                    {


                        if (archs.Type == MetaName.CBaseArchetypeDef || archs.Type == MetaName.CTimeArchetypeDef)
                        {
                            metaHashes.Add(archs.Hash);
                        }

                    }

                    if (metaHashes.Count > 0)
                    {
                        archetypeElement.archetypeNames = metaHashes;
                        YtypArchetypes.Add(archetypeElement);

                    }


                }

                //YMAP Processing
                foreach (var ymap in SelectedYmaps)
                {
                    YmapFile ymapFile = new();
                    ymapFile.Load(File.ReadAllBytes(ymap));

                    string ymapFileName = Path.GetFileNameWithoutExtension(ymap);


                    foreach (var ytypThing in YtypArchetypes)
                    {
                        List<YmapEntityDef> foundEntities = new();

                        foreach (var archs in ymapFile.AllEntities)
                        {
                            foreach (var addedArch in ytypThing.archetypeNames)
                            {
                                if (archs._CEntityDef.archetypeName == addedArch &&
                                    archs._CEntityDef.lodLevel == rage__eLodType.LODTYPES_DEPTH_ORPHANHD)
                                {
                                    foundEntities.Add(archs);
                                    ymapFile.RemoveEntity(archs);
                                }

                            }
                        }

                        Directory.CreateDirectory(Path.Combine(tbOutput.Text, "modified_ymaps"));

                        byte[] newYmapBytes = ymapFile.Save();
                        File.WriteAllBytes(Path.Combine(tbOutput.Text, "modified_ymaps") + $"\\{ymapFileName}.ymap", newYmapBytes);
                        if (foundEntities.Count > 0)
                        {
                            YmapFile SplittedYmap = new()
                            {
                                Name = $"{ymapFileName}_{ytypThing.YtypName}"
                            };

                            foreach (var item in foundEntities)
                            {
                                SplittedYmap.AddEntity(item);
                            }

                            if (SplittedYmap.AllEntities != null)
                            {
                                SplittedYmap.BuildCEntityDefs();
                                SplittedYmap.CalcExtents();
                                SplittedYmap.CalcFlags();
                                byte[] newYmapBytes2 = SplittedYmap.Save();
                                File.WriteAllBytes(tbOutput.Text + $"\\{ymapFileName}_{ytypThing.YtypName}.ymap", newYmapBytes2);
                            }
                        }



                    }
                }
                MessageBox.Show($"Processing Complete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);


            }
            else
            {
                MessageBox.Show("Please select YTYP and YMAP files!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }


        // Ymap merger =>

        private void btnBrowseYmapM_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbw = new();


            DialogResult dr = fbw.ShowDialog();

            if (dr == DialogResult.OK)
            {
                SelectedYmapsToMerge = Directory.GetFiles(fbw.SelectedPath, "*.ymap");

                tbYmapM.Text = fbw.SelectedPath;

                if (SelectedYmapsToMerge.Length > 0)
                {
                    lbYmapMerg.Text = $"{SelectedYmapsToMerge.Length} YMAP(s) found!";
                }
                else
                {
                    MessageBox.Show($"No YMAP(s) found!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }



        }



        private void btnMerge_Click(object sender, EventArgs e)
        {

            YmapFile yfhola = new();
            yfhola.Name = tbYmapName.Text;

            if (tbOutputM.Text != String.Empty && tbYmapName.Text != String.Empty)
            {
                SelectedYmapsToMerge = Directory.GetFiles(tbYmapM.Text, "*.ymap");

                if (SelectedYmapsToMerge.Length > 0)
                {
                    List<YmapEntityDef> AllEntsFromYmaps = new();
                    foreach (var ymap in SelectedYmapsToMerge)
                    {
                        YmapFile ymapFile = new();
                        ymapFile.Load(File.ReadAllBytes(ymap));

                        ymapFile.AllEntities.ToList().ForEach(x => yfhola.AddEntity(x));




                    }

                    yfhola.BuildCEntityDefs();
                    yfhola.CalcExtents();
                    yfhola.CalcFlags();

                    byte[] mergedYmapBytes = yfhola.Save();

                    File.WriteAllBytes(tbOutputM.Text + $"\\{tbYmapName.Text}.ymap", mergedYmapBytes);

                    MessageBox.Show($"Merge Complete!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Please select YMAP files!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBrowseOutputM_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbw = new();


            DialogResult dr = fbw.ShowDialog();
            if (dr == DialogResult.OK) { tbOutputM.Text = fbw.SelectedPath; }





        }





        private void btnMoveTracks_Click(object sender, EventArgs e)
        {
            double OffsetX;
            double OffsetY;
            double OffsetZ;

            if (tbOutputTracks.Text != string.Empty && tbTrainTracksIn.Text != string.Empty)
            {
                if (tbMoveX.Text != string.Empty && tbMoveY.Text != string.Empty && tbMoveZ.Text != string.Empty)
                {
                    if (double.TryParse(tbMoveX.Text, out OffsetX) &&
                       double.TryParse(tbMoveY.Text, out OffsetY) &&
                       double.TryParse(tbMoveZ.Text, out OffsetZ))
                    {
                        if (SelectedTrainTracks.Length > 0)
                        {
                            foreach (var trainTrackFile in SelectedTrainTracks)
                            {
                                string trainTrackName = Path.GetFileName(trainTrackFile);
                                TrackFile trackFile = new();
                                trackFile.LoadFile(trainTrackFile);
                                trackFile.MoveTrackNodes(OffsetX, OffsetY, OffsetZ);
                                trackFile.SaveFile(Path.Combine(tbOutputTracks.Text, trainTrackName));
                            }

                            MessageBox.Show($"{SelectedTrainTracks.Length} Train Track(s) has been moved", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        }

                    }


                }
            }
            else
            {
                MessageBox.Show("Invalid input and ouput", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }

        private void btnTrackOutputBrowse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbw = new();


            DialogResult dr = fbw.ShowDialog();
            if (dr == DialogResult.OK) { tbOutputTracks.Text = fbw.SelectedPath; }

        }

        private void btnBrowseTracks_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbw = new();
            fbw.ShowDialog();

            tbTrainTracksIn.Text = fbw.SelectedPath;

            int trainTracksCount = Directory.GetFiles(fbw.SelectedPath, "*.dat").Length;

            if (trainTracksCount > 0)
            {
                SelectedTrainTracks = Directory.GetFiles(fbw.SelectedPath, "*.dat");

                lbTrainTrack.Text = $"{trainTracksCount} Train Track(s) found!";


            }
            else
            {
                MessageBox.Show($"No Train Track(s) found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbMoveX_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(tbMoveX.Text, out _) || tbMoveX.Text.Contains(',')) { tbMoveX.Text = string.Empty; }

        }

        private void tbMoveY_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(tbMoveY.Text, out _) || tbMoveY.Text.Contains(',')) { tbMoveY.Text = string.Empty; }
        }

        private void tbMoveZ_TextChanged(object sender, EventArgs e)
        {
            if (!double.TryParse(tbMoveZ.Text, out _) || tbMoveZ.Text.Contains(',')) { tbMoveZ.Text = string.Empty; }
        }
    }
}