﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class TestTextureIporter : AssetPostprocessor {
	bool m_isReadable;
	bool m_importing;

	bool ShouldImportAsset(string path)
	{
		string pattern = GetMipmapFilenamePattern(path);
		string mip1Path = string.Format(pattern, 1);
		return File.Exists(mip1Path);
	}
	
	string GetMipmapFilenamePattern(string path)
	{
		var filename = Path.GetFileName(path);
		var filenameWithoutExtention = Path.GetFileNameWithoutExtension(path);
		var extension = Path.GetExtension(path);
		var directoryName = Path.GetDirectoryName(path);

		return Path.Combine(directoryName, filenameWithoutExtention + ".mip{0}" + extension);
	}

	void OnPreprocessTexture()
	{
		string extension = Path.GetExtension(assetPath);
		string filenameWithoutExtention = Path.GetFileNameWithoutExtension(assetPath);
		var match = Regex.Match(filenameWithoutExtention, @"\.mip(\d{1,2})$");

		if(match.Success)
		{
			string filenameWithoutMip = filenameWithoutExtention.Substring(0, match.Index);
			string directoryName = Path.GetDirectoryName(assetPath);
			string mip0Path = Path.Combine(directoryName, filenameWithoutMip + extension);

			TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(mip0Path);
			if(importer != null)
				importer.SaveAndReimport();
		}

		if (ShouldImportAsset(assetPath))
		{
			m_importing = true;

			string pattern = GetMipmapFilenamePattern(assetPath);
			int m = 1;

			bool reimport = false;

			while(true)
			{
				string mipPath = string.Format(pattern, m);
				m++;

				TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(mipPath);
				if(importer != null)
				{

					if(!importer.mipmapEnabled || !importer.isReadable)
					{
						importer.mipmapEnabled = true;
						importer.isReadable = true;
						importer.SaveAndReimport();

						reimport = true;
					}
					continue;
				}
				else
				{
					break;
				}
			}

			if(reimport)
			{
				m_importing = false;
				return;
			}
			TextureImporter textureImporter  = (TextureImporter)assetImporter;
			m_isReadable = textureImporter.isReadable;
			textureImporter.isReadable = true;
		}
	}

	void OnPostprocessTexture(Texture2D texture)
	{
		if (m_importing)
		{
			string pattern = GetMipmapFilenamePattern(assetPath);

			for (int m = 0; m < texture.mipmapCount; m++)
			{
				var mipmapTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(string.Format(pattern, m));

				if(mipmapTexture != null)
				{
					Color[] c = mipmapTexture.GetPixels(0);
					texture.SetPixels(c, m);
				}
			}
			
			texture.Apply(false, !m_isReadable);
			TextureImporter textureImporter  = (TextureImporter)assetImporter;
			textureImporter.isReadable = m_isReadable;
		}
	}
}

