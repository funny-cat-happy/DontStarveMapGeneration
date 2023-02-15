using System;
using System.Collections.Generic;

namespace SharpVoronoiLib
{
    internal abstract class RandomPointGeneration : IPointGenerationAlgorithm
    {
        public List<VoronoiSite> Generate(double minX, double minY, double maxX, double maxY, int count)
        {
            List<VoronoiSite> sites = new List<VoronoiSite>(count);

            Random random = new Random();

            for (int i = 0; i < count; i++)
            {
                sites.Add(
                    new VoronoiSite(
                        GetNextRandomValue(random, minX, maxX),
                        GetNextRandomValue(random, minY, maxY)
                    )
                );
            }

            return sites;
        }

        
        protected abstract double GetNextRandomValue(Random random, double min, double max);
    }
}