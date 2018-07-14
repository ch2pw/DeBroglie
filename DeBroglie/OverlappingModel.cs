﻿using System.Collections.Generic;
using System.Linq;

namespace DeBroglie
{

    public class OverlappingModel : TileModel
    {
        private List<PatternArray> patternArrays;

        private int n;
        private bool periodic;
        int rotationalSymmetry;
        bool reflectionalSymmetry;

        private int groundPattern;

        private IReadOnlyDictionary<int, Tile> patternsToTiles;
        private ILookup<Tile, int> tilesToPatterns;

        public static OverlappingModel Create<T>(T[,] sample, int n, bool periodic, int symmetries)
        {
            var topArray = new TopArray2D<T>(sample, periodic).ToTiles();

            return new OverlappingModel(topArray, n, symmetries > 1 ? symmetries / 2 : 1, symmetries > 1);
        }


        public OverlappingModel(ITopArray<Tile> sample, int n, int rotationalSymmetry, bool reflectionalSymmetry)
        {
            this.n = n;
            this.periodic = sample.Topology.Periodic;
            this.rotationalSymmetry = rotationalSymmetry;
            this.reflectionalSymmetry = reflectionalSymmetry;

            List<double> frequencies;

            OverlappingAnalysis.GetPatterns(sample, n, periodic, rotationalSymmetry, reflectionalSymmetry, out patternArrays, out frequencies, out groundPattern);

            this.Frequencies = frequencies.ToArray();

            var directions = sample.Topology.Directions;

            Propagator = new int[patternArrays.Count][][];

            for (var p = 0; p < patternArrays.Count; p++)
            {
                Propagator[p] = new int[directions.Count][];
                for (var d = 0; d < directions.Count; d++)
                {
                    var l = new List<int>();
                    for (var p2 = 0; p2 < patternArrays.Count; p2++)
                    {
                        var dx = directions.DX[d];
                        var dy = directions.DY[d];
                        var dz = directions.DZ[d];
                        if (Aggrees(patternArrays[p], patternArrays[p2], dx, dy, dz))
                        {
                            l.Add(p2);
                        }
                    }
                    Propagator[p][d] = l.ToArray();
                }
            }

            patternsToTiles = patternArrays
                .Select((x, i) => new KeyValuePair<int, Tile>(i, x.Values[0, 0, 0]))
                .ToDictionary(x => x.Key, x => x.Value);

            tilesToPatterns = patternsToTiles.ToLookup(x => x.Value, x => x.Key);
        }

        public override IReadOnlyDictionary<int, Tile> PatternsToTiles => patternsToTiles;
        public override ILookup<Tile, int> TilesToPatterns => tilesToPatterns;

        public int N => n;

        public IReadOnlyList<PatternArray> PatternArrays => patternArrays;

        /**
          * Return true if the pattern1 is compatible with pattern2
          * when pattern2 is at a distance (dy,dx) from pattern1.
          */
        private bool Aggrees(PatternArray a, PatternArray b, int dx, int dy, int dz)
        {
            var xmin = dx < 0 ? 0 : dx;
            var xmax = dx < 0 ? dx + b.Width : a.Width;
            var ymin = dy < 0 ? 0 : dy;
            var ymax = dy < 0 ? dy + b.Height : a.Height;
            var zmin = dz < 0 ? 0 : dz;
            var zmax = dz < 0 ? dz + b.Depth : a.Depth;
            for (var x = xmin; x < xmax; x++)
            {
                for (var y = ymin; y < ymax; y++)
                {
                    for (var z = zmin; z < zmax; z++)
                    {
                        if (a.Values[x, y, z] != b.Values[x - dx, y - dy, z - dz])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public GroundConstraint GetGroundConstraint()
        {
            return new GroundConstraint(groundPattern);
        }

        public override void ChangeFrequency(Tile tile, double relativeChange)
        {
            var multiplier = (1 + relativeChange);
            for (var p = 0; p < patternArrays.Count; p++)
            {
                var patternArray = patternArrays[p];
                for (var x = 0; x < patternArray.Width; x++)
                {
                    for (var y = 0; y < patternArray.Height; y++)
                    {
                        for (var z = 0; z < patternArray.Depth; z++)
                        {
                            if (patternArray.Values[x, y, z] == tile)
                            {
                                Frequencies[p] *= multiplier;
                            }
                        }
                    }
                }
            }
        }
    }

}
