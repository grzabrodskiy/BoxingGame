﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Main
{
    public struct StrAttDistribution
    {

        public List<string> Names;
        public List<double> LowLimit;
        public List<double> HiLimit;

        public StrAttDistribution(List<string> names, List<double> lowLimit, List<double> hiLimit)
        {
            Names = names;
            LowLimit = lowLimit;
            HiLimit = hiLimit;
        }

    }

    public static class Utility
    {

        public static double StandardDeviation(this IEnumerable<double> values, bool populationStd = true)
        {
            double avg = values.Average();
            double std =  Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
            if (!populationStd)
                std *= ((double) values.Count() / (values.Count() - 1.0));

            return std;
        }

        //names

        static StrAttDistribution fnames;
        public static string getRandomLastName()
        {
            if (fnames.Equals(default(StrAttDistribution)))
                fnames = LoadDistributionFile("LName.txt");

            return ResultFromDistribution(fnames);
        }

        public static string getRandomFirstName()
        {
            if (fnames.Equals(default(StrAttDistribution)))
                fnames = LoadDistributionFile("FName.txt");

            return ResultFromDistribution(fnames);
        }

        public static string getRandomName()
        {
            return string.Format("{0} {1}", getRandomFirstName(), getRandomLastName());
        }

        public static StrAttDistribution LoadDistributionFile(string filename)
        {
            List<string> names    = new List<string>();
            List<double> lowLimit = new List<double>();
            List<double> hiLimit  = new List<double>();
            var assembly = Assembly.GetExecutingAssembly();
            //using (var reader = new StreamReader(filename))

            string resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith(filename));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(',');
                    names.Add(values[0]);
                    lowLimit.Add(double.Parse(values[1]));
                    hiLimit.Add(double.Parse(values[2]));
                }
            }

            StrAttDistribution dist = new StrAttDistribution(names, lowLimit, hiLimit);
            return dist;
        }

        private static string ResultFromDistribution(StrAttDistribution d)
        {
            double bound = d.HiLimit[d.HiLimit.Count - 1];
            double target = StatsUtils.RangeUniform(0d, bound);

            int lo = 0, hi = d.Names.Count - 1;

            while (lo <= hi)
            {
                int mid = hi + lo / 2;

                if (target < d.LowLimit[mid])
                    hi = mid - 1;
                else if (target > d.HiLimit[mid])
                    lo = mid + 1;
                else
                    return d.Names[mid];
            }

            return d.Names[lo];

        }

        public static double WeightedAverage(params int[] list)
        {
            if (list.Length == 0 || (list.Length & 1) == 1)
                return -1;

            int i = 0;
            double weightSum = 0;
            double weightedSum = 0;
            while( i < list.Length)
            {
                int element = list[i++];
                int weight  = list[i++];
                weightSum += weight;
                weightedSum += element * weight;

            }

            return weightedSum / weightSum;

        }

        public static double AttributeRatio(int x, int y, double attDif = Constants.attDif)
        {
            double xx = Math.Pow(attDif, x / 10.0);
            double yy = Math.Pow(attDif, y / 10.0);

            return xx / (yy+xx);

        }

        public static double AttributeRatioCustom(params object[] param)
        {

            double xTotal = 1, yTotal = 1;

            if (param.Length == 0 || (param.Length %3 ) != 0)
                return -1;

            int i = 0;

            while (i < param.Length)
            {
                xTotal *= Math.Pow((double)param[i + 2], ((int) param[i])/10.0);
                yTotal *= Math.Pow((double)param[i + 2], ((int) param[i+1])/10.0);
                i += 3;
            }

            return xTotal / (yTotal + xTotal);
        }

    }
}