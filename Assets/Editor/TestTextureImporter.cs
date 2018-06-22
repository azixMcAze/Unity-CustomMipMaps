using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

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

		return Path.Combine(directoryName, filenameWithoutExtention + ".{0}" + extension);
	}

	void OnPreprocessTexture()
	{
		if (ShouldImportAsset(assetPath))
		{
			m_importing = true;
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

