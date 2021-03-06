﻿/*
 * User: good
 * Date: 2/21/2018
 * Time: 7:57 PM
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace VNDSConverter
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public string lastChosenDirectory=null;
		public string confirmedChosenDirectory=null;
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
			if (Options.canUseFFmpeg){
				ffmpegstatuslabel.Text="FFmpeg detected!"; // Enthusiasm, an average Windows user managed to download a program.
			}else{
				ffmpegstatuslabel.Text="FFmpeg not detected, .aac audio can't be converted. Please place ffmpeg.exe in the same directory as this program.";
				ffmpegstatuslabel.ForeColor = Color.Red;
			}
			
			for (int i=0;i<Options.possiblePlatforms.Length;++i){
				platformListBox.Items.Add(Options.possiblePlatforms[i]);
			}
		}
		void selectButtonClick(object sender, EventArgs e)
		{
			FolderBrowserDialog myOpenDialog = new FolderBrowserDialog();
			myOpenDialog.SelectedPath = Application.StartupPath;
			if (myOpenDialog.ShowDialog() == DialogResult.OK){
				if (!Directory.Exists(Path.Combine(myOpenDialog.SelectedPath,"script"+Path.DirectorySeparatorChar)) && !File.Exists((Path.Combine(myOpenDialog.SelectedPath,"script.zip"+Path.DirectorySeparatorChar)))){
					DialogResult dialogResult = MessageBox.Show("The folder you chose is probably not a VNDS game folder because it doesn't contain a \"script\" folder.\nContinue?","onoziez", MessageBoxButtons.YesNo);
					if (dialogResult != DialogResult.Yes){
						lastChosenDirectory=null;
						return;
					}
				}
				MessageBox.Show(String.Format("For the root of the VNDS game, you chose {0}.",myOpenDialog.SelectedPath));
				lastChosenDirectory = myOpenDialog.SelectedPath;
				selectFolderButton.Text="Reselect Folder";
			}
		}
		void GoButtonClick(object sender, EventArgs e){
			if (lastChosenDirectory!=null){
				if (platformListBox.SelectedIndex==-1){
					MessageBox.Show("Please select a platform with the list box on the right.");
				}else{
					confirmedChosenDirectory = lastChosenDirectory;
					Options.applyPlatformPresent(Options.possiblePlatforms[platformListBox.SelectedIndex]);
					Close();
				}
			}else{
				MessageBox.Show("Please select a folder first.");
			}
		}
	}
}
