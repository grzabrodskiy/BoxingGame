﻿using System;
using System.Collections.Concurrent;
using Main;
using Newtonsoft.Json;
using System.Collections.Generic;
using FightSim;

namespace FighterRanking
{


    public static class FighterPopularity
    {
        // simple dictionaries to hold country and weight popularity coefficients
        // #todo: integrate with the rest of the program and persist the coefficients in XLM

        private static Dictionary<string, double> CountryCoefficient = new Dictionary<string, double>() { { "US", 1.1 }, { "Mexico", 2.0 } };

        private static Dictionary<int, double> WeightCoefficient = new Dictionary<int, double>() { { 147, 1.1 }, { 154, 2.0 } };

        // PUBLIC INTERFACE


        /// <summary>
        //  This method initializes Popularity metrics for a fighter f and updates it 
        //  between the fights. Thus the only input is the fighter object itself
        //
        //  NB: to update popularity as a result of a fight, use UpdatePopularity(FighterOutcome method)
        //
        //  NB: Please note that Elo metrics is being set by EloFighterRanking.CalculateRatingChange() method
        //  #todo: make sure the implementation is consistent between Elo and other metrics
        //
        /// </summary>
        public static void UpdatePopularity(Fighter f)
        {
            double fans = f.Performance["Fans"];
            double casuals = f.Performance["Casuals"];
            double followers = f.Performance["followers"];

            // do the calculatiuon
            if (fans == 0)
                fans = 1000;


            fans = fans + 1;
            // end of calculation


            f.Performance["Fans"] = fans;
        }


        /// <summary>
        //  The method update fighter popularity of both fighters as a result
        //  of FightOutcome object
        //
        //  at the end of the method, FighterOutcome viewership is set by the 
        //  result of a private FightViewers(FighterOutcome) method. No need to 
        //  to call FightViewers outside of this class.
        /// </summary>
        public static void UpdatePopularity(FightOutcome fo)
        {

            double fans1 = fo.Fighter1().Performance["Fans"];
            double fans2 = fo.Fighter2().Performance["Fans"];

            double casuals1 = fo.Fighter1().Performance["Casuals"];
            double casuals2 = fo.Fighter2().Performance["Casuals"];

            double followers1 = fo.Fighter1().Performance["Followers"];
            double followers2 = fo.Fighter2().Performance["Followers"];


            // do the calculation

            if (fo.Fighter1() == fo.Winner)
            {
             
                //winner
                fans1 =+ 0.18 * casuals1;
                casuals1 =- 0.18 * casuals1;


                casuals1 =+ 0.18 * fo.Interested;
                fo.Interested = -0.18 * fo.Interested;

                followers1 =+ 0.18 * casuals1;
                casuals1 = -0.18 * casuals1;

                //loser
                fans2 =- 0.1 * casuals2;
                casuals2 =+ 0.1 * casuals2;


                casuals2 =- 0.1 * fo.Interested;
                fo.Interested =+ 0.1 * fo.Interested;

                followers2 =- 0.1 * casuals2;
                casuals2 =+ 0.1 * casuals2;



                //fans1 *= 1.1;
                //fans2 *= 0.9;

            }
            else if (fo.Fighter2() == fo.Winner)
            {
                //fans1 *= 0.9;
                //fans2 *= 1.1;
            }


            // end of calculation

            fo.Fighter1().Performance["Fans"] = fans1;
            fo.Fighter2().Performance["Fans"] = fans2;

            fo.Viewership = FightViewers(fo);


        }

       
        // PRIVATE METHODS

        private static double FightViewers(FightOutcome fo)
        {
            double viewers;


            

            Fighter f1 = fo.Fighter1();
            Fighter f2 = fo.Fighter2();


            WeightClass w1 = (WeightClass)f1.Weight;


            fo.Interested = MathUtils.Gauss(((f1.Performance["Elo"] + f2.Performance["Elo"]) / 2), 100) *
                (CountryCoefficient[f1.Country] * CountryCoefficient[f2.Country] * WeightCoefficient[w1.Weight] + f1.Belts + f2.Belts);



            viewers = f1.Performance["Fans"] + f1.Performance["Followers"] + f1.Performance["Casuals"];
            viewers += (f1.Performance["Fans"] + f2.Performance["Followers"] + f2.Performance["Casuals"]);



            //double risk1 = 1 / Math.Abs(f1.Performance["Elo"] - f2.Performance["Elo"]);
            //double risk2 = 1 - risk1;

            viewers = fo.Interested + (PWin(fo))*f1.Performance["Base"] + (1-PWin(fo)*f2.Performance["Base"]);
            

            return viewers;



        }




        public static double PWin(FightOutcome fo)
        {
            return 1 / (1 + Math.Pow(10, fo.Fighter2().Performance["Elo"] - fo.Fighter1().Performance["Elo"] / 400));
        }


    }

    
}
