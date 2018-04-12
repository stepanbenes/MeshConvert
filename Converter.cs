using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Newtonsoft.Json;

namespace MeshConvert
{
	public class Converter
	{
		public static List<string> Convert(string inputMeshFilePath, string outputDirectoryPath)
		{
			if (!Directory.Exists(outputDirectoryPath))
			{
				Directory.CreateDirectory(outputDirectoryPath);
			}

			string inputFileExtension = Path.GetExtension(inputMeshFilePath);
			TileSet tileset;

			switch (inputFileExtension)
			{
				case ".json":
					{
						tileset = JsonConvert.DeserializeObject<TileSet>(File.ReadAllText(inputMeshFilePath));
					}
					break;
				case ".vtk":
					{
						Mesh mesh;
						using (var fileStream = File.OpenRead(inputMeshFilePath))
						using (var streamReader = new StreamReader(fileStream))
						{
							mesh = new VtkFileParser().Parse(streamReader);
						}
						tileset = new TileSet
						{
							Tile = new[] { new Tile { Mesh = mesh } }
						};
					}
					break;
				default:
					throw new NotSupportedException("This input file type is not supported: " + inputFileExtension);
			}

			List<string> outputFilePaths = new List<string>(); 
			Guid layerId = Guid.NewGuid(); // TODO: set appropriate LayerId
			for (int index = 0; index < tileset.Tile.Length; index++)
			{
				Mesh meshInput = tileset.Tile[index].Mesh;

				// Mesh
				var (center, radius) = calculateMeshDimensions(meshInput);
				var meshOutput = new MeshOutput
				{
					LayerId = layerId,
					Index = index, // NOTE: numbering is 0-based
					NumberOfPoints = meshInput.NNodes,
					NumberOfCells = meshInput.NElem,
					//NumberOfEdges = 0,
					Center = center,
					Radius = radius,
					PointCoordinates = convertToBase64(meshInput.Nodes.SelectMany(i => i).ToArray()),
					CellConnectivity = convertToBase64(meshInput.Elements.SelectMany(i => i).Select(i => i - 1).ToArray()),
					CellTypes = convertToBase64(new byte[] { 5 /* TriangleLinear */ })
				};

				string outputMeshFilePath = Path.Combine(outputDirectoryPath, $"{meshOutput.Index}.mesh.json");
				writeJsonFile(outputMeshFilePath, meshOutput);
				outputFilePaths.Add(outputMeshFilePath);

				// Material attribute
				var attributeOutput = new AttributeOutput
				{
					LayerId = layerId,
					Index = index, // NOTE: numbering is 0-based
					MeshIndex = meshOutput.Index,
					FieldName = "Material",
					Location = "Cells",
					Compression = null,
					Encoding = new EncodingParameters
					{
						DataType = "Int32",
						OriginalLength = meshInput.NElem,
						Length = meshInput.NElem,
						Offset = 0,
						DefaultValue = null
					},
					Data = convertToBase64(meshInput.ElemMat.Select(matArray => matArray.Single()).ToArray())
				};

				string outputMaterialAttributeFilePath = Path.Combine(outputDirectoryPath, $"{attributeOutput.Index}.attribute.json");
				writeJsonFile(outputMaterialAttributeFilePath, attributeOutput);
				outputFilePaths.Add(outputMaterialAttributeFilePath);
			}

			return outputFilePaths;
		}

		private static void writeJsonFile(string filePath, object objectToSerialize)
		{
			string outputJson = JsonConvert.SerializeObject(objectToSerialize);
			File.WriteAllText(filePath, outputJson);
		}

		private static string convertToBase64<T>(T[] data) where T : struct
		{
			byte[] bytes = new byte[data.Length * Marshal.SizeOf<T>()];
			Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
			return System.Convert.ToBase64String(bytes);
		}

		private static (float[] center, float radius) calculateMeshDimensions(Mesh mesh)
		{
			float[] min = { float.MaxValue, float.MaxValue, float.MaxValue };
			float[] max = { float.MinValue, float.MinValue, float.MinValue };
			for (int i = 0; i < mesh.NNodes; i++)
			{
				for (int dim = 0; dim < 3; dim++)
				{
					min[dim] = Math.Min(min[dim], mesh.Nodes[i][dim]);
					max[dim] = Math.Max(max[dim], mesh.Nodes[i][dim]);
				}
			}
			float[] v = { (max[0] - min[0]) * 0.5f, (max[1] - min[1]) * 0.5f, (max[2] - min[2]) * 0.5f };
			float[] center = { v[0] + min[0], v[1] + min[1], v[2] + min[2] };
			float radius = (float)Math.Sqrt(v[0] * v[0] + v[1] * v[1] + v[2] * v[2]);
			return (center, radius);
		}
	}
}
