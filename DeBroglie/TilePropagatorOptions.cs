﻿using DeBroglie.Constraints;
using DeBroglie.Topo;
using DeBroglie.Wfc;
using System;
using System.Collections.Generic;

namespace DeBroglie
{
    public class PriorityAndWeight
    {
        public int Priority { get; set; }
        public double Weight { get; set; }
    }

    public enum PickHeuristicType
    {
        MinEntropy,
        Ordered
    }

    public class TilePropagatorOptions
    {
        /// <summary>
        /// Maximum number of steps to backtrack.
        /// Set to 0 to disable backtracking, and -1 for indefinite amounts of backtracking.
        /// </summary>
        public int BackTrackDepth { get; set; }

        /// <summary>
        /// Extra constraints to control the generation process.
        /// </summary>
        public ITileConstraint[] Constraints { get; set; }

        /// <summary>
        /// Overrides the weights set from the model, on a per-position basis.
        /// </summary>
        public ITopoArray<IDictionary<Tile, PriorityAndWeight>> Weights { get; set; }

        /// <summary>
        /// Source of randomness used by generation
        /// </summary>
        public Func<double> RandomDouble { get; set; }

        [Obsolete("Use RandomDouble")]
        public Random Random { get; set; }

        /// <summary>
        /// Controls which cells and tiles are selected during generation.
        /// </summary>
        public PickHeuristicType PickHeuristicType { get; set; }

        /// <summary>
        /// Controls the algorithm used for enforcing the constraints of the model.
        /// </summary>
        public ModelConstraintAlgorithm ModelConstraintAlgorithm { get; set; }
    }
}
