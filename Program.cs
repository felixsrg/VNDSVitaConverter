﻿/*
 * User: good
 * Date: 2/20/2018
 * Time: 7:10 PM
 */
using System;
using System.IO;
using System.Drawing;
// For specific image formats
using System.Drawing.Imaging;
using VNDSConverter;
using System.IO.Compression;
using System.Windows.Forms;

namespace VNDSConverter
{
	class Program
	{	
		static string changeDirectoryPath(string _newDirectory){
			string _returnString = _newDirectory.TrimEnd(Path.DirectorySeparatorChar);
			string _normalFolderName = Path.GetFileName(_returnString);
			_returnString = Path.GetDirectoryName(_returnString);
			_returnString = _returnString + Path.DirectorySeparatorChar +_normalFolderName+"-converted";
			return _returnString;
		}
		// Path the root directory of the new folder
		static string getNewCGDirectoryA(string _rootDirectory){
			return Path.Combine(_rootDirectory,"CG"+Path.DirectorySeparatorChar);
		}
		static string getNewCGDirectoryB(string _rootDirectory){
			return Path.Combine(_rootDirectory,"CGAlt"+Path.DirectorySeparatorChar);
		}
		static string getNewScriptDirectory(string _rootDirectory){
			return Path.Combine(_rootDirectory,"Scripts"+Path.DirectorySeparatorChar);
		}
		static string getNewAudioDirectory(string _rootDirectory){
			return Path.Combine(_rootDirectory,"SE"+Path.DirectorySeparatorChar);
		}
		
		// Pass the root directory of the old folder
		static string getOldCGDirectoryA(string _rootDirectory){
			return Path.Combine(_rootDirectory,"background"+Path.DirectorySeparatorChar);
		}
		static string getOldCGDirectoryB(string _rootDirectory){
			return Path.Combine(_rootDirectory,"foreground"+Path.DirectorySeparatorChar);
		}
		static string getOldScriptDirectory(string _rootDirectory){
			return Path.Combine(_rootDirectory,"script"+Path.DirectorySeparatorChar);
		}
		static string getOldAudioDirectory(string _rootDirectory){
			return Path.Combine(_rootDirectory,"sound"+Path.DirectorySeparatorChar);
		}
		
		static void createNewDirectories(string _newRootDirectory){
			Directory.CreateDirectory(_newRootDirectory);
			Directory.CreateDirectory(getNewAudioDirectory(_newRootDirectory));
			Directory.CreateDirectory(getNewCGDirectoryA(_newRootDirectory));
			Directory.CreateDirectory(getNewCGDirectoryB(_newRootDirectory));
			Directory.CreateDirectory(getNewScriptDirectory(_newRootDirectory));
		}
		
		static void copyIfExist(string _sourceFile, string _destFile){
			if (File.Exists(_sourceFile)){
				if (Options.detailedConsoleOutput){
					Console.Out.WriteLine("[COPY] {0} to {1}",_sourceFile,_destFile);
				}
				File.Copy(_sourceFile,_destFile);
			}
		}
		
		static void makeBitmap32Bit(ref Bitmap _toFix){
			Bitmap _newImage = new Bitmap(_toFix.Width,_toFix.Height,PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(_newImage))
			{
				g.DrawImageUnscaledAndClipped(_toFix,new Rectangle(0,0,_newImage.Width,_newImage.Height));
			}
			_toFix.Dispose();
			_toFix = _newImage;
		}
		
		static void processSingleImage(string _sourceFile, string _destFile){
			string _cachedExtension = Path.GetExtension(_sourceFile);
			if (_cachedExtension==".png" || _cachedExtension==".jpg" || _cachedExtension==".jpeg" || _cachedExtension==".bmp"){
				if (Options.detailedConsoleOutput){
					Console.Out.WriteLine("[Image] {0}",_sourceFile);
				}
				Bitmap _tempLoadedBitmap;
				try{
					_tempLoadedBitmap = new Bitmap(_sourceFile);
					//if (_tempLoadedBitmap.Width>_recordWidth){
					//	_recordWidth = _tempLoadedBitmap.Width;
					//}
					//if (_tempLoadedBitmap.Height>_recordHeight){
					//	_recordHeight = _tempLoadedBitmap.Height;
					//}
					makeBitmap32Bit(ref _tempLoadedBitmap);
				}catch(Exception){
					if (Options.importantConsoleOutput){
						Console.Out.WriteLine("[BLACK] Force black image {0}",_sourceFile);
					}
					_tempLoadedBitmap = new Bitmap(256,192,PixelFormat.Format32bppArgb);
					using (Graphics g = Graphics.FromImage(_tempLoadedBitmap)){
						g.FillRectangle(Brushes.Black,0,0,_tempLoadedBitmap.Width,_tempLoadedBitmap.Height);
					}
				}
				_tempLoadedBitmap.Save(_destFile,ImageFormat.Png);
				/*if (_cachedExtension==".png"){
					_tempLoadedBitmap.Save(Path.Combine(_destDirectory,Path.GetFileName(_filesToProcess[i]).ToUpper()),ImageFormat.Png);
				}else if (_cachedExtension==".jpg" || _cachedExtension==".jpeg"){
					_tempLoadedBitmap.Save(Path.Combine(_destDirectory,Path.GetFileName(_filesToProcess[i]).ToUpper()),ImageFormat.Jpeg);
				}*/
				_tempLoadedBitmap.Dispose();
			}else{
				if (Options.errorConsoleOutput){
					Console.Out.WriteLine("[Skip] Non-image extension {0}.",Path.GetExtension(_sourceFile));
				}
			}
		}

		// based on https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
		static void processAndCopyImages(string _sourceDirectory, string _destDirectory){
			// Get the subdirectories for the specified directory.
			DirectoryInfo _currentDirectoryInfo = new DirectoryInfo(_sourceDirectory);
			if (!_currentDirectoryInfo.Exists){
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: "+ _sourceDirectory);
			}
			// If the destination destination directory doesn't exist, create it.
			if (!Directory.Exists(_destDirectory)){
				Directory.CreateDirectory(_destDirectory);
			}
			// Get the files in the directory and copy them to the new location. Does not include subdirectory files
			FileInfo[] _currentDirectoryFiles = _currentDirectoryInfo.GetFiles();
			for (int i=0;i<_currentDirectoryFiles.Length;++i){
				string temppath = Path.Combine(_destDirectory, _currentDirectoryFiles[i].Name.ToUpper());
				processSingleImage(_currentDirectoryFiles[i].FullName,temppath);
			}
			DirectoryInfo[] _foundSubdirectories = _currentDirectoryInfo.GetDirectories();
			// Do this same function for all subdirectories
			for (int i=0;i<_foundSubdirectories.Length;++i){
				string temppath = Path.Combine(_destDirectory, _foundSubdirectories[i].Name.ToUpper());
				processAndCopyImages(_foundSubdirectories[i].FullName, temppath);
			}
		}

		// After running this function, there will be copies of the images files with fixed bit depths in the PNG format
		/*static void processAndCopyImages(string _sourceDirectory, string _destDirectory){
			if (Options.simpleConsoleOutput){
				Console.Out.WriteLine("[IMAGE] {0} to {1}",_sourceDirectory,_destDirectory);
			}
			string[] _filesToProcess = Directory.GetFiles(_sourceDirectory,"*",SearchOption.AllDirectories); // TODO - This removes directory structure, see https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
			// Make subdirectories
			for (int i=0;i<_filesToProcess.Length;++i){
				Directory.CreateDirectory(Path.Combine(_destDirectory,Path.GetDirectoryName(_filesToProcess[i].Substring(_sourceDirectory.Length)).ToUpper()));
			}
			//int _recordWidth=256;
			//int _recordHeight=192;
			for (int i=0;i<_filesToProcess.Length;i++){
				string _cachedExtension = Path.GetExtension(_filesToProcess[i]);
				if (_cachedExtension==".png" || _cachedExtension==".jpg" || _cachedExtension==".jpeg"){
					if (Options.detailedConsoleOutput){
						Console.Out.WriteLine("[Image] {0}",_filesToProcess[i]);
					}
					Bitmap _tempLoadedBitmap;
					try{
						_tempLoadedBitmap = new Bitmap(_filesToProcess[i]);
						//if (_tempLoadedBitmap.Width>_recordWidth){
						//	_recordWidth = _tempLoadedBitmap.Width;
						//}
						//if (_tempLoadedBitmap.Height>_recordHeight){
						//	_recordHeight = _tempLoadedBitmap.Height;
						//}
						makeBitmap32Bit(ref _tempLoadedBitmap);
					}catch(Exception){
						if (Options.importantConsoleOutput){
							Console.Out.WriteLine("[BLACK] Force black image {0}",_filesToProcess[i]);
						}
						_tempLoadedBitmap = new Bitmap(256,192,PixelFormat.Format32bppArgb);
						using (Graphics g = Graphics.FromImage(_tempLoadedBitmap)){
							g.FillRectangle(Brushes.Black,0,0,_tempLoadedBitmap.Width,_tempLoadedBitmap.Height);
						}
					}
					_tempLoadedBitmap.Save(Path.Combine(_destDirectory, _filesToProcess[i].Substring(_sourceDirectory.Length).ToUpper()),ImageFormat.Png);
					/*if (_cachedExtension==".png"){
						_tempLoadedBitmap.Save(Path.Combine(_destDirectory,Path.GetFileName(_filesToProcess[i]).ToUpper()),ImageFormat.Png);
					}else if (_cachedExtension==".jpg" || _cachedExtension==".jpeg"){
						_tempLoadedBitmap.Save(Path.Combine(_destDirectory,Path.GetFileName(_filesToProcess[i]).ToUpper()),ImageFormat.Jpeg);
					} ** /
					_tempLoadedBitmap.Dispose();
				}else{
					if (Options.errorConsoleOutput){
						Console.Out.WriteLine("[Skip] Non-image extension {0}.",Path.GetExtension(_filesToProcess[i]));
					}
				}
			}
		}*/
		
		static void maybeExtractZIPFile(string _zipPath, string _destPath){
			if (File.Exists(_zipPath)){
				using (ZipArchive myZipArchive = ZipFile.OpenRead(_zipPath)){
					if (Options.simpleConsoleOutput){
						Console.Out.WriteLine("[EXTRACT] {0} to {1}",_zipPath,_destPath);
					}
					try{
						myZipArchive.ExtractToDirectory(_destPath,true);
					}catch(Exception e){
						Console.Out.WriteLine("Error extracting ZIP file.");
						Console.Out.WriteLine(e.ToString());
						if (StolenCode.IsRunningOnMono()){
							Console.Out.WriteLine("=============");
							Console.Out.WriteLine("Please make sure Mono is updated if it's not already.");
						}
						printPressAnyKey();
						Console.ReadKey();
						Environment.Exit(1);
					}
				}
				
			}else{
				if (Options.simpleConsoleOutput){
					Console.Out.WriteLine("[NOT EXTRACT] {0} not exist.",_zipPath);
				}
			}
		}
		
		public static string doFunctionality(string _originalGameFolderName){
			string _newGameFolderPath = changeDirectoryPath(_originalGameFolderName);
			if (Options.autoDelete){
				if (Directory.Exists(_newGameFolderPath)){
					try{
						if (Options.importantConsoleOutput){
							Console.Out.WriteLine("Attempting to delete old conversion directory...");
						}
						Directory.Delete(_newGameFolderPath,true);
					}catch(Exception e){
						if (Options.errorConsoleOutput){
							Console.Out.WriteLine("{0}\nFailed to delete the old conversion directory. This probably means the program can't write to at least one of the files in the directory, so conversion will probably fail when it tries to overwrite that file.",e.ToString());
						}
					}
				}
			}
			createNewDirectories(_newGameFolderPath);
			
			maybeExtractZIPFile(Path.Combine(_originalGameFolderName,"background.zip"),_originalGameFolderName);
			maybeExtractZIPFile(Path.Combine(_originalGameFolderName,"foreground.zip"),_originalGameFolderName);
			maybeExtractZIPFile(Path.Combine(_originalGameFolderName,"script.zip"),_originalGameFolderName);
			maybeExtractZIPFile(Path.Combine(_originalGameFolderName,"sound.zip"),_originalGameFolderName);
			
			StolenCode.copyDirectory(getOldAudioDirectory(_originalGameFolderName),getNewAudioDirectory(_newGameFolderPath));
			StolenCode.copyDirectory(getOldScriptDirectory(_originalGameFolderName),getNewScriptDirectory(_newGameFolderPath));
			processAndCopyImages(getOldCGDirectoryA(_originalGameFolderName),getNewCGDirectoryA(_newGameFolderPath));
			processAndCopyImages(getOldCGDirectoryB(_originalGameFolderName),getNewCGDirectoryB(_newGameFolderPath));
			if (Options.simpleConsoleOutput){
				Console.Out.WriteLine("[COPY] Assorted root game directory files");
			}
			copyIfExist(Path.Combine(_originalGameFolderName,"default.ttf"),Path.Combine(_newGameFolderPath,"default.ttf"));
			copyIfExist(Path.Combine(_originalGameFolderName,"info.txt"),Path.Combine(_newGameFolderPath,"info.txt"));
			copyIfExist(Path.Combine(_originalGameFolderName,"img.ini"),Path.Combine(_newGameFolderPath,"img.ini"));
			if (File.Exists(Path.Combine(_originalGameFolderName,"icon.png"))){
				Console.Out.WriteLine("Fix and copy icon.png");
				processSingleImage(Path.Combine(_originalGameFolderName,"icon.png"),Path.Combine(_newGameFolderPath,"icon.png"));
			}
			if (File.Exists(Path.Combine(_originalGameFolderName,"thumbnail.png"))){
				Console.Out.WriteLine("Fix and copy thumbnail.png");
				processSingleImage(Path.Combine(_originalGameFolderName,"thumbnail.png"),Path.Combine(_newGameFolderPath,"thumbnail.png"));
			}
			
			
			if (Options.simpleConsoleOutput){
				Console.Out.WriteLine("[CREATE] {0}",Path.Combine(_newGameFolderPath,"isvnds"));
			}
			File.Create(Path.Combine(_newGameFolderPath,"isvnds")).Dispose();
			
			return _newGameFolderPath;
		}
		
		static void toggleDependingOnArgs(string[] args, ref int i, ref bool _toToggle){
			i++;
			if (args[i]!="off"){
				_toToggle=true;
			}else{
				_toToggle=false;
			}
		}
		
		static void printPressAnyKey(){
			Console.Write("Press any key to continue . . . ");
		}
		
		[STAThread]
		static void Main(string[] args){
			string _sourceFile=null;
			if (args.Length==0){
				if (!StolenCode.IsRunningOnMono()){
					Application.EnableVisualStyles();
					Application.SetCompatibleTextRenderingDefault(false);
					MainForm myMainForm = new MainForm();
					Application.Run(myMainForm);
					_sourceFile = myMainForm.confirmedChosenDirectory;
					if (_sourceFile==null){
						Console.Out.WriteLine("_sourceFile==null");
						return;
					}
				}else{
					args = new string[1];
					args[0]=null;
					while (args[0]==null){
						Console.Out.WriteLine("Enter the VNDS game's folder path. The path should end in a slash and the folder should be in a writable directory\nFor example, /home/nathan/higurashi/\n");
						args[0] = Console.ReadLine();
						if (!Directory.Exists(args[0])){
							Console.Out.WriteLine("\nDirectory {0} does not exist.",args[0]);
							args[0]=null;
						}
					}
				}
			}
			int i;
			for (i=0;i<args.Length;i++){
				if (args[i][0]!='-'){
					_sourceFile = args[i];
				}else{
					if (args[i]=="-simpleoutput"){
						toggleDependingOnArgs(args,ref i, ref Options.simpleConsoleOutput);
					}else if (args[i]=="-detailedoutput"){
						toggleDependingOnArgs(args,ref i, ref Options.detailedConsoleOutput);
					}else if (args[i]=="-erroroutput"){
						toggleDependingOnArgs(args,ref i, ref Options.errorConsoleOutput);
					}else if (args[i]=="-importantoutput"){
						toggleDependingOnArgs(args,ref i, ref Options.importantConsoleOutput);
					}else if (args[i]=="-autodelete"){
						toggleDependingOnArgs(args,ref i, ref Options.autoDelete);
					}
				}
			}
			if (_sourceFile==null){
				if (Options.importantConsoleOutput){
					Console.Out.WriteLine("No path found. Make sure your path doesn't start with a hyphen.");
					printPressAnyKey();
				}
				Console.ReadKey();
				return;
			}
			if (!File.Exists(Path.Combine(_sourceFile,"info.txt"))){
				Console.Out.WriteLine("{0} does not exist, so I assume that this IS NOT a VNDS game folder. Retry.\n",Path.Combine(_sourceFile,"info.txt"));
				return;
			}

			//for (i=0;i<possibleFreacLocations.Length;++i){
			//	if (File.Exists(possibleFreacLocations[i])){
			//		Options.canConvertAudio=true;
			//		Options.actualFreacLocation = possibleFreacLocations[i];
			//		break;
			//	}
			//}
			//if (!Options.canConvertAudio){
			//	Console.out.WriteLine("This is the version without audio conversion. DS novels may not convert correctly.");
			//}

			if (Options.importantConsoleOutput){
				Console.WriteLine("Hello World!");
			}
			// TODO: Implement Functionality Here
			if (Options.importantConsoleOutput){
				Console.Out.WriteLine("Done, you may close this window.\nThe converted game is at {0}",doFunctionality(_sourceFile));
				printPressAnyKey();
				Console.ReadKey(true);
			}else{
				doFunctionality(_sourceFile);
			}
		}
	}
}