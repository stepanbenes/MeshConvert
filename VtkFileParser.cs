using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace MeshConvert
{
	class VtkFileParser
	{
		enum State
		{
			FileVersion,
			Header,
			FileFormat,
			DatasetStructure,
			PointsHeader,
			Points,
			PolygonsHeader,
			Polygons,
			CellDataHeader,
			CellDataType,
			CellDataLookupTable,
			CellData,
		}

		struct Point
		{
			public float X, Y, Z;
		}

		struct Triangle
		{
			public int Point1, Point2, Point3;
		}

		public Mesh Parse(StreamReader reader)
		{
			State state = State.FileVersion;

			string line;
			List<Point> points = new List<Point>();
			List<Triangle> triangles = new List<Triangle>();
			List<int[]> materialIds = new List<int[]>();
			while ((line = reader.ReadLine()) != null)
			{
				string[] tokens = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

				if (tokens.Length == 0)
					continue;

				if (tokens[0].StartsWith("POINTS", StringComparison.InvariantCultureIgnoreCase))
				{
					state = State.PointsHeader;
				}
				else if (tokens[0].StartsWith("POLYGONS", StringComparison.InvariantCultureIgnoreCase))
				{
					state = State.PolygonsHeader;
				}
				else if (tokens[0].StartsWith("CELL_DATA", StringComparison.InvariantCultureIgnoreCase))
				{
					state = State.CellDataHeader;
				}

				switch (state)
				{
					case State.FileVersion:
						// ignore
						state = State.Header;
						break;
					case State.Header:
						// ignore
						state = State.FileFormat;
						break;
					case State.FileFormat:
						if (!tokens[0].Equals("ASCII", StringComparison.InvariantCultureIgnoreCase))
							throw new NotSupportedException("Only ASCII format is supported.");
						state = State.DatasetStructure;
						break;
					case State.DatasetStructure:
						// TODO: check data structure type
						break;
					case State.PointsHeader:
						{
							int numberOfPoints = int.Parse(tokens[1], CultureInfo.InvariantCulture);
							if (!tokens[2].Equals("float", StringComparison.InvariantCultureIgnoreCase))
							{
								throw new NotSupportedException("Only float data type is supported for point coordinates.");
							}
							state = State.Points;
						}
						break;
					case State.Points:
						var point = new Point
						{
							X = float.Parse(tokens[0], CultureInfo.InvariantCulture),
							Y = float.Parse(tokens[1], CultureInfo.InvariantCulture),
							Z = float.Parse(tokens[2], CultureInfo.InvariantCulture),
						};
						points.Add(point);
						break;
					case State.PolygonsHeader:
						{
							int polygonCount = int.Parse(tokens[1], CultureInfo.InvariantCulture);
							int polygonSize = int.Parse(tokens[2], CultureInfo.InvariantCulture);
							state = State.Polygons;
						}
						break;
					case State.Polygons:
						{
							int size = int.Parse(tokens[0], CultureInfo.InvariantCulture);
							if (size == 3)
							{
								var triangle = new Triangle
								{
									Point1 = int.Parse(tokens[1], CultureInfo.InvariantCulture),
									Point2 = int.Parse(tokens[2], CultureInfo.InvariantCulture),
									Point3 = int.Parse(tokens[3], CultureInfo.InvariantCulture),
								};
								triangles.Add(triangle);
							}
							else
								throw new NotSupportedException("Unsupported polygon size");
						}
						break;
					case State.CellDataHeader:
						{
							int count = int.Parse(tokens[1], CultureInfo.InvariantCulture);
							state = State.CellDataType;
						}
						break;
					case State.CellDataType:
						{
							string name = tokens[1];
							string dataType = tokens[2];

							if (!dataType.Equals("int", StringComparison.InvariantCultureIgnoreCase))
								throw new NotSupportedException("Only int data type is supported as cell data type.");

							int componentCount = int.Parse(tokens[3], CultureInfo.InvariantCulture);

							state = State.CellDataLookupTable;
						}
						break;
					case State.CellDataLookupTable:
						// ignore
						state = State.CellData;
						break;
					case State.CellData:
						{
							var numbers = tokens.Select(token => int.Parse(token, CultureInfo.InvariantCulture)).ToArray();
							materialIds.Add(numbers);
						}
						break;
					default:
						throw new NotSupportedException("Unsupported state");
				}
			}

			var mesh = new Mesh
			{
				NNodes = points.Count,
				NElem = triangles.Count,
				Nodes = points.Select(point => new[] { point.X, point.Y, point.Z }).ToArray(),
				Elements = triangles.Select(triangle => new[] { triangle.Point1, triangle.Point2, triangle.Point3 }).ToArray(),
				ElemMat = materialIds.ToArray()
			};
			return mesh;
		}
	}
}
